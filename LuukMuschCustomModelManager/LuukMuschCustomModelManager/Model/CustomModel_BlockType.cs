using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuukMuschCustomModelManager.Model
{
    [Table("CustomModel_BlockType")]
    internal class CustomModel_BlockType
    {
        [ForeignKey("CustomModelData")]
        public int CustomModelDataID { get; set; }
        public CustomModelData? CustomModelData { get; set; }

        [ForeignKey("BlockType")]
        public int BlockTypeID { get; set; }
        public BlockType? BlockType { get; set; }
    }
}
