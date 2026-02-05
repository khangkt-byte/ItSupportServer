using ItSupportServer.src.Modules.Area;
using ItSupportServer.src.Modules.Authentication;
using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Modules.Employee;
using ItSupportServer.src.Modules.Issue;
using ItSupportServer.src.Modules.IssueLog;
using ItSupportServer.src.Modules.Role;
using ItSupportServer.src.Modules.Account;
using FluentValidation;
using ItSupportServer.src.Modules.Cause;
using Microsoft.AspNetCore.Authorization;
using ItSupportServer.src.Shared.Services;
using ItSupportServer.src.Modules.Department;

namespace ItSupportServer
{
    /// <summary>
    /// Centralized service registration
    /// Pattern: Dependency Injection container configuration
    /// Reference: Clean Architecture, Microsoft .NET guidelines
    /// </summary>
    public static class AppServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // ===== AUTHORIZATION (Policy-based) =====
            // Pattern: ASP.NET Core Authorization system
            // Order matters: PolicyProvider → Handler → AuthorizationService
            
            // ✅ FIXED: Singleton (was Scoped - causing memory leak)
            // Reason: Stateless handler, creates scopes manually
            services.AddScoped<IAuthorizationHandler, PermissionHandler>();
            
            // ✅ CORRECT: Singleton for policy provider
            // Reason: Stateless, called on every request for policy discovery
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            
            // ✅ CORRECT: Scoped for authorization service
            // Reason: Needs DbContext (which is scoped)
            services.AddScoped<src.Modules.Authorization.IAuthorizationService, AuthorizationService>();
            
            // ✅ REQUIRED: Enable ASP.NET Core authorization
            services.AddAuthorization();

            // ===== BUSINESS SERVICES (Scoped - per request) =====
            
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IAreaService, AreaService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IIssueService, IssueService>();
            services.AddScoped<IIssueLogService, IssueLogService>();
            services.AddScoped<IIssueLogImportService, IssueLogImportService>();
            services.AddScoped<ICauseService, CauseService>();

            // ===== VALIDATION (FluentValidation) =====
            
            // ✅ Auto-discover all validators in assembly
            services.AddValidatorsFromAssemblyContaining<CreateAreaValidator>();
            // Automatically registers:
            // - CreateAreaValidator, UpdateAreaValidator
            // - LoginValidator, OtpValidator
            // - CreateIssueLogValidator, UpdateIssueLogValidator
            // - CreateRoleValidator, UpdateRoleValidator
            // - etc.

            // ===== MAPPERS (Mapperly - Singleton, stateless) =====
            
            services.AddSingleton<AccountMapper>();
            services.AddSingleton<RoleMapper>();
            services.AddSingleton<EmployeeMapper>();
            services.AddSingleton<AreaMapper>();
            services.AddSingleton<IssueMapper>();
            services.AddSingleton<IssueLogMapper>();
            services.AddSingleton<IssueLogImportMapper>();
            services.AddSingleton<CauseMapper>();

            // ===== BACKGROUND SERVICES =====
    
            /// <summary>
            /// Token cleanup service (runs every 6 hours)
            /// Pattern: BackgroundService for maintenance tasks
            /// Purpose: Remove expired password reset tokens + revoked refresh tokens
            /// </summary>
            services.AddHostedService<TokenCleanupService>();

            return services;
        }
    }
}
