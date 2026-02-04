using ItSupportServer;
using ItSupportServer.Data.Models;
using ItSupportServer.src.Shared.Middleware;


//using ItSupportServer.src.Shared.Attributes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Npgsql;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

Console.WriteLine(builder.Configuration["secret"]);
var dataSourceBuilder = new NpgsqlDataSourceBuilder(
    builder.Configuration.GetConnectionString("DefaultConnection")
);

dataSourceBuilder.EnableDynamicJson();

var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dataSource, npgsqlOptions =>
    npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name)));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddApplicationServices();

builder.Services.AddMemoryCache();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(option =>
{
    option.RequireHttpsMetadata = false;
    option.SaveToken = true;
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["AppSettings:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["AppSettings:Audience"],
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                builder.Configuration["AppSettings:Token"]!)),
        ValidateIssuerSigningKey = true,
    };
})
.AddCookie();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        // 1. Định nghĩa Security Scheme (Bearer)
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Nhập JWT Token vào đây "
        });

        //document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        //{
        //    {
        //        new OpenApiSecurityScheme
        //        {
        //            Reference = new OpenApiReference
        //            {
        //                Type = ReferenceType.SecurityScheme,
        //                Id = "Bearer"
        //            }
        //        },
        //        Array.Empty<string>()
        //    }
        //});

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

        //options.AddSchemaTransformer<ExampleSchemaTransformer>();

        return Task.CompletedTask;
    });
});

// Add global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandlerMiddleware>();
builder.Services.AddProblemDetails();

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
if (builder.Environment.IsProduction())
{
    // Add Application Insights or Serilog in production
    // builder.Logging.AddApplicationInsights();
}

var app = builder.Build();

// Use exception handler (MUST be before other middleware)
app.UseExceptionHandler();

// In development, include developer exception page for unhandled exceptions
if (app.Environment.IsDevelopment())
{
    // The exception handler middleware will still catch exceptions
    // but you can add dev-specific features here
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
