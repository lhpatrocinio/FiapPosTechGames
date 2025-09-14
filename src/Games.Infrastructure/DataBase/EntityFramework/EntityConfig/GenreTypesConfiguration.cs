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
    public class GenreTypesConfiguration : IEntityTypeConfiguration<GenreTypes>
    {
        public void Configure(EntityTypeBuilder<GenreTypes> builder)
        {
            builder.ToTable("GMS_GenreTypes");

            builder.HasKey(gt => gt.Id);

            builder.Property(gt => gt.Description)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasMany(gt => gt.GameGenres)
                   .WithOne(gg => gg.GenreType)
                   .HasForeignKey(gg => gg.IdGenre);
        }
    }
}
