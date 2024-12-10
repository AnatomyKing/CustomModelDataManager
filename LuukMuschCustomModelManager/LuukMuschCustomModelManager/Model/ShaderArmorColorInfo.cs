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
    [Table("ShaderArmorColorInfos")]
    internal class ShaderArmorColorInfo
    {
        [Key]
        public int ShaderArmorColorInfoID { get; set; }

        [Required, MaxLength(100)] // Name of the shader armor info
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(7)] // HEX color, formatted as #RRGGBB
        public string HEX { get; set; } = string.Empty;

        [Required, MaxLength(11)] // RGB color as "R,G,B"
        public string RGB { get; set; } = string.Empty;

        [Required] // Decimal representation of the color
        public int Color { get; set; }

        // Navigation property for many-to-many relationship
        public ICollection<CustomModel_ShaderArmor> CustomModelDataItems { get; set; } = new List<CustomModel_ShaderArmor>();
    }
}
