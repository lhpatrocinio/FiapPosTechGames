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
    public class LibraryConfiguration : IEntityTypeConfiguration<Library>
    {
        public void Configure(EntityTypeBuilder<Library> builder)
        {
            builder.ToTable("GMS_Library");

            builder.HasKey(g => g.Id);

            builder.Property(g => g.Name);

            builder.Property(g => g.IdUser);

            builder.Property(g => g.CreateAt);

            builder.Property(g => g.UpdateAt);

            builder.HasMany(gt => gt.GameLibraries)
           .WithOne(gg => gg.Library)
           .HasForeignKey(gg => gg.IdLibrary);        
        }
    }
}
