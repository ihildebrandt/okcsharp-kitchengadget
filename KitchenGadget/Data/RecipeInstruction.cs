using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace KitchenGadget.Data
{
    [Table("recipe_instructions")]
    public class RecipeInstruction
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("recipe_id")]
        public int RecipeId { get; set; }

        [Column("text"), Required, MaxLength(1000)]
        public string Text { get; set; }

        [Column("ord"), Required]
        public int Ord { get; set; }
    }
}
