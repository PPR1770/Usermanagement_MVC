using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApp.Migrations
{
    /// <inheritdoc />
    public partial class AddLastSeenInApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOnline",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "LastSeenAt",
                table: "ChatMessages");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeenAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSeenAt",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<bool>(
                name: "IsOnline",
                table: "ChatMessages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeenAt",
                table: "ChatMessages",
                type: "datetime2",
                nullable: true);
        }
    }
}
