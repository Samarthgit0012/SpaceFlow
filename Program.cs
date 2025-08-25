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
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    // Sign in settings
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configure authentication to return JSON responses for API requests
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        // Check if this is an API request
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
        
        // For non-API requests, redirect to login page
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    
    options.Events.OnRedirectToAccessDenied = context =>
    {
        // Check if this is an API request
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        }
        
        // For non-API requests, redirect to access denied page
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

// Add support for API controllers with JSON configuration
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Handle circular references in JSON serialization
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        
        // Set default policy for property names (optional)
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        
        // Set max depth to prevent deep nesting issues
        options.JsonSerializerOptions.MaxDepth = 32;
    });

// Add Razor Pages support for Identity UI
builder.Services.AddRazorPages();

// *** ADD SWAGGER ***
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SpaceFlow API",
        Version = "v1",
        Description = "API for SpaceFlow workspace booking system"
    });

    // Include XML comments if you have them
    // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // c.IncludeXmlComments(xmlPath);
});

// *** REGISTER REPOSITORIES ***
builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// *** REGISTER SERVICES ***
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPaymentService, RazorpayPaymentService>(); // Fixed: Use RazorpayPaymentService instead of DummyPaymentService
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Configure CORS for frontend applications
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });

    options.AddPolicy("Development",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

// Add memory cache
builder.Services.AddMemoryCache();

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// --- Database Seeding ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await SeedDatabase(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// --- HTTP Request Pipeline Configuration ---

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SpaceFlow API V1");
        c.RoutePrefix = "swagger";
    });
    app.UseCors("Development");
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseCors("AllowAll");
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

// Add a simple health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

try 
{
    app.Run();
}
catch (System.IO.IOException ex) when (ex.Message.Contains("address already in use"))
{
    Console.WriteLine("??  Port is already in use. Please:");
    Console.WriteLine("1. Stop any other instances of the application");
    Console.WriteLine("2. Or change the port in Properties/launchSettings.json");
    Console.WriteLine("3. Or run: dotnet run --urls \"http://localhost:5001;https://localhost:7001\"");
    throw;
}

// Database seeding method
static async Task SeedDatabase(ApplicationDbContext context)
{
    // Check if database has data
    if (await context.Workspaces.AnyAsync())
    {
        return; // Database already seeded
    }

    // Add sample workspaces
    var workspaces = new List<Workspace>
    {
        new Workspace
        {
            Name = "Conference Room A",
            Type = "Conference",
            Capacity = 10,
            PricePerHour = 50.00m,
            Amenities = "Projector, Whiteboard, Video Conferencing",
            IsAvailable = true,
            ImageUrl = "https://example.com/conference-a.jpg",
            CreatedDate = DateTime.UtcNow
        },
        new Workspace
        {
            Name = "Hot Desk 1",
            Type = "Hot Desk",
            Capacity = 1,
            PricePerHour = 15.00m,
            Amenities = "Wi-Fi, Power Outlet, Ergonomic Chair",
            IsAvailable = true,
            ImageUrl = "https://example.com/hotdesk-1.jpg",
            CreatedDate = DateTime.UtcNow
        },
        new Workspace
        {
            Name = "Private Office 1",
            Type = "Private Office",
            Capacity = 4,
            PricePerHour = 75.00m,
            Amenities = "Desk, Chairs, Phone, Wi-Fi, Storage",
            IsAvailable = true,
            ImageUrl = "https://example.com/office-1.jpg",
            CreatedDate = DateTime.UtcNow
        }
    };

    context.Workspaces.AddRange(workspaces);
    await context.SaveChangesAsync();

    // Add sample test user for testing
    var testUser = new ApplicationUser
    {
        Id = "test-user-123",
        UserName = "testuser@example.com",
        Email = "testuser@example.com",
        EmailConfirmed = true,
        FullName = "Test User"
    };
    
    context.Users.Add(testUser);
    await context.SaveChangesAsync();

    // Add sample bookings for testing
    var bookings = new List<Booking>
    {
        new Booking
        {
            Id = 1,
            ApplicationUserId = "test-user-123",
            WorkspaceId = workspaces[0].Id,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            Status = BookingStatus.Pending,
            TotalPrice = 100.00m
        },
        new Booking
        {
            Id = 2,
            ApplicationUserId = "test-user-123",
            WorkspaceId = workspaces[1].Id,
            StartTime = DateTime.UtcNow.AddDays(2),
            EndTime = DateTime.UtcNow.AddDays(2).AddHours(4),
            Status = BookingStatus.Pending,
            TotalPrice = 60.00m
        }
    };

    context.Bookings.AddRange(bookings);
    await context.SaveChangesAsync();
}