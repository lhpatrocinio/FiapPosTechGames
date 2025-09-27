using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFree",
                table: "GMS_Games",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "GMS_Library",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdUser = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GMS_Library", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GMS_GamesLibrary",
                columns: table => new
                {
                    IdLibrary = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdGame = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GMS_GamesLibrary", x => new { x.IdLibrary, x.IdGame });
                    table.ForeignKey(
                        name: "FK_GMS_GamesLibrary_GMS_Games_IdGame",
                        column: x => x.IdGame,
                        principalTable: "GMS_Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GMS_GamesLibrary_GMS_Library_IdLibrary",
                        column: x => x.IdLibrary,
                        principalTable: "GMS_Library",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GMS_GamesLibrary_IdGame",
                table: "GMS_GamesLibrary",
                column: "IdGame");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GMS_GamesLibrary");

            migrationBuilder.DropTable(
                name: "GMS_Library");

            migrationBuilder.DropColumn(
                name: "IsFree",
                table: "GMS_Games");
        }
    }
}
