using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProniaMVC.Migrations
{
    /// <inheritdoc />
    public partial class modifiedcode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ColorId",
                table: "ProductTags",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SizeId",
                table: "ProductTags",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProductTags_ColorId",
                table: "ProductTags",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTags_SizeId",
                table: "ProductTags",
                column: "SizeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductTags_Colors_ColorId",
                table: "ProductTags",
                column: "ColorId",
                principalTable: "Colors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductTags_Sizes_SizeId",
                table: "ProductTags",
                column: "SizeId",
                principalTable: "Sizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductTags_Colors_ColorId",
                table: "ProductTags");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductTags_Sizes_SizeId",
                table: "ProductTags");

            migrationBuilder.DropIndex(
                name: "IX_ProductTags_ColorId",
                table: "ProductTags");

            migrationBuilder.DropIndex(
                name: "IX_ProductTags_SizeId",
                table: "ProductTags");

            migrationBuilder.DropColumn(
                name: "ColorId",
                table: "ProductTags");

            migrationBuilder.DropColumn(
                name: "SizeId",
                table: "ProductTags");
        }
    }
}
