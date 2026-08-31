using System.Text;
using backend.Application.Services;
using backend.Data;
using backend.Domain.Entities;
using backend.Hubs;
using backend.Infrastructure.Storage;
using backend.Middleware;
using DotNetEnv;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;


// ─────────────────────────────────────────────────────────────────────────────
// 1. Load .env file (development convenience)
// ─────────────────────────────────────────────────────────────────────────────
Env.Load();

// ─────────────────────────────────────────────────────────────────────────────
// 2. Configure Serilog before the host is built
// ─────────────────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true)
        .AddEnvironmentVariables()
        .Build())
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // ─────────────────────────────────────────────────────────────────────────
    // 3. PostgreSQL + EF Core
    // ─────────────────────────────────────────────────────────────────────────
    var dbHost = Env.GetString("DB_HOST") ?? "localhost";
    var dbPort = Env.GetString("DB_PORT") ?? "5432";
    var dbName = Env.GetString("DB_NAME") ?? "resume_management";
    var dbUser = Env.GetString("DB_USER") ?? "postgres";
    var dbPassword = Env.GetString("DB_PASSWORD") ?? "";
    var dbCaPath = Env.GetString("DB_CA_PATH");

    var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";

    // SSL with Aiven CA certificate
    if (!string.IsNullOrWhiteSpace(dbCaPath) && File.Exists(dbCaPath))
    {
        connectionString += $";Ssl Mode=VerifyFull;Root Certificate={dbCaPath}";
    }

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));

    // ─────────────────────────────────────────────────────────────────────────
    // 4. ASP.NET Core Identity
    // ─────────────────────────────────────────────────────────────────────────
    builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

    // ─────────────────────────────────────────────────────────────────────────
    // 5. JWT Authentication
    // ─────────────────────────────────────────────────────────────────────────
    var jwtSecret = Env.GetString("JWT_SECRET")
        ?? throw new InvalidOperationException("JWT_SECRET environment variable is not set.");
    var jwtIssuer = Env.GetString("JWT_ISSUER") ?? "resume-management-backend";
    var jwtAudience = Env.GetString("JWT_AUDIENCE") ?? "resume-management-frontend";

    var key = Encoding.UTF8.GetBytes(jwtSecret);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // allow HTTP in dev
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        // Support JWT from query string for SignalR WebSocket connections
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                var path = ctx.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    ctx.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    })
    .AddGoogle(options =>
    {
        options.ClientId = Env.GetString("GOOGLE_CLIENT_ID") ?? "";
        options.ClientSecret = Env.GetString("GOOGLE_CLIENT_SECRET") ?? "";
        options.CallbackPath = "/api/auth/google/callback";
    })
    .AddGitHub(options =>
    {
        options.ClientId = Env.GetString("GITHUB_CLIENT_ID") ?? "";
        options.ClientSecret = Env.GetString("GITHUB_CLIENT_SECRET") ?? "";
        options.CallbackPath = "/api/auth/github/callback";
        options.Scope.Add("user:email");
    });

    // ─────────────────────────────────────────────────────────────────────────
    // 6. Authorization Policies
    // ─────────────────────────────────────────────────────────────────────────
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("RequireCandidate", p => p.RequireRole("Candidate", "Administrator"))
        .AddPolicy("RequireRecruiter", p => p.RequireRole("Recruiter", "Administrator"))
        .AddPolicy("RequireAdmin", p => p.RequireRole("Administrator"));

    // ─────────────────────────────────────────────────────────────────────────
    // 7. CORS
    // ─────────────────────────────────────────────────────────────────────────
    var originsRaw = Env.GetString("APP_ORIGINS") ?? "http://localhost:5173";
    var origins = originsRaw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    builder.Services.AddCors(options =>
        options.AddPolicy("AllowFrontend", policy =>
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()));

    // ─────────────────────────────────────────────────────────────────────────
    // 8. Controllers + JSON
    // ─────────────────────────────────────────────────────────────────────────
    builder.Services.AddControllers()
        .AddJsonOptions(opts =>
        {
            opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    // ─────────────────────────────────────────────────────────────────────────
    // 9. FluentValidation
    // ─────────────────────────────────────────────────────────────────────────
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // ─────────────────────────────────────────────────────────────────────────
    // 10. SignalR
    // ─────────────────────────────────────────────────────────────────────────
    builder.Services.AddSignalR();

    // ─────────────────────────────────────────────────────────────────────────
    // 11. Swagger / OpenAPI
    // ─────────────────────────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "CV Management API",
            Version = "v1",
            Description = "Recruitment platform API"
        });

        // JWT bearer support in Swagger UI
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header. Example: 'Bearer {token}'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                []
            }
        });
    });

    // ─────────────────────────────────────────────────────────────────────────
    // 12. Infrastructure services
    // ─────────────────────────────────────────────────────────────────────────

    // Map env vars to IConfiguration keys expected by CloudinaryStorageService
    builder.Configuration["Cloudinary:CloudName"] = Env.GetString("CLOUDINARY_CLOUD_NAME");
    builder.Configuration["Cloudinary:ApiKey"] = Env.GetString("CLOUDINARY_API_KEY");
    builder.Configuration["Cloudinary:ApiSecret"] = Env.GetString("CLOUDINARY_API_SECRET");

    builder.Services.AddSingleton<IFileStorageService, CloudinaryStorageService>();
    builder.Services.AddHttpContextAccessor();

    // ─────────────────────────────────────────────────────────────────────────
    // 13. Application Services
    // ─────────────────────────────────────────────────────────────────────────
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IProfileService, ProfileService>();
    builder.Services.AddScoped<IProjectService, ProjectService>();
    builder.Services.AddScoped<IAttributeService, AttributeService>();
    builder.Services.AddScoped<IPositionService, PositionService>();
    builder.Services.AddScoped<IAccessRuleEvaluationService, AccessRuleEvaluationService>();

    // ─────────────────────────────────────────────────────────────────────────
    // Build the app
    // ─────────────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ─────────────────────────────────────────────────────────────────────────
    // Middleware pipeline (order matters)
    // ─────────────────────────────────────────────────────────────────────────
    app.UseGlobalExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CV Management API v1"));
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowFrontend");
    app.UseSerilogRequestLogging();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseUserStatusCheck();

    app.MapControllers();
    app.MapHub<DiscussionHub>("/hubs/discussion");

    // ─────────────────────────────────────────────────────────────────────────
    // Auto-apply EF Core migrations and seed data on startup
    // ─────────────────────────────────────────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        if (app.Environment.IsDevelopment())
        {
            await db.Database.MigrateAsync();
            Log.Information("Database migrations applied.");
        }

        // Seed roles (idempotent)
        foreach (var roleName in new[] { "Candidate", "Recruiter", "Administrator" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                Log.Information("Seeded role: {Role}", roleName);
            }
        }
    }

    Log.Information("CV Management API starting...");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start.");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
