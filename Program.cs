using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.Repositories;
using Dia_CoreAPI.Repositories.Classes;
using Dia_CoreAPI.Repositories.Classes.Dashboard;
using Dia_CoreAPI.Repositories.Classes.Masters;
using Dia_CoreAPI.Repositories.Classes.Parcel.Parcel_Reports;
using Dia_CoreAPI.Repositories.Classes.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Repositories.Classes.ParcelDashboard;
using Dia_CoreAPI.Repositories.Classes.StockAccount;
using Dia_CoreAPI.Repositories.Interfaces;
using Dia_CoreAPI.Repositories.Interfaces.Dashboard;
using Dia_CoreAPI.Repositories.Interfaces.Masters;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Reports;
using Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Repositories.Interfaces.ParcelDashboard;
using Dia_CoreAPI.Repositories.Interfaces.StockAccount;
using Dia_CoreAPI.Services;
using Dia_CoreAPI.Services.Classes;
using Dia_CoreAPI.Services.Classes.Dashboard;
using Dia_CoreAPI.Services.Classes.Masters;
using Dia_CoreAPI.Services.Classes.Parcel.Parcel_Reports;
using Dia_CoreAPI.Services.Classes.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Services.Classes.ParcelDashboard;
using Dia_CoreAPI.Services.Classes.StockAccount;
using Dia_CoreAPI.Services.Interfaces;
using Dia_CoreAPI.Services.Interfaces.Dashboard;
using Dia_CoreAPI.Services.Interfaces.Masters;
using Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Reports;
using Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Transaction;
using Dia_CoreAPI.Services.Interfaces.ParcelDashboard;
using Dia_CoreAPI.Services.Interfaces.StockAccount;
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
builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.StockAccount.IParcelItemMasterRepository, Dia_CoreAPI.Repositories.Classes.StockAccount.ParcelItemMasterRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.StockAccount.IParcelItemMasterService, Dia_CoreAPI.Services.Classes.StockAccount.ParcelItemMasterService>();
builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.StockAccount.IAccGrpMasterRepository, Dia_CoreAPI.Repositories.Classes.StockAccount.AccGrpMasterRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.StockAccount.IAccGrpMasterService, Dia_CoreAPI.Services.Classes.StockAccount.AccGrpMasterService>();

builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.StockAccount.ISolitaireItemMasterRepository, Dia_CoreAPI.Repositories.Classes.StockAccount.SolitaireItemMasterRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.StockAccount.ISolitaireItemMasterService, Dia_CoreAPI.Services.Classes.StockAccount.SolitaireItemMasterService>();

// Masters Registration
builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.Masters.IUserMasterRepository, Dia_CoreAPI.Repositories.Classes.Masters.UserMasterRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.Masters.IUserMasterService, Dia_CoreAPI.Services.Classes.Masters.UserMasterService>();

builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.Masters.IUserRoleRepository, Dia_CoreAPI.Repositories.Classes.Masters.UserRoleRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.Masters.IUserRoleService, Dia_CoreAPI.Services.Classes.Masters.UserRoleService>();

builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.Masters.ILocationRepository, Dia_CoreAPI.Repositories.Classes.Masters.LocationRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.Masters.ILocationService, Dia_CoreAPI.Services.Classes.Masters.LocationService>();

builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.Masters.IInvoiceRepository, Dia_CoreAPI.Repositories.Classes.Masters.InvoiceRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.Masters.IInvoiceService, Dia_CoreAPI.Services.Classes.Masters.InvoiceService>();

builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.Masters.IDiamondOriginRepository, Dia_CoreAPI.Repositories.Classes.Masters.DiamondOriginRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.Masters.IDiamondOriginService, Dia_CoreAPI.Services.Classes.Masters.DiamondOriginService>();

// Setting Master Registration
builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.Masters.ISettingRepository, Dia_CoreAPI.Repositories.Classes.Masters.SettingRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.Masters.ISettingService, Dia_CoreAPI.Services.Classes.Masters.SettingService>();

// Jewellery Master Registration
builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.Masters.IJewelleryRepository, Dia_CoreAPI.Repositories.Classes.Masters.JewelleryRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.Masters.IJewelleryService, Dia_CoreAPI.Services.Classes.Masters.JewelleryService>();


// Grading Master Registration
builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.Masters.IGradingRepository, Dia_CoreAPI.Repositories.Classes.Masters.GradingRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.Masters.IGradingService, Dia_CoreAPI.Services.Classes.Masters.GradingService>();

builder.Services.AddScoped<ISyncSettingService, SyncSettingService>();
builder.Services.AddScoped<ISyncSettingRepository, SyncSettingRepository>();

builder.Services.AddScoped<IMenuSearchRepository, MenuSearchRepository>();
builder.Services.AddScoped<IMenuSearchService, MenuSearchService>();

builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IParcelDashboardRepository, ParcelDashboardRepository>();
builder.Services.AddScoped<IParcelDashboardService, ParcelDashboardService>();

builder.Services.AddScoped<IJewStockMasterRepository, JewStockMasterRepository>();
builder.Services.AddScoped<IJewStockMasterService, JewStockMasterService>();

builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports.ISolitaireStockSearchReportRepository, Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Reports.SolitaireStockSearchReportRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports.ISolitaireStockSearchReportService, Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Reports.SolitaireStockSearchReportService>();

builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports.ISolitaireAdminStockReportRepository, Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Reports.SolitaireAdminStockReportRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports.ISolitaireAdminStockReportService, Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Reports.SolitaireAdminStockReportService>();

builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports.ISolitaireStockHistoryReportRepository, Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Reports.SolitaireStockHistoryReportRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports.ISolitaireStockHistoryReportService, Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Reports.SolitaireStockHistoryReportService>();

builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports.ISolitarieGradingComparisonReportRepository, Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Reports.SolitarieGradingComparisonReportRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports.ISolitarieGradingComparisonReportService, Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Reports.SolitarieGradingComparisonReportService>();

//parcel dashbord
builder.Services.AddScoped<IParcelStockAnalysisRepository, ParcelStockAnalysisRepository>();
builder.Services.AddScoped<IParcelStockAnalysisService, ParcelStockAnalysisService>();

builder.Services.AddScoped<IParcelSaleAnalysisRepository, ParcelSaleAnalysisRepository>();
builder.Services.AddScoped<IParcelSaleAnalysisService, ParcelSaleAnalysisService>();

builder.Services.AddScoped<IParcelPurchaseAnalysisRepository, ParcelPurchaseAnalysisRepository>();
builder.Services.AddScoped<IParcelPurchaseAnalysisService, ParcelPurchaseAnalysisService>();


// Solitaire Rate Revise Report
builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports.ISolitaireRateReviseReportRepository, Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Reports.SolitaireRateReviseReportRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports.ISolitaireRateReviseReportService, Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Reports.SolitaireRateReviseReportService>();

// Solitaire Purchase Report
builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports.ISolitairePurchaseReportRepository, Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Reports.SolitairePurchaseReportRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports.ISolitairePurchaseReportService, Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Reports.SolitairePurchaseReportService>();

// Solitaire Consignment In Report
builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports.ISolitaireConsignmentInReportRepository, Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Reports.SolitaireConsignmentInReportRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports.ISolitaireConsignmentInReportService, Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Reports.SolitaireConsignmentInReportService>();

// Solitaire Memo Out Report
builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports.ISolitaireMemoOutReportRepository, Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Reports.SolitaireMemoOutReportRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports.ISolitaireMemoOutReportService, Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Reports.SolitaireMemoOutReportService>();

// Solitaire Party Hold Report
builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports.ISolitairePartyHoldReportRepository, Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Reports.SolitairePartyHoldReportRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports.ISolitairePartyHoldReportService, Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Reports.SolitairePartyHoldReportService>();

// Solitaire Kapan Summary Report
builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports.ISolitaireKapanSummaryReportRepository, Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Reports.SolitaireKapanSummaryReportRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports.ISolitaireKapanSummaryReportService, Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Reports.SolitaireKapanSummaryReportService>();

// Solitaire Sale Report
builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports.ISolitaireSaleReportRepository, Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Reports.SolitaireSaleReportRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports.ISolitaireSaleReportService, Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Reports.SolitaireSaleReportService>();

// Solitaire Sale Performance Report
builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.Solitaires.Solitaires_Reports.ISolitaireSalePerformanceReportRepository, Dia_CoreAPI.Repositories.Classes.Solitaires.Solitaires_Reports.SolitaireSalePerformanceReportRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.Solitaires.Solitaires_Reports.ISolitaireSalePerformanceReportService, Dia_CoreAPI.Services.Classes.Solitaires.Solitaires_Reports.SolitaireSalePerformanceReportService>();

builder.Services.AddScoped<IMonthAnalysisReportRepository, MonthAnalysisReportRepository>();
builder.Services.AddScoped<IMonthAnalysisReportService, MonthAnalysisReportService>();
builder.Services.AddScoped<IPurchaseAnalysisReportRepository, PurchaseAnalysisReportRepository>();
builder.Services.AddScoped<IPurchaseAnalysisReportService, PurchaseAnalysisReportService>();
builder.Services.AddScoped<ISaleAnalysisReportRepository, SaleAnalysisReportRepository>();
builder.Services.AddScoped<ISaleAnalysisReportService, SaleAnalysisReportService>();
builder.Services.AddScoped<IStockAnalysisReportRepository, StockAnalysisReportRepository>();
builder.Services.AddScoped<IStockAnalysisReportService, StockAnalysisReportService>();


// Parcel StockSearch Report
builder.Services.AddScoped<Dia_CoreAPI.Repositories.Interfaces.Parcel.Parcel_Reports.IParcelStockSearchReportRepository, Dia_CoreAPI.Repositories.Classes.Parcel.Parcel_Reports.ParcelStockSearchReportRepository>();
builder.Services.AddScoped<Dia_CoreAPI.Services.Interfaces.Parcel.Parcel_Reports.IParcelStockSearchReportService, Dia_CoreAPI.Services.Classes.Parcel.Parcel_Reports.ParcelStockSearchReportService>();

// Parcel StockHistory Report
builder.Services.AddScoped<IParcelStockRegisterReportRepository, ParcelStockRegisterReportRepository>();
builder.Services.AddScoped<IParcelStockRegisterReportService, ParcelStockRegisterReportService>();

// Mix Purchase Report
builder.Services.AddScoped<IMixPurchaseReportRepository, MixPurchaseReportRepository>();
builder.Services.AddScoped<IMixPurchaseReportService, MixPurchaseReportService>();

// Mix MemoOut Report
builder.Services.AddScoped<IMixMemoOutReportRepository, MixMemoOutReportRepository>();
builder.Services.AddScoped<IMixMemoOutReportService, MixMemoOutReportService>();

// Mix Sale Report
builder.Services.AddScoped<IMixSaleReportRepository, MixSaleReportRepository>();
builder.Services.AddScoped<IMixSaleReportService, MixSaleReportService>();

// Mix ConsignmentIn Report
builder.Services.AddScoped<IMixConsignmentInReportRepository, MixConsignmentInReportRepository>();
builder.Services.AddScoped<IMixConsignmentInReportService, MixConsignmentInReportService>();

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

var uploadPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "Upload");
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


