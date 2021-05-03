using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace KitchenGadget.Data
{
    [Table("recipes")]
    public class Recipe
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name"), Required, MaxLength(150)]
        public string Name { get; set; }

        [ForeignKey("RecipeId")]
        public ICollection<RecipeIngredient> Ingredients { get; set; }

        [ForeignKey("RecipeId")]
        public ICollection<RecipeInstruction> Instructions { get; set; }
    }
}
