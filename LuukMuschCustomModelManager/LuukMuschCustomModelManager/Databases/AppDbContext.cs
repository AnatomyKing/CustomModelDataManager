using System.Configuration;
using Microsoft.EntityFrameworkCore;
using LuukMuschCustomModelManager.Model;
using System.Diagnostics;
using MySql.Data.MySqlClient;

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

            // MANY-TO-MANY: Relationship between CustomModelData and ParentItem.
            modelBuilder.Entity<CustomModelData>()
                .HasMany(cmd => cmd.ParentItems)
                .WithMany(pi => pi.CustomModelDataItems)
                .UsingEntity<Dictionary<string, object>>(
                    "CustomModelData_ParentItem",
                    j => j.HasOne<ParentItem>().WithMany().HasForeignKey("ParentItemID"),
                    j => j.HasOne<CustomModelData>().WithMany().HasForeignKey("CustomModelDataID"),
                    j =>
                    {
                        j.HasKey("CustomModelDataID", "ParentItemID");
                        j.ToTable("CustomModelData_ParentItem");
                    }
                );
        }

        public static void InitializeDatabase()
        {
            using var context = new AppDbContext();
            int retryCount = 5;

            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    // Attempt to open connection and apply migrations.
                    var connection = context.Database.GetDbConnection();
                    connection.Open();
                    context.Database.Migrate(); // Will fail if DB doesn't exist
                    connection.Close();

                    // After successful migration, seed data
                    CMDSeeder seeder = new CMDSeeder();
                    seeder.SeedData();

                    // If everything worked, we can return
                    return;
                }
                catch (MySqlException ex) when (ex.Number == 1049)
                {
                    // #1049 = "Unknown database 'cmddb'"
                    Debug.WriteLine("Database does not exist. Creating it now...");

                    // Create the DB, then loop again so context.Database.Migrate() will succeed next time
                    CreateDatabaseIfNotExists("cmddb");
                }
                catch (MySqlException ex) when (ex.Number == 1042 || ex.Number == 2006 || ex.Number == 2013)
                {
                    // MySQL connection lost, try again (sleep briefly)
                    Debug.WriteLine($"MySQL connection lost. Retrying ({i + 1}/{retryCount})...");
                    Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    // Non-recoverable error
                    Debug.WriteLine($"Database initialization failed: {ex.Message}");
                    throw;
                }
            }

            // If we still fail after multiple tries, throw
            throw new Exception("Could not connect or create the database after multiple attempts.");
        }

        /// <summary>
        /// Creates the given database if it does not exist.
        /// </summary>
        private static void CreateDatabaseIfNotExists(string dbName)
        {
            // Rebuild a connection string that DOES NOT specify the 'cmddb' database,
            // so that we can create the DB. You can read these settings from config.
            string noDbConnStr = "server=localhost;user=root;pwd=;port=3306;" +
                                 "Pooling=false;Connect Timeout=60;Default Command Timeout=60;";

            using var con = new MySqlConnection(noDbConnStr);
            con.Open();

            // Create DB if missing
            string sql = $"CREATE DATABASE IF NOT EXISTS `{dbName}`;";
            using var cmd = new MySqlCommand(sql, con);
            cmd.ExecuteNonQuery();

            con.Close();
        }
    }
}