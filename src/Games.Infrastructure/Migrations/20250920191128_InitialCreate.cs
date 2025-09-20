using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GMS_Games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Developer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IndicatedAgeRating = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HourPlayed = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GMS_Games", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GMS_GenreTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GMS_GenreTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GMS_GameGenres",
                columns: table => new
                {
                    IdGenre = table.Column<int>(type: "int", nullable: false),
                    IdGame = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GMS_GameGenres", x => new { x.IdGame, x.IdGenre });
                    table.ForeignKey(
                        name: "FK_GMS_GameGenres_GMS_Games_IdGame",
                        column: x => x.IdGame,
                        principalTable: "GMS_Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GMS_GameGenres_GMS_GenreTypes_IdGenre",
                        column: x => x.IdGenre,
                        principalTable: "GMS_GenreTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GMS_GameGenres_IdGenre",
                table: "GMS_GameGenres",
                column: "IdGenre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GMS_GameGenres");

            migrationBuilder.DropTable(
                name: "GMS_Games");

            migrationBuilder.DropTable(
                name: "GMS_GenreTypes");
        }
    }
}
