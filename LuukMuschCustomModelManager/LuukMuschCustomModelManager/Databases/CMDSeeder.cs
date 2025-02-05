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
                // Seed the "unused" ParentItem if none exists.
                // When the database is empty, this first inserted ParentItem will have ID 1.
                if (!db.ParentItems.Any())
                {
                    var unusedParentItem = new ParentItem
                    {
                        Name = "unused",
                        Type = "unused"
                    };
                    db.ParentItems.Add(unusedParentItem);
                    db.SaveChanges();
                }

                // Seed the BlockTypes if none exist.
                if (!db.BlockTypes.Any())
                {
                    BlockType[] blockTypes = new BlockType[]
                    {
                        new BlockType { Name = "NOTE_BLOCK" },
                        new BlockType { Name = "CHORUS_PLANT" },
                        new BlockType { Name = "TRIPWIRE" }
                    };

                    db.BlockTypes.AddRange(blockTypes);
                    db.SaveChanges();
                }

                Console.WriteLine("Database seeded successfully with the unused ParentItem and BlockTypes!");
            }
        }
    }
}