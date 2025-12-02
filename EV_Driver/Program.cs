using System.Text;
using System.Text.Json.Serialization;
using BusinessObject;
using BusinessObject.DTOs;
using EV_Driver.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Service.BackgroundService;
using Service.Implementations;
using Service.Interfaces;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IStationService, StationService>();
builder.Services.AddScoped<IBatteryService, BatteryService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();
builder.Services.AddScoped<IStationStaffService, StationStaffService>();
builder.Services.AddScoped<IBatterySwapService, BatterySwapService>();
builder.Services.AddScoped<IBatteryTypeService, BatteryTypeService>();
builder.Services.AddScoped<IStationBatterySlotService, StationBatterySlotService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IFileService,FileService>();
builder.Services.AddScoped<IEmailTemplateLoaderService, EmailTemplateLoaderService>();
builder.Services.AddScoped<IStationService, StationService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IStaffInventoryBatteryService, StaffInventoryBatteryService>();
builder.Services.AddScoped<IBillingHistoryService, BillingHistoryService>();
builder.Services.AddScoped<IBatteryCoordinationService, BatteryCoordinationService>();
builder.Services.AddScoped<IStaffManagementBatteryService, StaffManagementBatteryService>();
builder.Services.AddScoped<IAbsenceService, AbsenceService>();
builder.Services.AddScoped<IReassignmentService, ReassignmentService>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
builder.Services.AddScoped<IAdminManagementBatteryService, AdminManagementBatteryService>();
builder.Services.AddHostedService<SubscriptionResetBackgroundService>();
builder.Services.AddHostedService<BatteryStationResetBackgroundService>();
builder.Services.AddScoped<IAdminRevenueService, AdminRevenueService>();
builder.Services.AddScoped<ISubscriptionPaymentService,SubscriptionPaymentService>();
builder.Services.AddScoped<IBatterySwapResponseService, BatterySwapResponseService>();
builder.Services.AddScoped<IPaymentManagementService, PaymentManagementService>();

// Add JWT authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
            ClockSkew = TimeSpan.Zero,
           
        };
    });

// Configure JSON options to handle enums
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EV Driver Authentication API",
        Version = "v1",
        Description = "API for EV Driver authentication system with 3-layer architecture"
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
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
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()      // Allow all origins
            .AllowAnyMethod()      // Allow all HTTP methods
            .AllowAnyHeader();     // Allow all headers
    });
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetSection("ConnectionStrings:RedisConnection").Value;
});

var app = builder.Build();

// Configure the HTTP request pipeline
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EV Driver API v1");
    //c.RoutePrefix = string.Empty; // Makes Swagger available at root
});
//}
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<GlobalExceptionMiddleware>();

using (var scope = app.Services.CreateScope()) {
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}
app.Run();