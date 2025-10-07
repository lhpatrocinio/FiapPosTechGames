using Games.Application.producer;
using Games.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Games.Application
{
    public static class ApplicationBootstrapper
    {
        public static void Register(IServiceCollection services)
        {
            services.AddTransient<IGameService, GameService>();
            services.AddTransient<IUserActiveProducer, UserActiveProducer>();
        }
    }
}
