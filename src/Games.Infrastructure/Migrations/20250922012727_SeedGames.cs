using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@$"
              INSERT INTO [dbo].[GMS_Games]
               ([Id]
               ,[Title]
               ,[Description]
               ,[Price]
               ,[Rating]
               ,[Developer]
               ,[IndicatedAgeRating]
               ,[HourPlayed]
               ,[ImageUrl]
               ,[IsFree])
         VALUES
               (730347b5-491f-4e49-b5e8-6934bb460c57
               ,'Game1'
               ,'Game 1 foi desenvolvido pela empresa 1'
               ,{decimal.Zero}
               ,3.0
               ,''
               ,'Livre'
               ,{decimal.Zero}
               ,''
               ,1)

  migrationBuilder.Sql(@$
              INSERT INTO [dbo].[GMS_Games]
               ([Id]
               ,[Title]
               ,[Description]
               ,[Price]
               ,[Rating]
               ,[Developer]
               ,[IndicatedAgeRating]
               ,[HourPlayed]
               ,[ImageUrl]
               ,[IsFree])
         VALUES
               (e91d7477-6896-4a70-b94d-d662c319f496
               ,'Game2'
               ,'Game 2 foi desenvolvido pela empresa 2'
               ,{decimal.Zero}
               ,3.0
               ,''
               ,'Livre'
               ,{decimal.Zero}
               ,''
               ,1)



  migrationBuilder.Sql(@$
              INSERT INTO [dbo].[GMS_Games]
               ([Id]
               ,[Title]
               ,[Description]
               ,[Price]
               ,[Rating]
               ,[Developer]
               ,[IndicatedAgeRating]
               ,[HourPlayed]
               ,[ImageUrl]
               ,[IsFree])
         VALUES
               (56cccd79-607c-470d-9b42-c1afaf745630
               ,'Game3'
               ,'Game 3 foi desenvolvido pela empresa 3'
               ,{decimal.Zero}
               ,3.0
               ,''
               ,'Livre'
               ,{decimal.Zero}
               ,''
               ,1)

  migrationBuilder.Sql(@$
              INSERT INTO [dbo].[GMS_Games]
               ([Id]
               ,[Title]
               ,[Description]
               ,[Price]
               ,[Rating]
               ,[Developer]
               ,[IndicatedAgeRating]
               ,[HourPlayed]
               ,[ImageUrl]
               ,[IsFree])
         VALUES
               (89036b1c-ffed-41d1-9f14-e878701a43aa
               ,'Game4'
               ,'Game 4 foi desenvolvido pela empresa 4'
               ,45.00
               ,3.0
               ,''
               ,'Livre'
               ,{decimal.Zero}
               ,''
               ,0)



  migrationBuilder.Sql(@$
              INSERT INTO [dbo].[GMS_Games]
               ([Id]
               ,[Title]
               ,[Description]
               ,[Price]
               ,[Rating]
               ,[Developer]
               ,[IndicatedAgeRating]
               ,[HourPlayed]
               ,[ImageUrl]
               ,[IsFree])
         VALUES
               (fec99d22-3744-406b-96cc-16edb14ae827
               ,'Game5'
               ,'Game 5 foi desenvolvido pela empresa 5'
               ,{29.99}
               ,3.0
               ,''
               ,'Livre'
               ,{decimal.Zero}
               ,''
               ,1)
     
                ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
