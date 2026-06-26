using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Stats.Core.Migrations
{
    /// <inheritdoc />
    public partial class PredictionReasons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Reasons",
                table: "Predictions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reasons",
                table: "Predictions");
        }
    }
}
