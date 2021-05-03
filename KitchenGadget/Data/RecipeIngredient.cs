using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace KitchenGadget.Data
{
    [Table("recipe_ingredients")]
    public class RecipeIngredient
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("recipe_id")]
        public int RecipeId { get; set; }

        [Column("amount")]
        public int Amount { get; set; }

        [Column("measure"), MaxLength(150)]
        public string Measure { get; set; }

        [Column("name"), Required, MaxLength(150)]
        public string Name { get; set; }

        [Column("ord"), Required]
        public int Ord { get; set; }
    }
}
