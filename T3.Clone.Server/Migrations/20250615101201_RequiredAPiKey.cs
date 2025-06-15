using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace T3.Clone.Server.Migrations
{
    /// <inheritdoc />
    public partial class RequiredAPiKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequiredApiKey",
                table: "AiModels",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiredApiKey",
                table: "AiModels");
        }
    }
}
