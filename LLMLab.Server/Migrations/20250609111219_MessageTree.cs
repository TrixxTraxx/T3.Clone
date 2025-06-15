using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace T3.Clone.Server.Migrations
{
    /// <inheritdoc />
    public partial class MessageTree : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PreviousMessageId",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviousMessageId",
                table: "Messages");
        }
    }
}
