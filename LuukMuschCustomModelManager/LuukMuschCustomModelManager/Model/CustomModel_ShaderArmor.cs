using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuukMuschCustomModelManager.Model
{
    [Table("CustomModel_ShaderArmor")]
    internal class CustomModel_ShaderArmor
    {
        [ForeignKey("CustomModelData")]
        public int CustomModelDataID { get; set; }
        public CustomModelData? CustomModelData { get; set; }

        [ForeignKey("ShaderArmorColorInfo")]
        public int ShaderArmorColorInfoID { get; set; }
        public ShaderArmorColorInfo? ShaderArmorColorInfo { get; set; }
    }
}
