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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Database with PostgreSQL + Interceptor
Console.WriteLine(builder.Configuration["secret"]);
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
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(option =>
{
    option.RequireHttpsMetadata = true;  // ✅ Enforce HTTPS in production
    option.SaveToken = true;
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["AppSettings:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["AppSettings:Audience"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                builder.Configuration["AppSettings:Token"]!)),
        ClockSkew = TimeSpan.Zero  // ✅ No tolerance for expired tokens
    };

    // ✅ Handle authentication failures
    option.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
})
.AddCookie();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorPolicy", policy =>
    {
        policy.WithOrigins("https://yourdomain.com")  // ✅ Specify allowed origins
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

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

        return Task.CompletedTask;
    });
});

// Global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandlerMiddleware>();
builder.Services.AddProblemDetails();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (builder.Environment.IsProduction())
{
    // Add Application Insights or Serilog in production
    // builder.Logging.AddApplicationInsights();
}

var app = builder.Build();

// Middleware pipeline
app.UseExceptionHandler();

// In development, include developer exception page for unhandled exceptions
if (app.Environment.IsDevelopment())
{
    // The exception handler middleware will still catch exceptions
    // but you can add dev-specific features here
}

// Scalar UI (instead of Swagger)
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options
        .WithTitle("IT Support API Documentation")
        .WithTheme(ScalarTheme.DeepSpace)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.UseCors("CorPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
