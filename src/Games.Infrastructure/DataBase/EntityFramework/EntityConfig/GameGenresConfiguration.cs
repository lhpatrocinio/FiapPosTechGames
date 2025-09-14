using Games.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Games.Infrastructure.DataBase.EntityFramework.EntityConfig
{
    public class GameGenresConfiguration : IEntityTypeConfiguration<GameGenre>
    {
        public void Configure(EntityTypeBuilder<GameGenre> builder)
        {
            builder.ToTable("GMS_GameGenres");

            builder.HasKey(gg => new { gg.IdGame, gg.IdGenre });

            builder.HasOne(gg => gg.Game)
                   .WithMany(g => g.Genres)
                   .HasForeignKey(gg => gg.IdGame);

            builder.HasOne(gg => gg.GenreType)
                   .WithMany(gt => gt.GameGenres)
                   .HasForeignKey(gg => gg.IdGenre);
        }
    }
}

