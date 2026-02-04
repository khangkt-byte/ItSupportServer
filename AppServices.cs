using ItSupportServer.AreasController;

namespace ItSupportServer
{
    public static class AppServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register application services here
            //services.AddScoped<IAreasServices, AreasServices>();

            return services;
        }
    }
}
