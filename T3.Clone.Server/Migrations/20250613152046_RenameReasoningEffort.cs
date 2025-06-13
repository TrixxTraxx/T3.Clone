using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace T3.Clone.Server.Migrations
{
    /// <inheritdoc />
    public partial class RenameReasoningEffort : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReasoningEffort",
                table: "Messages",
                newName: "ReasoningEffortLevel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReasoningEffortLevel",
                table: "Messages",
                newName: "ReasoningEffort");
        }
    }
}
