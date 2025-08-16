using ChatClubAPI.Data;
using ChatClubAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<DbService>();
builder.Services.AddScoped<CalculateService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddDbContext<ClubChatContext>(
        options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJs", policy =>
    {
        //policy.WithOrigins("https://localhost:3000") // IP Front END
        //      .AllowAnyHeader()
        //      .AllowAnyMethod()
        //      .AllowCredentials();
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition(
        name: JwtBearerDefaults.AuthenticationScheme,
        securityScheme: new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Enter the Bearer Authorization : 'Bearer Genreated-JWT-Token'",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            }, new string[] { }
        }
    });
});

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // read token from cookie
            foreach (var cookie in context.Request.Cookies)
            {
                Console.WriteLine($"{cookie.Key}: {cookie.Value}");
            }

            var accessToken = context.Request.Cookies["NEARSIP_ACCESS"];
            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },

        OnAuthenticationFailed = async context =>
        {
            // when access token failed, check if it's expired
            if (context.Exception is SecurityTokenExpiredException)
            {

                var refreshToken = context.Request.Cookies["NEARSIP_REFRESH"];
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();
                    var newTokens = await tokenService.RefreshTokenAsync(refreshToken);

                    if (newTokens != null)
                    {
                        // set new cookies
                        var cookieOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = false,
                            SameSite = SameSiteMode.None,
                            Expires = DateTime.UtcNow.AddDays(7),
                            Path = "/"
                        };

                        var refreshCookieOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = false,   
                            SameSite = SameSiteMode.None,
                            Expires = DateTime.UtcNow.AddDays(7),
                            Path = "/"
                        };

                        context.Response.Cookies.Append("NEARSIP_ACCESS", newTokens!.AccessToken!, cookieOptions);
                        context.Response.Cookies.Append("NEARSIP_REFRESH", newTokens!.RefreshToken!, refreshCookieOptions);

                        // create new claims Principal 
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var principal = tokenHandler.ValidateToken(newTokens.AccessToken, options.TokenValidationParameters, out var validatedToken);

                        context.Principal = principal;
                        context.Success();

                        return;
                    }
                }

                // refresh fail status 401
                context.Response.StatusCode = 401;
                context.Response.Headers.Add("Token-Expired", "true");
                await context.Response.WriteAsync("Access denied. Please login again.");
            }
        }
    };

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["JwtConfig:Issuer"],
        ValidAudiences = builder.Configuration.GetSection("JwtConfig:Audience").Get<string[]>(),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:Key"]!)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero
    };

    //options.RequireHttpsMetadata = false;
    //options.SaveToken = true;
    //options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    //{
    //    ValidIssuer = builder.Configuration["JwtConfig:Issuer"],
    //    ValidAudiences = builder.Configuration.GetSection("JwtConfig:Audience").Get<string[]>(),
    //    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:Key"]!)),
    //    ValidateIssuer = true,
    //    ValidateAudience = true,
    //    ValidateLifetime = true,
    //    ValidateIssuerSigningKey = true,
    //    ClockSkew = TimeSpan.Zero
    //};
});
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseStaticFiles();
app.MapPost("/api/upload", async (HttpRequest request) =>
{
    var form = await request.ReadFormAsync();
    var file = form.Files["file"];

    if (file is null || file.Length == 0)
        return Results.BadRequest("No file uploaded");

    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "images", "users");

    Directory.CreateDirectory(folderPath); // ?????????????
    var filePath = Path.Combine(folderPath, fileName);

    using var stream = new FileStream(filePath, FileMode.Create);
    await file.CopyToAsync(stream);

    var publicPath = $"/uploads/images/users/{fileName}";
    return Results.Ok(new { path = publicPath });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();

app.UseCors("AllowNextJs");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
