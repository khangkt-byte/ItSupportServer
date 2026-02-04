using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ITSupportServer;
using ITSupportServer.EF_Core.Data;
using ITSupportServer.EF_Core.Seeds;
using ITSupportServer.src.Shared.Attributes;
using ITSupportServer.src.Shared.BackgroundServices;
using ITSupportServer.src.Shared.MidlewareServices;
using Npgsql;
using Scalar.AspNetCore;
using System.Text;
using static ITSupportServer.src.Shared.Helper.GitHubImageService;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

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

builder.Services.AddOpenApi();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddApplicationServices();

builder.Services.Configure<GitHubOptions>(builder.Configuration.GetSection(GitHubOptions.GitHub));
builder.Services.AddMemoryCache();
//Khai báo các background services
builder.Services.AddBackgroundJobs();

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
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
.AddCookie()
.AddGoogle(option =>
{
    var clientId = builder.Configuration["Authentication:Google:ClientId"];
    var clientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    if (clientId is null) throw new ArgumentNullException("ClientId is null");
    if (clientSecret is null) throw new ArgumentNullException("ClientSecret is null");

    option.ClientId = clientId;
    option.ClientSecret = clientSecret;
    option.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    option.Scope.Add("profile");
    option.Scope.Add("email");
    option.Scope.Add("openid");
    option.ClaimActions.MapJsonKey("picture", "picture");
});
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorPolicy", policy =>
        {
            policy
            .WithOrigins("http://localhost:7000", "https://dauqlk.vercel.app")
                //.AllowAnyOrigin()     // hoặc .WithOrigins("http://localhost:5173") nếu muốn giới hạn
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });
}
else
{
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
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()     // hoặc .WithOrigins("http://localhost:5173") nếu muốn giới hạn
            .AllowAnyMethod()
            .AllowAnyHeader();
        //.AllowCredentials();
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

        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
        // Sửa thông tin Info của tài liệu
        document.Info = new OpenApiInfo
        {
            Title = "Nhà Hàng API",
            Version = "v1",
            Description = "Tài liệu API cho hệ thống đặt món ăn.\n\n" +
                          "**Hỗ trợ:** \n" +
                          "- Quản lý món ăn\n" +
                          "- Đặt hàng real-time",
            Contact = new OpenApiContact
            {
                Name = "XuongKien Support",
                Email = "xuongkienphung.com"
            }
        };

        options.AddSchemaTransformer<ExampleSchemaTransformer>();

        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseHttpsRedirection();
//if (app.Environment.IsDevelopment())
//{
//    app.MapScalarApiReference();
//    app.MapOpenApi();
//}

app.MapScalarApiReference();
app.MapOpenApi();

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin-allow-popups";
    ctx.Response.Headers["Cross-Origin-Embedder-Policy"] = "unsafe-none";
    await next();
});

app.UseCors("CorPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<CheckBannedUserMiddleware>();

app.MapControllers();

app.Run();
