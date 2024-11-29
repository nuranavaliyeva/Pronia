using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProniaMVC.Migrations
{
    /// <inheritdoc />
    public partial class modified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_ProductImages_ProductImageId",
                table: "ProductImages");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductTags_ProductImages_ProductImageId",
                table: "ProductTags");

            migrationBuilder.DropIndex(
                name: "IX_ProductTags_ProductImageId",
                table: "ProductTags");

            migrationBuilder.DropColumn(
                name: "ProductImageId",
                table: "ProductTags");

            migrationBuilder.RenameColumn(
                name: "ProductImageId",
                table: "ProductImages",
                newName: "TagId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImages_ProductImageId",
                table: "ProductImages",
                newName: "IX_ProductImages_TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_Tags_TagId",
                table: "ProductImages",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_Tags_TagId",
                table: "ProductImages");

            migrationBuilder.RenameColumn(
                name: "TagId",
                table: "ProductImages",
                newName: "ProductImageId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImages_TagId",
                table: "ProductImages",
                newName: "IX_ProductImages_ProductImageId");

            migrationBuilder.AddColumn<int>(
                name: "ProductImageId",
                table: "ProductTags",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductTags_ProductImageId",
                table: "ProductTags",
                column: "ProductImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_ProductImages_ProductImageId",
                table: "ProductImages",
                column: "ProductImageId",
                principalTable: "ProductImages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductTags_ProductImages_ProductImageId",
                table: "ProductTags",
                column: "ProductImageId",
                principalTable: "ProductImages",
                principalColumn: "Id");
        }
    }
}
