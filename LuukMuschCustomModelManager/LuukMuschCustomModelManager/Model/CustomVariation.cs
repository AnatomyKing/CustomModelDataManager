using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuukMuschCustomModelManager.Model
{
    [Table("CustomVariations")]
    internal class CustomVariation
    {
        [Key]
        public int CustomVariationID { get; set; }

        [Required] // Custom variation number, e.g., 40
        public int Variation { get; set; }

        [Required, MaxLength(255)] // Block data, e.g., minecraft:note_block[instrument=snare,note=16,powered=false]
        public string BlockData { get; set; } = string.Empty;

        [ForeignKey("CustomModelData")]
        public int CustomModelDataID { get; set; }
        public CustomModelData? CustomModelData { get; set; }

        [ForeignKey("BlockType")]
        public int BlockTypeID { get; set; }
        public BlockType? BlockType { get; set; }
    }
}