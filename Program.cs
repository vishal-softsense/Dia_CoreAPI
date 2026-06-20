using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.Repositories.Classes;
using Dia_CoreAPI.Repositories.Classes.Masters;
using Dia_CoreAPI.Repositories.Classes.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Services.Classes.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Repositories.Implementations;
using Dia_CoreAPI.Repositories.Interfaces;
using Dia_CoreAPI.Repositories.Interfaces.Masters;
using Dia_CoreAPI.Services.Classes;
using Dia_CoreAPI.Services.Classes.Masters;
using Dia_CoreAPI.Services.Implementations;
using Dia_CoreAPI.Services.Interfaces;
using Dia_CoreAPI.Services.Interfaces.Masters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dia Core API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<ISqlDataAccess, SqlDataAccess>();
builder.Services.AddScoped<IMasterRepository, MasterRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<IMenuService, MenuService>();

builder.Services.AddScoped<IAllMasterRepository, AllMasterRepository>();
builder.Services.AddScoped<IAllMasterService, AllMasterService>();

builder.Services.AddScoped<I4CMasterRepository, _4CMasterRepository>();
builder.Services.AddScoped<I4CMasterService, _4CMasterService>();

builder.Services.AddScoped<IPurchaseParcelRepository, PurchaseParcelRepository>();
builder.Services.AddScoped<IPurchaseParcelService, PurchaseParcelService>();

// Stock & Account
builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.StockAccount.IParcelItemMasterRepository, Dia_CoreAPI.Repositories.Implementations.StockAccount.ParcelItemMasterRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.StockAccount.IParcelItemMasterService, Dia_CoreAPI.Services.Implementations.StockAccount.ParcelItemMasterService>();

// Masters Registration
builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.Masters.IUserMasterRepository, Dia_CoreAPI.Repositories.Implementations.Masters.UserMasterRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.Masters.IUserMasterService, Dia_CoreAPI.Services.Implementations.Masters.UserMasterService>();

builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.Masters.IUserRoleRepository, Dia_CoreAPI.Repositories.Implementations.Masters.UserRoleRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.Masters.IUserRoleService, Dia_CoreAPI.Services.Classes.Masters.UserRoleService>();



// Setting Master Registration
builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.Masters.ISettingRepository, Dia_CoreAPI.Repositories.Implementations.Masters.SettingRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.Masters.ISettingService, Dia_CoreAPI.Services.Classes.Masters.SettingService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:Key"]))
            };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var uploadPath = Path.Combine(builder.Environment.ContentRootPath, "Upload");
if (!Directory.Exists(uploadPath))
{
    Directory.CreateDirectory(uploadPath);
}

app.UseCors("AllowAll");

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/Upload"
});

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


