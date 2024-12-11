using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace LuukMuschCustomModelManager.Migrations
{
    /// <inheritdoc />
    public partial class CMDdatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BlockTypes",
                columns: table => new
                {
                    BlockTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockTypes", x => x.BlockTypeID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ParentItems",
                columns: table => new
                {
                    ParentItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentItems", x => x.ParentItemID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ShaderArmorColorInfos",
                columns: table => new
                {
                    ShaderArmorColorInfoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    HEX = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false),
                    RGB = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: false),
                    Color = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShaderArmorColorInfos", x => x.ShaderArmorColorInfoID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CustomModelDataItems",
                columns: table => new
                {
                    CustomModelDataID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    CustomModelNumber = table.Column<int>(type: "int", nullable: false),
                    ModelPath = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Status = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ParentItemID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomModelDataItems", x => x.CustomModelDataID);
                    table.ForeignKey(
                        name: "FK_CustomModelDataItems_ParentItems_ParentItemID",
                        column: x => x.ParentItemID,
                        principalTable: "ParentItems",
                        principalColumn: "ParentItemID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CustomModel_BlockType",
                columns: table => new
                {
                    CustomModelDataID = table.Column<int>(type: "int", nullable: false),
                    BlockTypeID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomModel_BlockType", x => new { x.CustomModelDataID, x.BlockTypeID });
                    table.ForeignKey(
                        name: "FK_CustomModel_BlockType_BlockTypes_BlockTypeID",
                        column: x => x.BlockTypeID,
                        principalTable: "BlockTypes",
                        principalColumn: "BlockTypeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomModel_BlockType_CustomModelDataItems_CustomModelDataID",
                        column: x => x.CustomModelDataID,
                        principalTable: "CustomModelDataItems",
                        principalColumn: "CustomModelDataID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CustomModel_ShaderArmor",
                columns: table => new
                {
                    CustomModelDataID = table.Column<int>(type: "int", nullable: false),
                    ShaderArmorColorInfoID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomModel_ShaderArmor", x => new { x.CustomModelDataID, x.ShaderArmorColorInfoID });
                    table.ForeignKey(
                        name: "FK_CustomModel_ShaderArmor_CustomModelDataItems_CustomModelData~",
                        column: x => x.CustomModelDataID,
                        principalTable: "CustomModelDataItems",
                        principalColumn: "CustomModelDataID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomModel_ShaderArmor_ShaderArmorColorInfos_ShaderArmorCol~",
                        column: x => x.ShaderArmorColorInfoID,
                        principalTable: "ShaderArmorColorInfos",
                        principalColumn: "ShaderArmorColorInfoID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CustomVariations",
                columns: table => new
                {
                    CustomVariationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Variation = table.Column<int>(type: "int", nullable: false),
                    BlockData = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    CustomModelDataID = table.Column<int>(type: "int", nullable: false),
                    BlockTypeID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomVariations", x => x.CustomVariationID);
                    table.ForeignKey(
                        name: "FK_CustomVariations_BlockTypes_BlockTypeID",
                        column: x => x.BlockTypeID,
                        principalTable: "BlockTypes",
                        principalColumn: "BlockTypeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomVariations_CustomModelDataItems_CustomModelDataID",
                        column: x => x.CustomModelDataID,
                        principalTable: "CustomModelDataItems",
                        principalColumn: "CustomModelDataID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CustomModel_BlockType_BlockTypeID",
                table: "CustomModel_BlockType",
                column: "BlockTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomModel_ShaderArmor_ShaderArmorColorInfoID",
                table: "CustomModel_ShaderArmor",
                column: "ShaderArmorColorInfoID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomModelDataItems_ParentItemID",
                table: "CustomModelDataItems",
                column: "ParentItemID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomVariations_BlockTypeID",
                table: "CustomVariations",
                column: "BlockTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomVariations_CustomModelDataID",
                table: "CustomVariations",
                column: "CustomModelDataID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomModel_BlockType");

            migrationBuilder.DropTable(
                name: "CustomModel_ShaderArmor");

            migrationBuilder.DropTable(
                name: "CustomVariations");

            migrationBuilder.DropTable(
                name: "ShaderArmorColorInfos");

            migrationBuilder.DropTable(
                name: "BlockTypes");

            migrationBuilder.DropTable(
                name: "CustomModelDataItems");

            migrationBuilder.DropTable(
                name: "ParentItems");
        }
    }
}
