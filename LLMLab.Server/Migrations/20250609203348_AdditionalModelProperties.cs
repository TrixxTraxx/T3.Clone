using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace T3.Clone.Server.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalModelProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasImageGenerationSupport",
                table: "AiModels",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasImageSupport",
                table: "AiModels",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasThinkingSupport",
                table: "AiModels",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "ImageGenerationCost",
                table: "AiModels",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "InputTokenCost",
                table: "AiModels",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "AiModels",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxInputTokens",
                table: "AiModels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxOutputTokens",
                table: "AiModels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "OutputTokenCost",
                table: "AiModels",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "SupportedContentTypes",
                table: "AiModels",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "ThinkingCost",
                table: "AiModels",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasImageGenerationSupport",
                table: "AiModels");

            migrationBuilder.DropColumn(
                name: "HasImageSupport",
                table: "AiModels");

            migrationBuilder.DropColumn(
                name: "HasThinkingSupport",
                table: "AiModels");

            migrationBuilder.DropColumn(
                name: "ImageGenerationCost",
                table: "AiModels");

            migrationBuilder.DropColumn(
                name: "InputTokenCost",
                table: "AiModels");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "AiModels");

            migrationBuilder.DropColumn(
                name: "MaxInputTokens",
                table: "AiModels");

            migrationBuilder.DropColumn(
                name: "MaxOutputTokens",
                table: "AiModels");

            migrationBuilder.DropColumn(
                name: "OutputTokenCost",
                table: "AiModels");

            migrationBuilder.DropColumn(
                name: "SupportedContentTypes",
                table: "AiModels");

            migrationBuilder.DropColumn(
                name: "ThinkingCost",
                table: "AiModels");
        }
    }
}
