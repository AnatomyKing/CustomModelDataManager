using LuukMuschCustomModelManager.Databases;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuukMuschCustomModelManager.Migrations
{
    /// <inheritdoc />
    public partial class TestData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            new CMDSeeder().SeedData();
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
