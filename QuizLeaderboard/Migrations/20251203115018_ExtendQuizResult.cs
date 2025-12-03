using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizLeaderboard.Migrations
{
    /// <inheritdoc />
    public partial class ExtendQuizResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Difficulty",
                table: "QuizResults",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Mode",
                table: "QuizResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Topic",
                table: "QuizResults",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "QuizResults");

            migrationBuilder.DropColumn(
                name: "Mode",
                table: "QuizResults");

            migrationBuilder.DropColumn(
                name: "Topic",
                table: "QuizResults");
        }
    }
}
