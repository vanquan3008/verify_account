using be_account.Data;
using be_account.Models;
using be_account.Repositories;
using be_account.Repositories.IRepositories;
using be_account.Services;
using be_account.Services.IServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle


// Connect with database

builder.Services.AddDbContext<DataContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresSQL"))
);

builder.Services.AddAuthorization();

builder.Services.AddIdentity<User,Role>()
    .AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();

// Adding Jwt Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
        };
    });


//builder.Services.AddAuthentication().AddGoogle(googleOptions =>
//{
//    googleOptions.SaveTokens = true;
//    IConfigurationSection googleAuthNSection =
//        builder.Configuration.GetSection("Authentication:Google");

//    googleOptions.ClientId = googleAuthNSection["ClientId"];
//    googleOptions.ClientSecret = googleAuthNSection["ClientSecret"];


//});


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
   .AddCookie(
        option => option.LoginPath = "/signin"
    )
   .AddGoogle(options =>
   { 
       options.SaveTokens = true;
       IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
       options.ClientId = googleAuthNSection["ClientId"];
       options.ClientSecret = googleAuthNSection["ClientSecret"];
   });

builder.Services.Configure<IdentityOptions>(options =>
{
    //Password settings.

    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;

});


builder.Services.AddEndpointsApiExplorer();
//CORS
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: MyAllowSpecificOrigins,
        policy =>{
            policy.WithOrigins("https://localhost:5001")
             .AllowAnyHeader()
             .AllowCredentials()
            .AllowAnyMethod();
        });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "jwtToken_Auth_API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
});
//Repository
builder.Services.AddScoped<IUserRepository, UserRepository>();


//Service
builder.Services.AddScoped<IJwtService , JwtService>();
builder.Services.AddScoped<IAccountService , AccountService>();

var app = builder.Build();
// Check connection database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<DataContext>();
        context.Database.EnsureCreated(); 
        logger.LogInformation("Connected to the database successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while connecting to the database.");
    }
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
