using Games.Infrastructure.DataBase.EntityFramework.EntityConfig;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.DataBase.EntityFramework.Context
{
    /// <summary>  
    /// Represents the application's database context, providing access to the database and its entities.  
    /// </summary>  
    public class ApplicationDbContext : DbContext
    {
        /// <summary>  
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class with the specified options.  
        /// </summary>  
        /// <param name="options">The options to configure the database context.</param>  
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new GameConfiguration());
            builder.ApplyConfiguration(new GenreTypesConfiguration());
            builder.ApplyConfiguration(new GameGenresConfiguration());
        }
    }
}
