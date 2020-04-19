using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

namespace ObligatorioDA2.Data.DataAccess.Migrations
{
    public partial class RenameMatchToEncounter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Matches_MatchEntityId",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "MatchEntityId",
                table: "Comments",
                newName: "EncounterEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_MatchEntityId",
                table: "Comments",
                newName: "IX_Comments_EncounterEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Matches_EncounterEntityId",
                table: "Comments",
                column: "EncounterEntityId",
                principalTable: "Matches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Matches_EncounterEntityId",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "EncounterEntityId",
                table: "Comments",
                newName: "MatchEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_EncounterEntityId",
                table: "Comments",
                newName: "IX_Comments_MatchEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Matches_MatchEntityId",
                table: "Comments",
                column: "MatchEntityId",
                principalTable: "Matches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
