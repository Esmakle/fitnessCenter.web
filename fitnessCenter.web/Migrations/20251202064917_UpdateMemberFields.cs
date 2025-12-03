using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fitnessCenter.web.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMemberFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cinsiyet",
                table: "Members",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DogumTarihi",
                table: "Members",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "KayitTarihi",
                table: "Members",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cinsiyet",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "DogumTarihi",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "KayitTarihi",
                table: "Members");
        }
    }
}
