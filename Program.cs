using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using puzzle_alloc.Data;
using puzzle_alloc.Middleware;
using puzzle_alloc.Models.Entities;
using puzzle_alloc.Services;


var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
const string folderName = "Logs";

var projectDir = builder.Environment.ContentRootPath;
var solutionRoot = Directory.GetParent(projectDir)?.FullName ?? projectDir;
var logDir = Path.Combine(solutionRoot, folderName);
log4net.GlobalContext.Properties["LogRoot"] = logDir;

Directory.CreateDirectory(logDir);

builder.Logging.ClearProviders();

var log4netConfigPath = Path.Combine(projectDir, "log4net.config");
builder.Logging.AddLog4Net(log4netConfigPath);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(config.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


var jwtKey = config["Jwt:Key"] ?? "super_secret_dev_key_change_in_prod";
var jwtIssuer = config["Jwt:Issuer"] ?? "PuzzleAllocIssuer";
var jwtAudience = config["Jwt:Audience"] ?? "PuzzleAllocAudience";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; 
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; 
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;   
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };

   
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var authHeader = ctx.Request.Headers.Authorization.ToString();
                var logger = ctx.HttpContext.RequestServices
                               .GetRequiredService<ILoggerFactory>()
                               .CreateLogger("Auth");
                logger.LogInformation("Authorization header present? {Present}", !string.IsNullOrWhiteSpace(authHeader));
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = ctx =>
            {
                var logger = ctx.HttpContext.RequestServices
                               .GetRequiredService<ILoggerFactory>()
                               .CreateLogger("Auth");
                logger.LogError(ctx.Exception, "JWT auth failed: {Message}", ctx.Exception.Message);
                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("UserOnly", p => p.RequireRole("User"));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PuzzleAlloc API", Version = "v1" });

  
    const string schemeName = "Bearer";
    var bearerScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Paste the raw JWT here (do NOT prefix with 'Bearer ').",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = schemeName
        }
    };

    c.AddSecurityDefinition(schemeName, bearerScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { bearerScheme, Array.Empty<string>() }
    });
});


builder.Services.AddCors(opt =>
{
    opt.AddPolicy("frontend", p => p
        .WithOrigins(
            "http://localhost:5173",
            "http://localhost:5174"
        )
        .AllowAnyHeader()
        .AllowAnyMethod());
});


builder.Services.AddScoped<IAllocationEngine, AllocationEngine>();

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
      
        options.ConfigObject.AdditionalItems["persistAuthorization"] = true;
    });
}
app.UseHttpsRedirection();
app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();