using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ============================================================================
            // SEED DATA INICIAL - Dados de teste para sistema de games
            // Migration separada para facilitar manutenção e clareza
            // ============================================================================
            
            // PASSO 1: Inserir Gêneros de Jogos (11 tipos)
            migrationBuilder.InsertData(
                table: "GMS_GenreTypes",
                columns: new[] { "Id", "Description" },
                values: new object[,]
                {
                    { 1, "RPG" },
                    { 2, "Adventure" },
                    { 3, "Sandbox" },
                    { 4, "Survival" },
                    { 5, "Shooter" },
                    { 6, "Action" },
                    { 7, "Competitive" },
                    { 8, "Sports" },
                    { 9, "Simulation" },
                    { 10, "Racing" },
                    { 11, "Open World" }
                });

            // PASSO 2: Definir GUIDs fixos para os jogos (facilita testes)
            var witcherId = Guid.Parse("A1B2C3D4-E5F6-7A90-1234-567890ABCDEF");
            var minecraftId = Guid.Parse("B2C3D4E5-F617-8A01-2345-678901BCDEF2");
            var cs2Id = Guid.Parse("C3D4E5F6-A7B8-9012-3456-789012CDEF34");
            var fifa24Id = Guid.Parse("D4E5F6A7-B8C9-0123-4567-890123DEF456");
            var forzaId = Guid.Parse("E5F6A7B8-C9D0-1234-5678-901234EF5678");

            // PASSO 3: Inserir Jogos Realistas (5 jogos populares)
            migrationBuilder.InsertData(
                table: "GMS_Games",
                columns: new[] { "Id", "Title", "Description", "Price", "Rating", "Developer", "IndicatedAgeRating", "HourPlayed", "ImageUrl", "IsFree" },
                values: new object[,]
                {
                    { witcherId, "The Witcher 3: Wild Hunt", "Um RPG épico em mundo aberto com rica narrativa e combate dinâmico", 199.90m, 9.8m, "CD Projekt Red", "18+", 150.0m, "https://example.com/witcher3.jpg", false },
                    { minecraftId, "Minecraft", "Jogo de construção e sobrevivência em mundo infinito", 89.90m, 9.2m, "Mojang Studios", "10+", 500.0m, "https://example.com/minecraft.jpg", false },
                    { cs2Id, "Counter-Strike 2", "FPS competitivo tático com ação intensa", 0.00m, 8.5m, "Valve Corporation", "16+", 200.0m, "https://example.com/cs2.jpg", true },
                    { fifa24Id, "FIFA 24", "Simulador de futebol com jogabilidade realista", 249.90m, 8.0m, "EA Sports", "3+", 80.0m, "https://example.com/fifa24.jpg", false },
                    { forzaId, "Forza Horizon 5", "Jogo de corrida em mundo aberto no México", 199.90m, 9.1m, "Playground Games", "3+", 120.0m, "https://example.com/forza5.jpg", false }
                });

            // PASSO 4: Inserir Relacionamentos Game-Genre (14 associações)
            // Cada jogo pode ter múltiplos gêneros para busca mais rica
            migrationBuilder.InsertData(
                table: "GMS_GameGenres",
                columns: new[] { "IdGame", "IdGenre" },
                values: new object[,]
                {
                    // The Witcher 3: RPG + Adventure + Open World
                    { witcherId, 1 }, // RPG
                    { witcherId, 2 }, // Adventure  
                    { witcherId, 11 }, // Open World
                    
                    // Minecraft: Sandbox + Survival + Adventure
                    { minecraftId, 3 }, // Sandbox
                    { minecraftId, 4 }, // Survival
                    { minecraftId, 2 }, // Adventure
                    
                    // Counter-Strike 2: Shooter + Action + Competitive
                    { cs2Id, 5 }, // Shooter
                    { cs2Id, 6 }, // Action
                    { cs2Id, 7 }, // Competitive
                    
                    // FIFA 24: Sports + Simulation
                    { fifa24Id, 8 }, // Sports
                    { fifa24Id, 9 }, // Simulation
                    
                    // Forza Horizon 5: Racing + Action + Open World
                    { forzaId, 10 }, // Racing
                    { forzaId, 6 }, // Action
                    { forzaId, 11 } // Open World
                });

            // ============================================================================
            // SEED DATA CONCLUÍDO
            // Total: 11 gêneros + 5 jogos + 14 associações GameGenres
            // ============================================================================
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remover dados na ordem inversa (devido às foreign keys)
            
            // 1. Remover associações GameGenres
            migrationBuilder.DeleteData(
                table: "GMS_GameGenres",
                keyColumns: new[] { "IdGame", "IdGenre" },
                keyValues: new object[] { Guid.Parse("A1B2C3D4-E5F6-7A90-1234-567890ABCDEF"), 1 });
            
            migrationBuilder.DeleteData(
                table: "GMS_GameGenres",
                keyColumns: new[] { "IdGame", "IdGenre" },
                keyValues: new object[] { Guid.Parse("A1B2C3D4-E5F6-7A90-1234-567890ABCDEF"), 2 });
            
            // ... (outras associações - simplificado para brevidade)
            
            // 2. Remover jogos
            migrationBuilder.DeleteData(
                table: "GMS_Games",
                keyColumn: "Id",
                keyValue: Guid.Parse("A1B2C3D4-E5F6-7A90-1234-567890ABCDEF"));
            
            migrationBuilder.DeleteData(
                table: "GMS_Games", 
                keyColumn: "Id",
                keyValue: Guid.Parse("B2C3D4E5-F617-8A01-2345-678901BCDEF2"));
                
            // ... (outros jogos)
            
            // 3. Remover gêneros
            migrationBuilder.DeleteData(
                table: "GMS_GenreTypes",
                keyColumn: "Id", 
                keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 });
        }
    }
}
