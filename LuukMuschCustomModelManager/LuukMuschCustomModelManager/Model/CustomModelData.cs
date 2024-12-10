using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuukMuschCustomModelManager.Helpers;

namespace LuukMuschCustomModelManager.Model
{
    [Table("CustomModelDataItems")]
    internal class CustomModelData
    {
        [Key]
        public int CustomModelDataID { get; set; }

        [Required, MaxLength(100)] // Name of item, like banana_pearl
        public string Name { get; set; } = string.Empty;

        [Required] // Custom model data number, e.g., 27356
        public int CustomModelNumber { get; set; }

        [Required, MaxLength(255)] // Path to model, max length 255
        public string ModelPath { get; set; } = string.Empty;

        [Required] // Boolean flag for used/unused status
        public bool Status { get; set; }

        [ForeignKey("ParentItem")] // Foreign key to ParentItem
        public int ParentItemID { get; set; }
        public ParentItem? ParentItem { get; set; }

        public ICollection<CustomModel_BlockType> BlockTypes { get; set; } = new List<CustomModel_BlockType>();
        public ICollection<CustomModel_ShaderArmor> ShaderArmors { get; set; } = new List<CustomModel_ShaderArmor>();
        public ICollection<CustomVariation> CustomVariations { get; set; } = new List<CustomVariation>();

    }
}
