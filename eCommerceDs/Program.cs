using eCommerceDs.AutoMappers;
using eCommerceDs.DTOs;
using eCommerceDs.Models;
using eCommerceDs.Repository;
using eCommerceDs.Services;
using eCommerceDs.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddDbContext<eCommerceDsContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Connection"));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
});

// Configure security
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = false,
                   ValidateAudience = false,
                   ValidateLifetime = true,
                   ValidateIssuerSigningKey = true,
                   IssuerSigningKey = new SymmetricSecurityKey(
                     Encoding.UTF8.GetBytes(builder.Configuration["JWTKey"]))
               });

// Setting up security in Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
        "JWT Authentication Using Bearer Scheme. \r\n\r " +
        "Enter the word 'Bearer' followed by a space and the authentication token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
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
                        new string[]{}
                    }
                });

});

//CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Validators
builder.Services.AddScoped<IValidator<GroupInsertDTO>, GroupInsertValidator>();
builder.Services.AddScoped<IValidator<GroupUpdateDTO>, GroupUpdateValidator>();
builder.Services.AddScoped<IValidator<MusicGenreInsertDTO>, MusicGenreInsertValidator>();
builder.Services.AddScoped<IValidator<MusicGenreUpdateDTO>, MusicGenreUpdateValidator>();
builder.Services.AddScoped<IValidator<RecordInsertDTO>, RecordInsertValidator>();
builder.Services.AddScoped<IValidator<RecordUpdateDTO>, RecordUpdateValidator>();

// Services
builder.Services.AddTransient<IFileManagerService, FileManagerService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IMusicGenreService, MusicGenreService>();
builder.Services.AddScoped<IRecordService, RecordService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddTransient<HashService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICartDetailService, CartDetailService>();

// Mappers
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Repositories
builder.Services.AddScoped<IGroupRepository<Group>, GroupRepository>();
builder.Services.AddScoped<IMusicGenreRepository<MusicGenre>, MusicGenreRepository>();
builder.Services.AddScoped<IRecordRepository<Record>, RecordRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICartDetailRepository, CartDetailRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICartDetailRepository, CartDetailRepository>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<eCommerceDsContext>();
    var retryCount = 0;
    const int maxRetries = 10;
    
    while (retryCount < maxRetries)
    {
        try
        {
            await context.Database.MigrateAsync();
            break;
        }
        catch (Exception)
        {
            retryCount++;
            if (retryCount == maxRetries)
                throw;
            await Task.Delay(2000);
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles(); 

app.UseCors("AllowAll");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
