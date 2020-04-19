using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

namespace ObligatorioDA2.Data.DataAccess.Migrations
{
    public partial class renaming : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Matches_EncounterEntityId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Sports_SportEntityName",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchTeams_Matches_MatchId",
                table: "MatchTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchTeams_Teams_TeamNumber",
                table: "MatchTeams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MatchTeams",
                table: "MatchTeams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Matches",
                table: "Matches");

            migrationBuilder.RenameTable(
                name: "MatchTeams",
                newName: "EncounterTeams");

            migrationBuilder.RenameTable(
                name: "Matches",
                newName: "Encounters");

            migrationBuilder.RenameColumn(
                name: "MatchId",
                table: "EncounterTeams",
                newName: "EncounterId");

            migrationBuilder.RenameIndex(
                name: "IX_MatchTeams_TeamNumber",
                table: "EncounterTeams",
                newName: "IX_EncounterTeams_TeamNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Matches_SportEntityName",
                table: "Encounters",
                newName: "IX_Encounters_SportEntityName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EncounterTeams",
                table: "EncounterTeams",
                columns: new[] { "EncounterId", "TeamNumber" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Encounters",
                table: "Encounters",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Encounters_EncounterEntityId",
                table: "Comments",
                column: "EncounterEntityId",
                principalTable: "Encounters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Encounters_Sports_SportEntityName",
                table: "Encounters",
                column: "SportEntityName",
                principalTable: "Sports",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EncounterTeams_Encounters_EncounterId",
                table: "EncounterTeams",
                column: "EncounterId",
                principalTable: "Encounters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EncounterTeams_Teams_TeamNumber",
                table: "EncounterTeams",
                column: "TeamNumber",
                principalTable: "Teams",
                principalColumn: "TeamNumber",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Encounters_EncounterEntityId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Encounters_Sports_SportEntityName",
                table: "Encounters");

            migrationBuilder.DropForeignKey(
                name: "FK_EncounterTeams_Encounters_EncounterId",
                table: "EncounterTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_EncounterTeams_Teams_TeamNumber",
                table: "EncounterTeams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EncounterTeams",
                table: "EncounterTeams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Encounters",
                table: "Encounters");

            migrationBuilder.RenameTable(
                name: "EncounterTeams",
                newName: "MatchTeams");

            migrationBuilder.RenameTable(
                name: "Encounters",
                newName: "Matches");

            migrationBuilder.RenameColumn(
                name: "EncounterId",
                table: "MatchTeams",
                newName: "MatchId");

            migrationBuilder.RenameIndex(
                name: "IX_EncounterTeams_TeamNumber",
                table: "MatchTeams",
                newName: "IX_MatchTeams_TeamNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Encounters_SportEntityName",
                table: "Matches",
                newName: "IX_Matches_SportEntityName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MatchTeams",
                table: "MatchTeams",
                columns: new[] { "MatchId", "TeamNumber" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Matches",
                table: "Matches",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Matches_EncounterEntityId",
                table: "Comments",
                column: "EncounterEntityId",
                principalTable: "Matches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Sports_SportEntityName",
                table: "Matches",
                column: "SportEntityName",
                principalTable: "Sports",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchTeams_Matches_MatchId",
                table: "MatchTeams",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchTeams_Teams_TeamNumber",
                table: "MatchTeams",
                column: "TeamNumber",
                principalTable: "Teams",
                principalColumn: "TeamNumber",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
