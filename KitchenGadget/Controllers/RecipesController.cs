using KitchenGadget.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KitchenGadget.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RecipesController : ControllerBase
    {
        private readonly RecipeContext _context;

        public RecipesController(RecipeContext context)
        {
            _context = context;
        }

        public IActionResult GetRecipes()
        {
            var r = _context.Recipes
                        .Include(c => c.Ingredients.OrderBy(i => i.Ord))
                        .Include(c => c.Instructions.OrderBy(i => i.Ord));
            return Ok(new
            {
                success = true, 
                items = r
            });
        }
    }
}
