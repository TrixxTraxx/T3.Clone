using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LLMLab.Server.Migrations
{
    /// <inheritdoc />
    public partial class Encryption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM MessageAttachments");
            migrationBuilder.Sql("DELETE FROM Messages");
            migrationBuilder.Sql("DELETE FROM MessageThreads");
            migrationBuilder.Sql("DELETE FROM AiModelKeys");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
