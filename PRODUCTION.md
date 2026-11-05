# Production Deployment Guide

## Overview

This guide provides step-by-step instructions for deploying the Sales Operations Dashboard to a production environment with a real database, authentication, authorization, and audit logging.

## Architecture

### Development (Current)
```
Blazor App (In-Memory)
└── OrderDataService (In-Memory Store)
```

### Production (Target)
```
┌─────────────────────────────────────┐
│     Blazor WebAssembly App          │
│  (Production-optimized JavaScript)  │
└──────────────┬──────────────────────┘
               │ HTTPS/REST API
┌──────────────▼──────────────────────┐
│    ASP.NET Core 8 Backend API       │
│  - Authentication (JWT)             │
│  - Authorization (Role-based)       │
│  - Business Logic                   │
│  - Audit Logging                    │
│  - Input Validation                 │
│  - Rate Limiting                    │
└──────────────┬──────────────────────┘
               │ Entity Framework Core
┌──────────────▼──────────────────────┐
│     SQL Server / PostgreSQL         │
│  - Orders Table                     │
│  - Users & Roles Table              │
│  - Audit Logs Table                 │
│  - Session/Token Storage            │
└─────────────────────────────────────┘
```

## Step 1: Create ASP.NET Core Backend API

### 1.1 Create API Project

```bash
dotnet new webapi -n SalesOperationsAPI -f net8.0
cd SalesOperationsAPI
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
```

### 1.2 Create Database Models

**Data/OrderEntity.cs**
```csharp
namespace SalesOperationsAPI.Data;

public class OrderEntity
{
    public int Id { get; set; }
    public string OrderID { get; set; } = null!;
    public string Customer { get; set; } = null!;
    public string Region { get; set; } = null!;
    public string Status { get; set; } = "New";
    public DateTime OrderDate { get; set; }
    public DateTime DeliveryDate { get; set; }
    public string SalesRepresentative { get; set; } = null!;
    public decimal OrderValue { get; set; }
    public decimal ForecastValue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
```

**Data/AuditLogEntity.cs**
```csharp
namespace SalesOperationsAPI.Data;

public class AuditLogEntity
{
    public int Id { get; set; }
    public string Action { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public string? EntityId { get; set; }
    public string? UserId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
```

**Data/ApplicationUser.cs**
```csharp
using Microsoft.AspNetCore.Identity;

namespace SalesOperationsAPI.Data;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = null!;
    public string Department { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
```

### 1.3 Create DbContext

**Data/ApplicationDbContext.cs**
```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SalesOperationsAPI.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<OrderEntity> Orders { get; set; } = null!;
    public DbSet<AuditLogEntity> AuditLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure OrderEntity
        builder.Entity<OrderEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrderID).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Customer);
            entity.HasIndex(e => e.OrderDate);
            
            entity.Property(e => e.OrderValue)
                .HasColumnType("decimal(18,2)");
            entity.Property(e => e.ForecastValue)
                .HasColumnType("decimal(18,2)");
        });

        // Configure AuditLogEntity
        builder.Entity<AuditLogEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.UserId);
        });
    }
}
```

### 1.4 Setup Program.cs for API

**Program.cs**
```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SalesOperationsAPI.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SalesManagerPolicy", policy =>
        policy.RequireRole("SalesManager", "Admin"));
    options.AddPolicy("SalesRepPolicy", policy =>
        policy.RequireRole("SalesRep", "SalesManager", "Admin"));
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
        policy.WithOrigins(builder.Configuration["AllowedOrigins"]?.Split(";") ?? Array.Empty<string>())
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// Services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazorApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    
    // Seed default roles and admin user
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    
    await SeedDataAsync(roleManager, userManager);
}

app.Run();

static async Task SeedDataAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
{
    var roles = new[] { "Admin", "SalesManager", "SalesRep" };
    
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var adminEmail = "admin@salesops.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            Email = adminEmail,
            UserName = adminEmail,
            FullName = "Administrator",
            Department = "IT",
            EmailConfirmed = true
        };
        
        await userManager.CreateAsync(adminUser, "AdminPassword123!");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}
```

**appsettings.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SalesOperationsDb;Trusted_Connection=true;"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-minimum-256-bits-long-change-in-production",
    "Issuer": "https://yourdomain.com",
    "Audience": "https://yourdomain.com/api",
    "ExpirationMinutes": 60
  },
  "AllowedOrigins": "https://localhost:7201;https://yourdomain.com",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 1.5 Create API Services

**Services/IOrderService.cs**
```csharp
using SalesOperationsAPI.Data;
using SalesOperationsAPI.DTOs;

namespace SalesOperationsAPI.Services;

public interface IOrderService
{
    Task<List<OrderDto>> GetAllOrdersAsync();
    Task<OrderDto?> GetOrderByIdAsync(string orderId);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, string userId);
    Task<OrderDto> UpdateOrderAsync(string orderId, UpdateOrderDto dto, string userId);
    Task DeleteOrderAsync(string orderId, string userId);
}
```

**Services/OrderService.cs**
```csharp
using Microsoft.EntityFrameworkCore;
using SalesOperationsAPI.Data;
using SalesOperationsAPI.DTOs;

namespace SalesOperationsAPI.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public OrderService(ApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<List<OrderDto>> GetAllOrdersAsync()
    {
        return await _context.Orders
            .Select(o => new OrderDto
            {
                OrderID = o.OrderID,
                Customer = o.Customer,
                Region = o.Region,
                Status = o.Status,
                OrderDate = o.OrderDate,
                DeliveryDate = o.DeliveryDate,
                SalesRepresentative = o.SalesRepresentative,
                OrderValue = o.OrderValue,
                ForecastValue = o.ForecastValue
            })
            .ToListAsync();
    }

    public async Task<OrderDto?> GetOrderByIdAsync(string orderId)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.OrderID == orderId);
        
        return order == null ? null : new OrderDto
        {
            OrderID = order.OrderID,
            Customer = order.Customer,
            Region = order.Region,
            Status = order.Status,
            OrderDate = order.OrderDate,
            DeliveryDate = order.DeliveryDate,
            SalesRepresentative = order.SalesRepresentative,
            OrderValue = order.OrderValue,
            ForecastValue = order.ForecastValue
        };
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, string userId)
    {
        if (dto.ForecastValue <= 0)
            throw new ArgumentException("Forecast Value must be greater than zero");

        var order = new OrderEntity
        {
            OrderID = dto.OrderID,
            Customer = dto.Customer,
            Region = dto.Region,
            Status = dto.Status,
            OrderDate = dto.OrderDate,
            DeliveryDate = dto.DeliveryDate,
            SalesRepresentative = dto.SalesRepresentative,
            OrderValue = dto.OrderValue,
            ForecastValue = dto.ForecastValue,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("CREATE", "Order", order.OrderID, userId, 
            $"Created order for {order.Customer} with value ${order.OrderValue:N2}");

        return new OrderDto
        {
            OrderID = order.OrderID,
            Customer = order.Customer,
            Region = order.Region,
            Status = order.Status,
            OrderDate = order.OrderDate,
            DeliveryDate = order.DeliveryDate,
            SalesRepresentative = order.SalesRepresentative,
            OrderValue = order.OrderValue,
            ForecastValue = order.ForecastValue
        };
    }

    public async Task<OrderDto> UpdateOrderAsync(string orderId, UpdateOrderDto dto, string userId)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found");

        if (dto.ForecastValue <= 0)
            throw new ArgumentException("Forecast Value must be greater than zero");

        var changes = new List<string>();

        if (order.Status != dto.Status)
            changes.Add($"Status: {order.Status} → {dto.Status}");
        if (order.ForecastValue != dto.ForecastValue)
            changes.Add($"Forecast Value: ${order.ForecastValue:N2} → ${dto.ForecastValue:N2}");

        order.Status = dto.Status;
        order.ForecastValue = dto.ForecastValue;
        order.UpdatedBy = userId;
        order.UpdatedAt = DateTime.UtcNow;

        _context.Orders.Update(order);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("UPDATE", "Order", orderId, userId, 
            string.Join("; ", changes));

        return new OrderDto
        {
            OrderID = order.OrderID,
            Customer = order.Customer,
            Region = order.Region,
            Status = order.Status,
            OrderDate = order.OrderDate,
            DeliveryDate = order.DeliveryDate,
            SalesRepresentative = order.SalesRepresentative,
            OrderValue = order.OrderValue,
            ForecastValue = order.ForecastValue
        };
    }

    public async Task DeleteOrderAsync(string orderId, string userId)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found");

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("DELETE", "Order", orderId, userId, 
            $"Deleted order for {order.Customer}");
    }
}
```

**Services/IAuditService.cs & AuditService.cs**
```csharp
namespace SalesOperationsAPI.Services;

public interface IAuditService
{
    Task LogAsync(string action, string entityType, string? entityId, string? userId, string? details);
}

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string action, string entityType, string? entityId, string? userId, string? details)
    {
        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        
        var log = new AuditLogEntity
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            UserId = userId,
            Details = details,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
```

### 1.6 Create API Controllers

**Controllers/OrdersController.cs**
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesOperationsAPI.DTOs;
using SalesOperationsAPI.Services;
using System.Security.Claims;

namespace SalesOperationsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    [Authorize(Policy = "SalesRepPolicy")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
    {
        try
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{orderId}")]
    [Authorize(Policy = "SalesRepPolicy")]
    public async Task<ActionResult<OrderDto>> GetOrder(string orderId)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return NotFound($"Order {orderId} not found");
            return Ok(order);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = "SalesManagerPolicy")]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto dto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("User ID not found");

            var order = await _orderService.CreateOrderAsync(dto, userId);
            return CreatedAtAction(nameof(GetOrder), new { orderId = order.OrderID }, order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("{orderId}")]
    [Authorize(Policy = "SalesManagerPolicy")]
    public async Task<IActionResult> UpdateOrder(string orderId, [FromBody] UpdateOrderDto dto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("User ID not found");

            await _orderService.UpdateOrderAsync(orderId, dto, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("{orderId}")]
    [Authorize(Policy = "SalesManagerPolicy")]
    public async Task<IActionResult> DeleteOrder(string orderId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("User ID not found");

            await _orderService.DeleteOrderAsync(orderId, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
```

**Controllers/AuthController.cs**
```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SalesOperationsAPI.Data;
using SalesOperationsAPI.DTOs;
using SalesOperationsAPI.Services;

namespace SalesOperationsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;

    public AuthController(UserManager<ApplicationUser> userManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized("Invalid credentials");

        var token = await _jwtService.GenerateTokenAsync(user);
        return Ok(new { token, userId = user.Id, email = user.Email });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            FullName = request.FullName,
            Department = request.Department ?? "Sales"
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _userManager.AddToRoleAsync(user, "SalesRep");
        
        return Ok(new { message = "User registered successfully" });
    }
}
```

### 1.7 Create DTOs

**DTOs/OrderDto.cs**
```csharp
namespace SalesOperationsAPI.DTOs;

public class OrderDto
{
    public string OrderID { get; set; } = null!;
    public string Customer { get; set; } = null!;
    public string Region { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public DateTime DeliveryDate { get; set; }
    public string SalesRepresentative { get; set; } = null!;
    public decimal OrderValue { get; set; }
    public decimal ForecastValue { get; set; }
}

public class CreateOrderDto
{
    public string OrderID { get; set; } = null!;
    public string Customer { get; set; } = null!;
    public string Region { get; set; } = null!;
    public string Status { get; set; } = "New";
    public DateTime OrderDate { get; set; }
    public DateTime DeliveryDate { get; set; }
    public string SalesRepresentative { get; set; } = null!;
    public decimal OrderValue { get; set; }
    public decimal ForecastValue { get; set; }
}

public class UpdateOrderDto
{
    public string Status { get; set; } = null!;
    public decimal ForecastValue { get; set; }
}
```

**DTOs/AuthDto.cs**
```csharp
namespace SalesOperationsAPI.DTOs;

public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class RegisterRequest
{
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? Department { get; set; }
}
```

## Step 2: Update Blazor App to Use Backend API

### 2.1 Create API Service

**Services/OrderApiService.cs**
```csharp
using AIAssistantSalesOperationsDashboard.Models;
using System.Net.Http.Json;

namespace AIAssistantSalesOperationsDashboard.Services;

public class OrderApiService : IOrderDataService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService? _localStorage;

    public event EventHandler? DataChanged;

    public OrderApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<OrderRecord>> GetOrdersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/orders");
            response.EnsureSuccessStatusCode();
            var orders = await response.Content.ReadFromJsonAsync<List<OrderRecord>>();
            return orders ?? new();
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"API Error: {ex.Message}");
            return new();
        }
    }

    public async Task<OrderRecord?> GetOrderByIdAsync(string orderId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/orders/{orderId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<OrderRecord>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<OrderRecord> CreateOrderAsync(OrderRecord order)
    {
        var response = await _httpClient.PostAsJsonAsync("api/orders", order);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<OrderRecord>();
        RaiseDataChanged();
        return created ?? order;
    }

    public async Task<OrderRecord> UpdateOrderAsync(OrderRecord order)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/orders/{order.OrderID}", 
            new { status = order.Status, forecastValue = order.ForecastValue });
        response.EnsureSuccessStatusCode();
        RaiseDataChanged();
        return order;
    }

    public async Task DeleteOrderAsync(string orderId)
    {
        var response = await _httpClient.DeleteAsync($"api/orders/{orderId}");
        response.EnsureSuccessStatusCode();
        RaiseDataChanged();
    }

    public Task LogActivityAsync(ActivityLog log)
    {
        // In production, could send to server
        return Task.CompletedTask;
    }

    public async Task<List<ActivityLog>> GetActivityLogsAsync(int count = 20)
    {
        // Could fetch from API if tracked server-side
        return new();
    }

    public async Task<Dictionary<string, object>> GetOrderSummaryAsync()
    {
        var orders = await GetOrdersAsync();
        return new Dictionary<string, object>
        {
            { "TotalOrders", orders.Count },
            { "DelayedOrders", orders.Count(o => o.Status == "Delayed") },
            { "TotalOrderValue", orders.Sum(o => o.OrderValue) },
            { "AverageOrderValue", orders.Count > 0 ? orders.Average(o => o.OrderValue) : 0 },
            { "CompletedOrders", orders.Count(o => o.Status == "Completed") },
            { "InProgressOrders", orders.Count(o => o.Status == "In Progress") },
            { "NewOrders", orders.Count(o => o.Status == "New") }
        };
    }

    public Task ResetDataAsync() => Task.CompletedTask;

    private void RaiseDataChanged()
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }
}
```

### 2.2 Update Program.cs

```csharp
// Register HTTP Client for API
builder.Services.AddScoped(sp =>
{
    var apiUrl = builder.Configuration["ApiUrl"] ?? "https://localhost:7001";
    var client = new HttpClient { BaseAddress = new Uri(apiUrl) };
    
    // Add auth token if available
    var token = sp.GetRequiredService<ITokenService>().GetToken();
    if (!string.IsNullOrEmpty(token))
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    
    return client;
});

// Register API service instead of in-memory
builder.Services.AddScoped<IOrderDataService, OrderApiService>();
```

### 2.3 Create Authentication Service

**Services/AuthService.cs**
```csharp
using System.Net.Http.Json;

namespace AIAssistantSalesOperationsDashboard.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    string? GetToken();
}

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private string? _token;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", 
            new { email, password });
        
        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content.ReadAsAsync<LoginResponse>();
        _token = result?.Token;
        
        // Store in local storage for persistence
        // await localStorage.SetItemAsync("authToken", _token);
        
        return true;
    }

    public Task LogoutAsync()
    {
        _token = null;
        // await localStorage.RemoveItemAsync("authToken");
        return Task.CompletedTask;
    }

    public Task<bool> IsAuthenticatedAsync()
    {
        return Task.FromResult(!string.IsNullOrEmpty(_token));
    }

    public string? GetToken() => _token;
}

public class LoginResponse
{
    public string Token { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string Email { get; set; } = null!;
}
```

## Step 3: Database Migration & Setup

```bash
# In API project directory
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Step 4: Security Best Practices

### 4.1 HTTPS Only
```csharp
app.UseHsts(); // HTTP Strict Transport Security
app.UseHttpsRedirection();
```

### 4.2 CORS Configuration
```json
{
  "AllowedOrigins": "https://yourdomain.com;https://www.yourdomain.com"
}
```

### 4.3 API Rate Limiting
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
    });
});

app.UseRateLimiter();
```

### 4.4 SQL Injection Prevention
- Always use parameterized queries (Entity Framework)
- Never concatenate user input into SQL strings

### 4.5 XSS Prevention
- Blazor has built-in XSS protection
- Use `@Html.Raw()` only for trusted content

### 4.6 CSRF Protection
- Implement CSRF tokens for form submissions
- Use SameSite cookies

## Step 5: Deployment

### Azure App Service
```bash
dotnet publish -c Release
# Upload to Azure Portal or use Azure CLI
az webapp deployment source config-zip --resource-group mygroup --name myapp --src publish.zip
```

### Docker
See SETUP.md for Docker configuration

### On-Premises IIS
```bash
dotnet publish -c Release -o ./iis
# Copy folder to IIS wwwroot
# Configure application pool (.NET CLR version: No Managed Code)
```

## Step 6: Monitoring & Logging

### Implement Structured Logging
```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();
```

### Application Insights
```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

## Production Checklist

- [ ] Database backups configured
- [ ] SSL/TLS certificate installed
- [ ] API authentication enabled
- [ ] Authorization policies tested
- [ ] Audit logging verified
- [ ] Input validation implemented
- [ ] Rate limiting configured
- [ ] CORS properly scoped
- [ ] HTTPS redirect enabled
- [ ] Logging & monitoring active
- [ ] Error messages sanitized (no SQL details)
- [ ] Sensitive data encrypted
- [ ] Database connection pooling configured
- [ ] Performance tested under load
- [ ] Disaster recovery plan documented
- [ ] Security headers configured (CSP, X-Frame-Options, etc.)

---

**Last Updated**: January 2025
