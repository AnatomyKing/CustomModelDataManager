using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuukMuschCustomModelManager.Migrations
{
    /// <inheritdoc />
    public partial class ManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomModelDataItems_ParentItems_ParentItemID",
                table: "CustomModelDataItems");

            migrationBuilder.DropIndex(
                name: "IX_CustomModelDataItems_ParentItemID",
                table: "CustomModelDataItems");

            migrationBuilder.DropColumn(
                name: "ParentItemID",
                table: "CustomModelDataItems");

            migrationBuilder.CreateTable(
                name: "CustomModelData_ParentItem",
                columns: table => new
                {
                    CustomModelDataID = table.Column<int>(type: "int", nullable: false),
                    ParentItemID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomModelData_ParentItem", x => new { x.CustomModelDataID, x.ParentItemID });
                    table.ForeignKey(
                        name: "FK_CustomModelData_ParentItem_CustomModelDataItems_CustomModelD~",
                        column: x => x.CustomModelDataID,
                        principalTable: "CustomModelDataItems",
                        principalColumn: "CustomModelDataID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomModelData_ParentItem_ParentItems_ParentItemID",
                        column: x => x.ParentItemID,
                        principalTable: "ParentItems",
                        principalColumn: "ParentItemID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CustomModelData_ParentItem_ParentItemID",
                table: "CustomModelData_ParentItem",
                column: "ParentItemID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomModelData_ParentItem");

            migrationBuilder.AddColumn<int>(
                name: "ParentItemID",
                table: "CustomModelDataItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CustomModelDataItems_ParentItemID",
                table: "CustomModelDataItems",
                column: "ParentItemID");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomModelDataItems_ParentItems_ParentItemID",
                table: "CustomModelDataItems",
                column: "ParentItemID",
                principalTable: "ParentItems",
                principalColumn: "ParentItemID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
