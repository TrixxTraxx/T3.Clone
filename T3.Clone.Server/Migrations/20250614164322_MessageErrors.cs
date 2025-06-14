using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace T3.Clone.Server.Migrations
{
    /// <inheritdoc />
    public partial class MessageErrors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Error",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Error",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "Messages");
        }
    }
}
