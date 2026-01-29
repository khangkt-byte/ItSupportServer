using ItSupportServer.src.Modules.Area;
using ItSupportServer.src.Modules.Authentication;
using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Modules.Employee;
using ItSupportServer.src.Modules.Issue;
using ItSupportServer.src.Modules.IssueLog;
using ItSupportServer.src.Modules.Role;
using ItSupportServer.src.Modules.Account;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using ItSupportServer.src.Modules.Cause;

namespace ItSupportServer
{
    public static class AppServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // ✅ Register Services (Scoped - per request)
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<AuthorizationService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IAreaService, AreaService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IIssueService, IssueService>();
            services.AddScoped<IIssueLogService, IssueLogService>();
            services.AddScoped<IIssueLogImportService, IssueLogImportService>();
            services.AddScoped<ICauseService, CauseService>();

            // ✅ Register FluentValidation Validators (auto-discovery)
            services.AddValidatorsFromAssemblyContaining<CreateAreaDtoValidator>();
            // This automatically registers all validators in the assembly:
            // - CreateAreaDtoValidator, UpdateAreaDtoValidator
            // - LoginDtoValidator, OtpDtoValidator
            // - CreateIssueLogDtoValidator, etc.

            // ✅ Register Mapperly Mappers (Singleton - stateless)
            services.AddSingleton<AccountMapper>();
            services.AddSingleton<RoleMapper>();
            services.AddSingleton<EmployeeMapper>();
            services.AddSingleton<AreaMapper>();
            services.AddSingleton<IssueMapper>();
            services.AddSingleton<IssueLogMapper>();
            services.AddSingleton<IssueLogImportMapper>();
            services.AddSingleton<CauseMapper>();

            // ✅ Register Authorization Handler & Policy Provider (Singleton)
            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

            return services;
        }
    }
}
