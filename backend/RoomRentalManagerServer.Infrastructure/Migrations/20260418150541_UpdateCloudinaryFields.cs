using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomRentalManagerServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCloudinaryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image",
                table: "image_descriptions");

            migrationBuilder.RenameColumn(
                name: "imageType",
                table: "image_descriptions",
                newName: "publicId");

            migrationBuilder.AddColumn<string>(
                name: "avatarPublicId",
                table: "user",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "imageUrl",
                table: "image_descriptions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "avatarPublicId",
                table: "user");

            migrationBuilder.DropColumn(
                name: "imageUrl",
                table: "image_descriptions");

            migrationBuilder.RenameColumn(
                name: "publicId",
                table: "image_descriptions",
                newName: "imageType");

            migrationBuilder.AddColumn<byte[]>(
                name: "image",
                table: "image_descriptions",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
