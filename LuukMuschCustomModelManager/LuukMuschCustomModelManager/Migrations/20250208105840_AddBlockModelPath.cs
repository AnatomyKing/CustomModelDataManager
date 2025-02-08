using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuukMuschCustomModelManager.Migrations
{
    /// <inheritdoc />
    public partial class AddBlockModelPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlockModelPath",
                table: "CustomVariations",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockModelPath",
                table: "CustomVariations");
        }
    }
}
