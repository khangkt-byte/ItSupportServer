using ItSupportServer.src.Modules.Area;
using ItSupportServer.src.Modules.Authentication;
using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Modules.Employee;
using ItSupportServer.src.Modules.Issue;
using ItSupportServer.src.Modules.IssueLog;
using ItSupportServer.src.Modules.Role;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;

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
            services.AddScoped<IEmployeesService, EmployeesService>();
            services.AddScoped<IIssuesService, IssuesService>();
            services.AddScoped<IIssueLogsService, IssueLogsService>();

            // Đăng ký Fluent Validation Validators
            services.AddValidatorsFromAssemblyContaining<CreateAreaDtoValidator>();

            // Đăng ký Mapperly Mapper
            services.AddSingleton<EmployeesMapper>();

            // Đăng ký Handler xử lý logic
            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

            // Đăng ký Provider để tự động nhận diện Permission từ Attribute
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

            return services;
        }
    }
}
