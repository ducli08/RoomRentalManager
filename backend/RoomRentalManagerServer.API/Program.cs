using Assembly = System.Reflection.Assembly;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RoomRentalManagerServer.Application.Model.UsersModel.UserProfileMapper;
using RoomRentalManagerServer.Domain.Interfaces.RedisCache;
using RoomRentalManagerServer.Domain.Interfaces.UserInterfaces;
using RoomRentalManagerServer.Infrastructure.Data;
using RoomRentalManagerServer.Infrastructure.RedisCache;
using RoomRentalManagerServer.Infrastructure.Repositories.UserRepository;
using StackExchange.Redis;
using System.Reflection;
using System.Text;
using RoomRentalManagerServer.Domain.Model;
using RoomRentalManagerServer.Domain.Interfaces;
using RoomRentalManagerServer.Infrastructure.Services;
var builder = WebApplication.CreateBuilder(args);
//cấu hình redis
builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

// cấu hình cloudinary
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
// khởi tạo kết nối postgredb
builder.Services.AddDbContext<RoomRentalManagerServerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// khởi tạo các controller
builder.Services.AddControllers();
// khởi tạo objectmaper
builder.Services.AddAutoMapper(typeof(UserMappingProfile).Assembly);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// inject các service từ các project khác
var applicationAssembly = Assembly.Load("RoomRentalManagerServer.Application");
var types = applicationAssembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && x.Name.EndsWith("Service"));
foreach(var type in types)
{
    var interfaceType = type.GetInterfaces().FirstOrDefault();
    if(interfaceType != null)
    {
        builder.Services.AddScoped(interfaceType, type);
    }
}
var domainAssembly = typeof(IUserRepository).Assembly;
var infrastructureAssembly = typeof(UserRepository).Assembly;

var repositoryInterface = domainAssembly.GetTypes().Where(x => x.IsInterface && x.Name.EndsWith("Repository")).ToList();
foreach(var interfaceType in repositoryInterface)
{
    var implementationType = infrastructureAssembly.GetTypes().FirstOrDefault(x => interfaceType.IsAssignableFrom(x) && x.IsClass);
    if(implementationType != null)
    {
        builder.Services.AddScoped(interfaceType, implementationType);
    }
}
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddSwaggerGen(x =>
{
    x.AddServer(new OpenApiServer { Url = "https://localhost:7246" });

    // Enable JWT Bearer auth in Swagger UI
    x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer token only. Example: \"Bearer {token}\""
    });
    x.AddSecurityRequirement(new OpenApiSecurityRequirement
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
});
// cho phép gọi api từ client
var AllowSpecificOrigins = "allowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins(builder.Configuration["Base_URL"]).AllowAnyHeader().AllowAnyMethod();
        });
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var options = new ConfigurationOptions
    {
        EndPoints = { configuration["Redis:ConnectionString"] },
        ConnectTimeout = int.Parse(configuration["Redis:ConnectTimeout"] ?? "5000"),
        SyncTimeout = int.Parse(configuration["Redis:SyncTimeout"] ?? "10000"),
        AbortOnConnectFail = false
    };
    return ConnectionMultiplexer.Connect(options);
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        options.SaveToken = true;
        options.RequireHttpsMetadata = false; // dev only

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? string.Empty))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var auth = ctx.Request.Headers["Authorization"].FirstOrDefault();
                Console.WriteLine($"Jwt OnMessageReceived: {ctx.Request.Method} {ctx.Request.Path} - Header: {auth ?? "null"}");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine($"Jwt OnAuthenticationFailed: {ctx.Exception?.GetType().Name} - {ctx.Exception?.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = ctx =>
            {
                Console.WriteLine($"Jwt OnTokenValidated: {ctx.Principal?.Identity?.Name}; Authenticated={ctx.Principal?.Identity?.IsAuthenticated}");
                return Task.CompletedTask;
            }
        };
    });var app = builder.Build();
app.UseStaticFiles();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(AllowSpecificOrigins);
app.UseHttpsRedirection();
app.UseAuthentication();
app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    if (!string.IsNullOrEmpty(authHeader))
    {
        Console.WriteLine($"Authorization Header: {authHeader}");
    }
    else
    {
        Console.WriteLine("Authorization Header not found!");
    }

    await next();
});
app.Use(async (context, next) =>
{
    Console.WriteLine($"After UseAuthentication - {context.Request.Method} {context.Request.Path} - IsAuthenticated: {context.User?.Identity?.IsAuthenticated}, Name: {context.User?.Identity?.Name}");
    await next();
});
app.UseAuthorization();
app.MapControllers();
app.Run();
