using ItSupportServer;
using ItSupportServer.Data.Models;
using ItSupportServer.Data.Interceptors;
using ItSupportServer.src.Shared.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Npgsql;
using Scalar.AspNetCore;
using System.Text;
using ItSupportServer.src.Shared.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Reflection;

// ✅ STEP 1: Configure Serilog BEFORE building WebApplication
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger(); // ✅ Bootstrap logger for startup errors

try
{
    Log.Information("Starting IT Support Server application");

    var builder = WebApplication.CreateBuilder(args);

    // ✅ STEP 2: Replace built-in logging with Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.WithProcessId()
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}")
        .WriteTo.File(
            path: "Logs/itsupport-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            fileSizeLimitBytes: 10485760,
            shared: true,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    );

    // ✅ Configure Kestrel to use HTTPS
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.AddServerHeader = false; // Remove Server header

        if (!builder.Environment.IsDevelopment())
        {
            // ✅ PRODUCTION: Force HTTPS only
            options.ConfigureHttpsDefaults(httpsOptions =>
            {
                httpsOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 |
                                           System.Security.Authentication.SslProtocols.Tls13;
            });
        }
    });

    // Add services to the container.
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

    // Database with PostgreSQL + Interceptor
    var dataSourceBuilder = new NpgsqlDataSourceBuilder(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );

    dataSourceBuilder.EnableDynamicJson();
    var dataSource = dataSourceBuilder.Build();

    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseNpgsql(dataSource, npgsqlOptions =>
            npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name));

        // ✅ ADD AUDIT INTERCEPTOR
        options.AddInterceptors(new AuditInterceptor());
    });

    // Add application services
    builder.Services.AddApplicationServices();

    builder.Services.AddMemoryCache();

    // JWT Authentication
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            // ✅ CRITICAL: Require HTTPS in production
            options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

            // ✅ Save token to AuthenticationProperties (for refresh scenarios)
            options.SaveToken = false; // ⚠️ Set to false for security (tokens in memory only)

            // ✅ Token Validation Parameters
            options.TokenValidationParameters = new TokenValidationParameters
            {
                // ===== SIGNATURE VALIDATION =====
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)),
                RequireSignedTokens = true,

                // ===== ISSUER VALIDATION =====
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["AppSettings:Issuer"],

                // ===== AUDIENCE VALIDATION =====
                ValidateAudience = true,
                ValidAudience = builder.Configuration["AppSettings:Audience"],

                // ===== LIFETIME VALIDATION =====
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.FromMinutes(1), // ✅ Allow 1 min clock skew (RECOMMENDED)

                // ===== ADDITIONAL SECURITY =====
                NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier,
                RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            };

            // ✅ Event Handlers for Session Management
            options.Events = new JwtBearerEvents
            {
                // ✅ 1. Handle Authentication Failures
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        // ✅ CRITICAL: Signal token expiration to client
                        context.Response.Headers.Add("Token-Expired", "true");
                        context.Response.Headers.Add("Access-Control-Expose-Headers", "Token-Expired");
                    }

                    // ✅ Log failed authentication with Serilog
                    Log.Warning(
                        "Authentication failed: {Exception} | Path: {Path} | IP: {IP}",
                        context.Exception.Message,
                        context.Request.Path,
                        context.HttpContext.Connection.RemoteIpAddress
                    );

                    return Task.CompletedTask;
                },

                // ✅ 2. Validate Token on Each Request
                OnTokenValidated = async context =>
                {
                    // ✅ Get account ID from token
                    var accountId = context.Principal?.FindFirst(
                        System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                    if (accountId == null)
                    {
                        context.Fail("Invalid token - missing account ID");
                        return;
                    }

                    // ✅ CRITICAL: Check if account is still valid
                    var db = context.HttpContext.RequestServices
                        .GetRequiredService<AppDbContext>();

                    var account = await db.Accounts
                        .AsNoTracking()
                        .FirstOrDefaultAsync(a => a.AccountId.ToString() == accountId);

                    if (account == null || account.DeletedAt != null)
                    {
                        Log.Warning("Token valid but account deleted: {AccountId}", accountId);
                        context.Fail("Account no longer exists");
                        return;
                    }

                    // ✅ CRITICAL: Check if account is locked
                    if (account.IsLocked && account.LockedUntil > DateTime.UtcNow)
                    {
                        Log.Warning("Token valid but account locked: {AccountId}", accountId);
                        context.Fail("Account is locked");
                        return;
                    }

                    Log.Debug("Token validated successfully for account: {AccountId}", accountId);
                },

                // ✅ 3. Handle Challenge (401 responses)
                OnChallenge = context =>
                {
                    Log.Warning(
                        "Authentication challenge: {Error} | Path: {Path}",
                        context.Error ?? "Unauthorized",
                        context.Request.Path
                    );

                    return Task.CompletedTask;
                },

                // ✅ 4. Handle Message Received
                OnMessageReceived = context =>
                {
                    // ✅ Support SignalR (token from query string)
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorPolicy", policy =>
        {
            // ✅ FIX: Explicit whitelist origins
            policy.WithOrigins(
                    "http://localhost:3000",  // Development
                    "http://localhost:5173"  // Vite
                    //"https://yourdomain.com"   // Production
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                // ✅ ADD: Expose custom headers
                .WithExposedHeaders("Token-Expired", "X-CSRF-Token");
        });
    });

    // ✅ CRITICAL: Add Rate Limiting
    builder.Services.AddRateLimiter(options =>
    {
        // ✅ 1. IP-based global limiter (catches bots)
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        {
            var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = httpContext.Request.Headers["User-Agent"].ToString().ToLower();

            // ✅ ENHANCED: Detect bot signatures
            var isSuspiciousBot = new[] { "bot", "crawler", "spider", "scraper", "headless" }
                .Any(sig => userAgent.Contains(sig));

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: ip,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = isSuspiciousBot ? 10 : 100, // ✅ Stricter for bots
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
        });

        // ✅ 2. Strict rate limiter for authentication endpoints
        options.AddFixedWindowLimiter("auth", opt =>
        {
            opt.PermitLimit = 5; // 5 login attempts
            opt.Window = TimeSpan.FromMinutes(15); // per 15 minutes
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 0;
        });

        // ✅ 3. Moderate rate limiter for API endpoints
        options.AddSlidingWindowLimiter("api", opt =>
        {
            opt.PermitLimit = 50; // 50 requests
            opt.Window = TimeSpan.FromMinutes(1); // per minute
            opt.SegmentsPerWindow = 6; // 6 segments (10 seconds each)
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 0;
        });

        // ✅ 4. OnRejected handler
        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = 429; // Too Many Requests

            // ✅ FIX: Declare retryAfter outside if block
            TimeSpan? retryAfterValue = null;

            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                retryAfterValue = retryAfter;
                context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString();
            }

            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "TOO_MANY_REQUESTS",
                message = "Rate limit exceeded. Please try again later.",
                retryAfter = retryAfterValue?.TotalSeconds
            }, cancellationToken: token);

            Log.Warning(
                "Rate limit exceeded for IP: {IP} | Path: {Path}",
                context.HttpContext.Connection.RemoteIpAddress,
                context.HttpContext.Request.Path
            );
        };
    });

    // ✅ CRITICAL: Add Health Checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<AppDbContext>(
            name: "database",
            failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
            tags: new[] { "db", "sql", "postgresql" })
        .AddCheck("self", () =>
            Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Application is running"),
            tags: new[] { "self" });

    // Scalar configuration
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            // Initialize Components if null
            document.Components ??= new OpenApiComponents();

            // Initialize SecuritySchemes dictionary if null
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

            // Now safe to add
            document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Nhập JWT Token vào đây "
            });

            // Sửa thông tin Info của tài liệu
            document.Info = new OpenApiInfo
            {
                Title = "IT Support Management API",
                Version = "v1",
                Description = "Tài liệu API cho hệ thống ký lục IT Support.\n\n" +
                              "**Hỗ trợ:** \n" +
                              "- Viết ký lục sửa chữa thiết bị, máy tính",
                Contact = new OpenApiContact
                {
                    Name = "Khangkt",
                    Email = "khangkttb01029@fpt.edu.vn"
                }
            };

            options.AddSchemaTransformer<ExampleSchemaTransformer>();

            // ✅ ADD: Include XML comments
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            // Note: Scalar automatically reads XML comments
            // No additional configuration needed

            return Task.CompletedTask;
        });
    });

    // Global exception handler
    builder.Services.AddExceptionHandler<GlobalExceptionHandlerMiddleware>();
    builder.Services.AddProblemDetails();

    var app = builder.Build();

    // Scalar UI (instead of Swagger)
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("IT Support API Documentation")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });

    // ✅ OPTIMIZED MIDDLEWARE PIPELINE (5 middleware instead of 11)
    app.UseExceptionHandler();

    // ✅ 1. Security headers (CSP, HSTS, X-Frame-Options)
    if (!app.Environment.IsDevelopment())
    {
        app.UseSecurityHeaders(); // Only in Production
    }

    // ✅ 2. HTTPS Redirection & HSTS
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
        app.UseHsts();
    }

    // ✅ 3. Serilog Request Logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.GetLevel = (httpContext, elapsed, ex) =>
        {
            if (ex != null) return LogEventLevel.Error;
            if (httpContext.Response.StatusCode > 499) return LogEventLevel.Error;
            if (httpContext.Response.StatusCode > 399) return LogEventLevel.Warning;
            return LogEventLevel.Information;
        };
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        };
    });

    // ✅ 4. Routing
    app.UseRouting();

    // ✅ 5. CORS
    app.UseCors("CorPolicy");

    // ✅ 6. Rate Limiting (includes bot protection)
    app.UseRateLimiter();

    // ✅ 7. Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // ✅ 8. CONSOLIDATED: 3-layer CSRF protection
    app.UseEnhancedCsrfProtection(); // ✅ NEW (replaces 3 middleware)

    // ✅ 9. CONSOLIDATED: Session tracking + hijacking detection
    app.UseUnifiedSessionTracking(); // ✅ NEW (replaces 2 middleware)

    // ✅ 10. Health Checks & Endpoints
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds
                }),
                totalDuration = report.TotalDuration.TotalMilliseconds
            });
            await context.Response.WriteAsync(result);
        }
    }).RequireHost("*:5001"); // ✅ Restrict to management port

    app.MapHealthChecks("/health/ready").RequireHost("*:5001");
    app.MapHealthChecks("/health/live").RequireHost("*:5001");

    app.MapControllers();

    Log.Information("IT Support Server started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
