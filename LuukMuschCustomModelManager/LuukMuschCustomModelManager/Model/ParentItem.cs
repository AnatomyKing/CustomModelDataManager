using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuukMuschCustomModelManager.Helpers;

namespace LuukMuschCustomModelManager.Model
{
    [Table("ParentItems")]
    internal class ParentItem
    {
        [Key]
        public int ParentItemID { get; set; }

        [Required, MaxLength(100)] // Name of the JSON file, e.g., rabbit.json
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(50)] // Type of item, like block or item
        public string Type { get; set; } = string.Empty;

        public ICollection<CustomModelData> CustomModelDataItems { get; set; } = new List<CustomModelData>();
    }
}
