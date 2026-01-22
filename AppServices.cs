using ItSupportServer.src.Modules.Area;
using ItSupportServer.src.Modules.Authentication;
using ItSupportServer.src.Modules.Role;

namespace ItSupportServer
{
    public static class AppServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register application services here
            services.AddScoped<IAuthenticationsService, AuthenticationsService>();
            services.AddScoped<IRolesService, RolesService>();
            services.AddScoped<IAreasService, AreasService>();

            return services;
        }
    }
}
