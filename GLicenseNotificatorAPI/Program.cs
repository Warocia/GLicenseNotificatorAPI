using GLicenseNotificatorAPI.Authentication;
using GLicenseNotificatorAPI.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(option => { 
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
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

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddDbContext<DataContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("WebApiDatabase")));

builder.Services.AddAuthorization();

builder.Services.AddCors(policyBuilder =>
    policyBuilder.AddPolicy("devPolicy", policy =>
        policy.WithOrigins("http://localhost:3000")
        .SetIsOriginAllowedToAllowWildcardSubdomains()
        .AllowAnyMethod()
        .AllowAnyHeader())
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.MapPost("/security/createToken",
[AllowAnonymous] (User user, DataContext db) =>
{
    try
    {
        if (user == null || user.UserName == null || user.UserName == ""
                || user.Password == null || user.Password == "")
        {
            return Results.Unauthorized();
        }

        //Database is empty, create Admin user
        if (db.Users.Count() < 1)
        {
            var userName = builder.Configuration["MainUser:UserName"];
            var password = builder.Configuration["MainUser:Password"];
            var email = builder.Configuration["MainUser:Email"];

            if (userName != null && password != null && email != null)
            {
                MyDbContextSeeder.Seed(db, userName, email, password);
            }
        }


        var dbUser = db.Users.FirstOrDefault(u => u.UserName == user.UserName);

        if (dbUser != null)
        {
            //Check password
            GLicenseNotificatorAPI.Crypto.PasswordHasher passwordHasher = new GLicenseNotificatorAPI.Crypto.PasswordHasher();
            if(!passwordHasher.Check(dbUser.Password, user.Password))
            {
                return Results.Unauthorized();
            }

            string? issuer = builder.Configuration["Jwt:Issuer"];
            string? audience = builder.Configuration["Jwt:Audience"];
            string? jwtKey = builder.Configuration["Jwt:Key"];

            if (issuer == null || audience == null || jwtKey == null)
            {
                return Results.Unauthorized();
            }

            var key = Encoding.ASCII.GetBytes(jwtKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim("Id", Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
             }),
                Expires = DateTime.UtcNow.AddMinutes(60),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials
                (new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha512Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);
            var stringToken = tokenHandler.WriteToken(token);

            dynamic jsonObject = new ExpandoObject();
            jsonObject.JwtToken = stringToken;
            jsonObject.IsAdmin = dbUser.IsAdmin;
           
            return Results.Ok(jsonObject);
        }
        return Results.Unauthorized();
    }
    catch (Exception)
    {
        return Results.Unauthorized(); ;
    }
});

app.MapControllers();
app.UseCors("devPolicy");

app.Run();
