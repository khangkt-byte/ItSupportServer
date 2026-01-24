using ItSupportServer.src.Modules.Area;
using ItSupportServer.src.Modules.Authentication;
using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Modules.Role;
using Microsoft.AspNetCore.Authorization;

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

            // Đăng ký Handler xử lý logic
            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

            // Đăng ký Provider để tự động nhận diện Permission từ Attribute
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

            return services;
        }
    }
}
