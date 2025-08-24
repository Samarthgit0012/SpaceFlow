using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SpaceFlow.Data;
using SpaceFlow.Repositories.Implementation;
using SpaceFlow.Repositories.Interfaces;
using SpaceFlow.Repositories.Models;
using SpaceFlow.Services.Implementation;
using SpaceFlow.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// --- Service Configuration ---

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configure EF Core with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configure ASP.NET Identity with your custom ApplicationUser
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add support for API controllers
builder.Services.AddControllers();

// Add Razor Pages support for Identity UI
builder.Services.AddRazorPages();

// *** ADD SWAGGER ***
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// *** REGISTER YOUR CUSTOM SERVICES AND REPOSITORIES ***
// This tells the application how to create your custom classes when they're needed.
builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, DummyPaymentService>();


var app = builder.Build();

// --- HTTP Request Pipeline Configuration ---

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    // *** USE SWAGGER ***
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map API controllers
app.MapControllers();

// Map Razor Pages for Identity UI
app.MapRazorPages();

app.Run();