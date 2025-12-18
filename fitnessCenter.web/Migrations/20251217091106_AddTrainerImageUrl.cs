using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fitnessCenter.web.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainerImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_AspNetUsers_IdentityUserId",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Members_IdentityUserId",
                table: "Members");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Trainers",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IdentityUserId",
                table: "Members",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Trainers");

            migrationBuilder.AlterColumn<string>(
                name: "IdentityUserId",
                table: "Members",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_IdentityUserId",
                table: "Members",
                column: "IdentityUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Members_AspNetUsers_IdentityUserId",
                table: "Members",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
