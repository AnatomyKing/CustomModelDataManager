using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuukMuschCustomModelManager.Model;

namespace LuukMuschCustomModelManager.Databases
{
    internal class CMDSeeder
    {
        public void SeedData()
        {
            using (AppDbContext db = new AppDbContext())
            {
                // Add Parent Items
                ParentItem[] parentItems = new ParentItem[]
                {
                    new ParentItem { Name = "rabbit.json", Type = "item" },
                    new ParentItem { Name = "note_block.json", Type = "block" }
                };
                db.ParentItems.AddRange(parentItems);
                db.SaveChanges();

                ParentItem rabbitItem = parentItems.First(p => p.Name == "rabbit.json");
                ParentItem noteBlockItem = parentItems.First(p => p.Name == "note_block.json");

                // Add Custom Model Data
                CustomModelData[] customModelDataArray = new CustomModelData[]
                {
                    new CustomModelData
                    {
                        Name = "banana_pearl",
                        CustomModelNumber = 27356,
                        ModelPath = "models/banana_pearl.json",
                        Status = true,
                        ParentItemID = rabbitItem.ParentItemID
                    },
                    new CustomModelData
                    {
                        Name = "magic_note",
                        CustomModelNumber = 12345,
                        ModelPath = "models/magic_note.json",
                        Status = true,
                        ParentItemID = noteBlockItem.ParentItemID
                    }
                };
                db.CustomModelDataItems.AddRange(customModelDataArray);

                // Add Block Types
                BlockType[] blockTypes = new BlockType[]
                {
                    new BlockType { Name = "NoteBlock" },
                    new BlockType { Name = "Tripwire" }
                };
                db.BlockTypes.AddRange(blockTypes);
                db.SaveChanges();

                BlockType noteBlockType = blockTypes.First(b => b.Name == "NoteBlock");
                BlockType tripwireType = blockTypes.First(b => b.Name == "Tripwire");

                // Retrieve CustomModelData entities now that IDs are generated
                CustomModelData bananaPearl = customModelDataArray.First(c => c.Name == "banana_pearl");
                CustomModelData magicNote = customModelDataArray.First(c => c.Name == "magic_note");

                // Add Custom Variations
                CustomVariation[] customVariations = new CustomVariation[]
                {
                    new CustomVariation
                    {
                        Variation = 40,
                        BlockData = "minecraft:note_block[instrument=snare,note=16,powered=false]",
                        CustomModelDataID = bananaPearl.CustomModelDataID,
                        BlockTypeID = noteBlockType.BlockTypeID
                    },
                    new CustomVariation
                    {
                        Variation = 50,
                        BlockData = "minecraft:tripwire[attached=true,disarmed=false]",
                        CustomModelDataID = magicNote.CustomModelDataID,
                        BlockTypeID = tripwireType.BlockTypeID
                    }
                };
                db.CustomVariations.AddRange(customVariations);

                // Add Shader Armor Color Infos
                ShaderArmorColorInfo[] shaderColors = new ShaderArmorColorInfo[]
                {
                    new ShaderArmorColorInfo
                    {
                        Name = "Mystic Blue",
                        HEX = "#1234AB",
                        RGB = "18,52,171",
                        Color = 1193131
                    },
                    new ShaderArmorColorInfo
                    {
                        Name = "Emerald Green",
                        HEX = "#00FF00",
                        RGB = "0,255,0",
                        Color = 65280
                    }
                };
                db.ShaderArmorColorInfos.AddRange(shaderColors);
                db.SaveChanges();

                ShaderArmorColorInfo mysticBlue = shaderColors.First(s => s.Name == "Mystic Blue");
                ShaderArmorColorInfo emeraldGreen = shaderColors.First(s => s.Name == "Emerald Green");

                // Add Shader Armor Relationships
                CustomModel_ShaderArmor[] shaderRelationships = new CustomModel_ShaderArmor[]
                {
                    new CustomModel_ShaderArmor
                    {
                        CustomModelDataID = bananaPearl.CustomModelDataID,
                        ShaderArmorColorInfoID = mysticBlue.ShaderArmorColorInfoID
                    },
                    new CustomModel_ShaderArmor
                    {
                        CustomModelDataID = magicNote.CustomModelDataID,
                        ShaderArmorColorInfoID = emeraldGreen.ShaderArmorColorInfoID
                    }
                };
                db.CustomModel_ShaderArmors.AddRange(shaderRelationships);
                db.SaveChanges();

                // Add CustomModel_BlockType Relationships
                // This links CustomModelData items to BlockTypes, similar to CustomModel_ShaderArmor
                CustomModel_BlockType[] customModelBlockTypes = new CustomModel_BlockType[]
                {
                    new CustomModel_BlockType
                    {
                        CustomModelDataID = bananaPearl.CustomModelDataID,
                        BlockTypeID = noteBlockType.BlockTypeID
                    },
                    new CustomModel_BlockType
                    {
                        CustomModelDataID = magicNote.CustomModelDataID,
                        BlockTypeID = tripwireType.BlockTypeID
                    }
                };
                db.AddRange(customModelBlockTypes);
                db.SaveChanges();

                Console.WriteLine("Database seeded successfully! ✔️");
            }
        }
    }
}