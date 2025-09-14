using Games.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.DataBase.EntityFramework.EntityConfig
{
    public class GameConfiguration : IEntityTypeConfiguration<Game>
    {
        public void Configure(EntityTypeBuilder<Game> builder)
        {
            builder.ToTable("GMS_Games");

            builder.HasKey(g => g.Id);

            builder.Property(g => g.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(g => g.Description)
                .HasMaxLength(1000);

            builder.Property(g => g.Price)
                .HasColumnType("decimal(10,2)");

            builder.Property(g => g.Rating)
                .HasColumnType("decimal(5,2)");

            builder.Property(g => g.Developer)
                .HasMaxLength(200);

            builder.Property(g => g.IndicatedAgeRating)
                .HasMaxLength(50);

            builder.Property(g => g.HourPlayed)
                .HasColumnType("decimal(6,2)");

            builder.Property(g => g.ImageUrl)
                .HasMaxLength(500);

            builder.HasMany(g => g.Genres)
                .WithOne(gg => gg.Game)
                .HasForeignKey(gg => gg.IdGame);
        }
    }
}
