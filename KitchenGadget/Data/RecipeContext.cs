using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KitchenGadget.Data
{
    public class RecipeContext : DbContext
    {
        public DbSet<Recipe> Recipes { get; set; }

        public RecipeContext(DbContextOptions<RecipeContext> options) : base(options) { }
    }
}
