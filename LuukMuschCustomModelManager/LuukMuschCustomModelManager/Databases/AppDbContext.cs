using System.Configuration;
using Microsoft.EntityFrameworkCore;
using LuukMuschCustomModelManager.Model;

namespace LuukMuschCustomModelManager.Databases
{
    internal class AppDbContext : DbContext
    {
        public DbSet<ParentItem> ParentItems { get; set; }
        public DbSet<CustomModelData> CustomModelDataItems { get; set; }
        public DbSet<BlockType> BlockTypes { get; set; }
        public DbSet<CustomVariation> CustomVariations { get; set; }
        public DbSet<ShaderArmorColorInfo> ShaderArmorColorInfos { get; set; }
        public DbSet<CustomModel_BlockType> CustomModel_BlockTypes { get; set; }
        public DbSet<CustomModel_ShaderArmor> CustomModel_ShaderArmors { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connStr = ConfigurationManager.ConnectionStrings["MyConnStr"].ConnectionString;
                optionsBuilder.UseMySQL(connStr);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // CustomModel_BlockType many-to-many relationship
            modelBuilder.Entity<CustomModel_BlockType>()
                .HasKey(cb => new { cb.CustomModelDataID, cb.BlockTypeID });

            modelBuilder.Entity<CustomModel_BlockType>()
                .HasOne(cb => cb.CustomModelData)
                .WithMany(cd => cd.BlockTypes)
                .HasForeignKey(cb => cb.CustomModelDataID);

            modelBuilder.Entity<CustomModel_BlockType>()
                .HasOne(cb => cb.BlockType)
                .WithMany(bt => bt.CustomModelDataItems)
                .HasForeignKey(cb => cb.BlockTypeID);

            // CustomModel_ShaderArmor many-to-many relationship
            modelBuilder.Entity<CustomModel_ShaderArmor>()
                .HasKey(cs => new { cs.CustomModelDataID, cs.ShaderArmorColorInfoID });

            modelBuilder.Entity<CustomModel_ShaderArmor>()
                .HasOne(cs => cs.CustomModelData)
                .WithMany(cd => cd.ShaderArmors)
                .HasForeignKey(cs => cs.CustomModelDataID);

            modelBuilder.Entity<CustomModel_ShaderArmor>()
                .HasOne(cs => cs.ShaderArmorColorInfo)
                .WithMany(sa => sa.CustomModelDataItems)
                .HasForeignKey(cs => cs.ShaderArmorColorInfoID);

            // CustomVariation relationship
            modelBuilder.Entity<CustomVariation>()
                .HasOne(cv => cv.CustomModelData)
                .WithMany(cmd => cmd.CustomVariations)
                .HasForeignKey(cv => cv.CustomModelDataID);

            modelBuilder.Entity<CustomVariation>()
                .HasOne(cv => cv.BlockType)
                .WithMany(bt => bt.CustomVariations)
                .HasForeignKey(cv => cv.BlockTypeID);
        }


        public static void InitializeDatabase()
        {
            using (var context = new AppDbContext())
            {
                try
                {
                    // drop database
                    //context.Database.EnsureDeleted();

                    //create database
                    //context.Database.EnsureCreated();

                    context.Database.Migrate();  // Apply migrations
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Database initialization failed: {ex.Message}", ex);
                }
            }
        }
    }
}
