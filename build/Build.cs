using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.ControlFlow;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Docker.DockerTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Threading;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    class DatabaseConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
    }

    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    private const string DockerAtom = "KitchenGadget";

    public static int Main () => Execute<Build>(x => x.CompileSolution);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [PathExecutable] readonly Tool Liquibase;

    private IConfiguration Config { get; set; }
    private bool DevelopmentEnvironmentIsRunning = false;

    AbsolutePath OutputDirectory => RootDirectory / "output";

    Target CleanSolution => _ => _
        .Before(RestoreSolution)
        .Executes(() =>
        {
            EnsureCleanDirectory(OutputDirectory);
        });

    Target RestoreSolution => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target CompileSolution => _ => _
        .DependsOn(RestoreSolution)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    // Local Development
    Target InitConfig => _ => _
        .Executes(() =>
        {
            var builder = new ConfigurationBuilder();
            if (IsLocalBuild)
            {
                builder.AddUserSecrets<Build>();
            }
            Config = builder.Build();
        });

    Target CleanDevelopmentContainers => _ => _
        .After(CompileSolution)
        .Executes(() =>
        {
            var runningContainers = DockerPs(s => s.SetFilter($"label=atom={DockerAtom}").SetQuiet(true));
            if (runningContainers.Any())
            {
                DockerKill(s => s.AddContainers(runningContainers.Select(c => c.Text)));
            }

            var containers = DockerPs(s => s.SetFilter($"label=atom={DockerAtom}").SetQuiet(true).EnableAll());
            if (containers.Any())
            {
                DockerRm(s => s.AddContainers(containers.Select(c => c.Text)));
            }
        });

    Target StartDevelopmentContainers => _ => _
        .DependsOn(InitConfig)
        .DependsOn(CleanDevelopmentContainers)
        .Executes(() =>
        {
            var dbConfig = new DatabaseConfig();
            Config.Bind("Database", dbConfig);

            DockerRun(s => s
                .SetName("kitchengadget_postgres")
                .AddLabel($"atom={DockerAtom}")
                .AddEnv($"POSTGRES_USER={dbConfig.Username}")
                .AddEnv($"POSTGRES_PASSWORD={dbConfig.Password}")
                .AddEnv($"POSTGRES_DB={dbConfig.Database}")
                .SetPublish($"{dbConfig.Port}:5432")
                .SetDetach(true)
                .SetImage("postgres"));
        });

    Target MigrateDatabase => _ => _
        .DependsOn(InitConfig)
        .DependsOn(StartDevelopmentContainers)
        .Executes(() =>
        {
            var dbConfig = new DatabaseConfig();
            Config.Bind("Database", dbConfig);

            var url = $"jdbc:postgresql://{dbConfig.Host}:{dbConfig.Port}/{dbConfig.Database}";
            ExecuteWithRetry(() =>
            {
                Liquibase($"--url=\"{url}\" --username=\"{dbConfig.Username}\" --password=\"{dbConfig.Password}\" update");
            }, waitInSeconds: 2);
        });

    Target StartApi => _ => _
        .DependsOn(MigrateDatabase)
        .DependsOn(CompileSolution)
        .Executes(() =>
        {
            Task.Run(() =>
            {
                Thread.Sleep(100);
                DotNet("watch run", "KitchenGadget");
            });
        });

    Target StartApp => _ => _
        .Executes(() =>
        {
            Task.Run(() =>
            {
                Npm("start", "kitchengadget-app");
            });
        });

    Target StartDevelopmentContainerLogs => _ => _
        .DependsOn(StartDevelopmentContainers)
        .Before(StartDevelopmentEnvironment)
        .After(MigrateDatabase)
        .Executes(() =>
        {
            Task.Run(() =>
            {
                Thread.Sleep(100);
                DockerLogs(s => s.SetContainer("kitchengadget_postgres").EnableFollow());
            });
        });

    Target StartDevelopmentEnvironment => _ => _
        .DependsOn(StartApi)
        .DependsOn(StartApp)
        .DependsOn(StartDevelopmentContainerLogs)
        .Executes(() =>
        {
            DevelopmentEnvironmentIsRunning = true;

            Console.CancelKeyPress += (s, a) =>
            {
                // kill dotnet watch
                // kill react
                a.Cancel = true;
                DevelopmentEnvironmentIsRunning = false;
            };

            while (DevelopmentEnvironmentIsRunning) 
                Thread.Sleep(10);
        });
}
