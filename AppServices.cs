using ItSupportServer.src.Modules.Area;
using ItSupportServer.src.Modules.Authentication;
using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Modules.Employee;
using ItSupportServer.src.Modules.Issue;
using ItSupportServer.src.Modules.IssueLog;
using ItSupportServer.src.Modules.Role;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using ItSupportServer.src.Modules.Account;

namespace ItSupportServer
{
    public static class AppServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register application services here
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IAreaService, AreaService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IIssueService, IssueService>();
            services.AddScoped<IIssueLogService, IssueLogService>();

            // Đăng ký Fluent Validation Validators
            services.AddValidatorsFromAssemblyContaining<CreateAreaDtoValidator>();

            // Đăng ký Mapperly Mapper
            services.AddSingleton<AccountMapper>();
            services.AddSingleton<RoleMapper>();
            services.AddSingleton<EmployeeMapper>();
            services.AddSingleton<AreaMapper>();
            services.AddSingleton<IssueMapper>();
            services.AddSingleton<IssueLogsMapper>();

            // Đăng ký Handler xử lý logic
            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

            // Đăng ký Provider để tự động nhận diện Permission từ Attribute
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

            return services;
        }
    }
}
