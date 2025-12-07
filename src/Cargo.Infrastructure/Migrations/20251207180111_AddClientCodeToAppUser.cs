using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClientCodeToAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientCode",
                table: "AspNetUsers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ClientCode",
                table: "AspNetUsers",
                column: "ClientCode",
                unique: true,
                filter: "\"ClientCode\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ClientCode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ClientCode",
                table: "AspNetUsers");
        }
    }
}
