using Games.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Games.Infrastructure.DataBase.EntityFramework.EntityConfig
{
    public class GameLibraryConfiguration : IEntityTypeConfiguration<GameLibrary>
    {
            public void Configure(EntityTypeBuilder<GameLibrary> builder)
            {
                builder.ToTable("GMS_GamesLibrary");

                // Chave composta
                builder.HasKey(gl => new { gl.IdLibrary, gl.IdGame });

                // Relacionamento com Library (1:N)
                builder.HasOne(gl => gl.Library)
                       .WithMany(l => l.GameLibraries) // precisa de ICollection<GameLibrary> em Library
                       .HasForeignKey(gl => gl.IdLibrary)
                       .OnDelete(DeleteBehavior.Cascade);

                // Relacionamento com Game (1:N)
                builder.HasOne(gl => gl.Game)
                       .WithMany(g => g.GameLibraries) // precisa de ICollection<GameLibrary> em Game
                       .HasForeignKey(gl => gl.IdGame)
                       .OnDelete(DeleteBehavior.Cascade);
            }
    }
}
