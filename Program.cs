using ITSupportServer;
using ITSupportServer.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Console.WriteLine(builder.Configuration["secret"]);
//var dataSourceBuilder = new NpgsqlDataSourceBuilder(
//    builder.Configuration.GetConnectionString("DefaultConnection")
//);

//dataSourceBuilder.EnableDynamicJson();

//var dataSource = dataSourceBuilder.Build();

//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseNpgsql(dataSource, npgsqlOptions =>
//    npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name)));

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
            Title = "IT Support Management API",
            Version = "v1",
            Description = "Tài liệu API cho hệ thống ký lục IT Support.\n\n" +
                          "**Hỗ trợ:** \n" +
                          "- Viết ký lục sửa chữa thiết bị, máy tính" +
            Contact = new OpenApiContact
            {
                Name = "Khang Support",
                Email = "khangkttb01029@fpt.edu.vn"
            }
        };

        options.AddSchemaTransformer<ExampleSchemaTransformer>();

        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("CorPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
