using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Stats.Core.Migrations
{
    /// <inheritdoc />
    public partial class PredictionPhase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Predictions_Year_Round_DriverId",
                table: "Predictions");

            migrationBuilder.AddColumn<string>(
                name: "Phase",
                table: "Predictions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_Year_Round_DriverId_Phase",
                table: "Predictions",
                columns: new[] { "Year", "Round", "DriverId", "Phase" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Predictions_Year_Round_DriverId_Phase",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "Phase",
                table: "Predictions");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_Year_Round_DriverId",
                table: "Predictions",
                columns: new[] { "Year", "Round", "DriverId" },
                unique: true);
        }
    }
}
