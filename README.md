# AI-Assisted Sales Operations Dashboard

A complete .NET 8 Blazor Web App with native Syncfusion Blazor components for sales order management, visualization, and reporting.

## Features

- **SfGrid**: Interactive order management with filtering, sorting, selection, and in-line editing
- **SfChart**: Column chart visualization of order values by customer
- **SfSpreadsheet**: Advanced reporting with formulas, conditional formatting, and Excel export
- **Responsive Layout**: Optimized for 1366 × 768 webinar screens and responsive to all resolutions
- **Summary Cards**: Real-time KPI tracking (Total Orders, Delayed Orders, Total Order Value, Average Order Value)
- **Activity Panel**: Logs all application actions and changes
- **Data Validation**: Forecast Value validation with confirmation dialogs
- **Shared Data Service**: All three components synchronized through a single C# service
- **In-Memory Data**: Easy to test and extend; production guidance included

## Project Structure

```
AIAssistantSalesOperationsDashboard/
├── AIAssistantSalesOperationsDashboard.csproj
├── Program.cs
├── app.razor
├── App.razor
├── Layout/
│   └── MainLayout.razor
├── Components/
│   ├── Dashboard.razor (Main component)
│   ├── OrderGrid.razor
│   ├── OrderChart.razor
│   ├── OrderSpreadsheet.razor
│   ├── SummaryCards.razor
│   └── ActivityPanel.razor
├── Models/
│   ├── OrderRecord.cs
│   └── ActivityLog.cs
├── Services/
│   ├── OrderDataService.cs
│   └── IOrderDataService.cs
├── wwwroot/
│   ├── css/
│   │   └── app.css
│   └── index.html
└── Properties/
    └── launchSettings.json
```

## Prerequisites

- **.NET 8 SDK** ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- **Visual Studio 2022** or **Visual Studio Code** with C# extension
- **Syncfusion Blazor License** or Community License (free for companies with gross annual revenues < $1 million USD)

## Installation & Setup

### 1. Create the Project

```bash
dotnet new blazor -n AIAssistantSalesOperationsDashboard -f net8.0
cd AIAssistantSalesOperationsDashboard
```

### 2. Install Syncfusion Blazor NuGet Package

```bash
dotnet add package Syncfusion.Blazor.Grids --version 25.1.34
dotnet add package Syncfusion.Blazor.Charts --version 25.1.34
dotnet add package Syncfusion.Blazor.Spreadsheet --version 25.1.34
dotnet add package Syncfusion.Blazor.Calendars --version 25.1.34
dotnet add package Syncfusion.Blazor.DropDowns --version 25.1.34
dotnet add package Syncfusion.Blazor.Buttons --version 25.1.34
dotnet add package Syncfusion.Blazor.Inputs --version 25.1.34
dotnet add package Syncfusion.Blazor.Popups --version 25.1.34
dotnet add package Syncfusion.Blazor.Notifications --version 25.1.34
```

### 3. Replace Project Files

Copy all files from this repository into your project directory, replacing the default template files.

### 4. Configure Program.cs

The project includes a complete `Program.cs` with Syncfusion service registration, CORS, and HttpClient configuration.

### 5. Register Syncfusion License (Optional)

If you have a Syncfusion license key:

```csharp
// In Program.cs, add before builder.Build():
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY_HERE");
```

### 6. Run the Application

```bash
dotnet run
```

The application will start on `https://localhost:7201` (adjust port as needed).

## Usage

### Dashboard Layout

The dashboard displays:

1. **Summary Cards** (Top) - Real-time KPI metrics
2. **Order Grid** (Middle-Left) - Editable order records with filtering and sorting
3. **Order Chart** (Middle-Right) - Column chart of order values by customer
4. **Order Spreadsheet** (Bottom) - Advanced reporting with formulas and export
5. **Activity Panel** (Right Sidebar) - Real-time action log

### Editing Orders

1. Click **Edit** on any grid row or double-click a cell to enter edit mode
2. Modify the order details as needed
3. When editing **Forecast Value**:
   - A confirmation dialog appears
   - Value must be positive (> 0)
   - The chart and spreadsheet auto-update on save
4. Click **Cancel** to discard changes

### Chart Interaction

- Hover over chart columns to see values
- Chart updates automatically when grid data changes
- Drill-down capability available (implementation depends on Syncfusion version)

### Spreadsheet Report

- Displays summarized order data with calculations
- **Formulas**:
  - Total Order Value: `SUM()` of all orders
  - Total Forecast Value: `SUM()` of all forecasts
  - Average Order Value: `AVERAGE()` of all orders
- **Conditional Formatting**: Highlights high-value orders (> $50,000)
- **Excel Export**: Download the report as .xlsx file
- Updates automatically when grid data changes

### Activity Panel

- Logs all actions: record creation, updates, deletions
- Shows timestamp and action details
- Useful for audit trails and debugging

### Data Notes

- **Data is stored in-memory only** — refreshing the browser restores original records
- **No database persistence** — this is a demonstration dashboard
- See "Production Deployment" below for database integration

## Data Model

### OrderRecord

```csharp
public class OrderRecord
{
    public string OrderID { get; set; }
    public string Customer { get; set; }
    public string Region { get; set; }
    public string Status { get; set; } // New, In Progress, Delayed, Completed
    public DateTime OrderDate { get; set; }
    public DateTime DeliveryDate { get; set; }
    public string SalesRepresentative { get; set; }
    public decimal OrderValue { get; set; }
    public decimal ForecastValue { get; set; }
}
```

### ActivityLog

```csharp
public class ActivityLog
{
    public DateTime Timestamp { get; set; }
    public string Action { get; set; }
    public string Details { get; set; }
}
```

## Syncfusion Components Used

### 1. SfGrid (`OrderGrid.razor`)

- **Features**:
  - Filtering, sorting, column selection
  - Inline editing with confirmation
  - Currency and date formatting
  - Custom edit templates
  - Row selection
  
- **Key Properties**:
  - `AllowFiltering`, `AllowSorting`, `AllowSelection`
  - `EditSettings`, `GridLines`, `AllowExcelExport`
  - `ActionFailure`, `OnActionComplete` events

### 2. SfChart (`OrderChart.razor`)

- **Features**:
  - Column chart showing order values by customer
  - Tooltip and legend
  - Responsive sizing
  - Auto-update on data changes

- **Key Properties**:
  - `ChartArea`, `Title`, `Tooltip`
  - `SeriesCollection`, `AxisCollection`
  - `Series` with `DataSource` binding

### 3. SfSpreadsheet (`OrderSpreadsheet.razor`)

- **Features**:
  - Range-based data input
  - Formula support (SUM, AVERAGE)
  - Conditional formatting
  - Excel export with XlsxExport class
  - Currency number formatting

- **Key Properties**:
  - `AllowOpen`, `AllowSave`, `ShowSheetTabs`
  - `CellFormatting`, `ConditionalFormatting`
  - Built-in export: `XlsxExport` class

## Component Architecture

### Shared Data Service Pattern

All three components use a single `OrderDataService` (implemented in `Services/OrderDataService.cs`):

```csharp
public interface IOrderDataService
{
    Task<List<OrderRecord>> GetOrdersAsync();
    Task<OrderRecord> CreateOrderAsync(OrderRecord order);
    Task<OrderRecord> UpdateOrderAsync(OrderRecord order);
    Task DeleteOrderAsync(string orderId);
    event EventHandler DataChanged;
}
```

**Data Flow**:
1. Dashboard loads orders via `OrderDataService.GetOrdersAsync()`
2. Grid, Chart, and Spreadsheet bind to the same data
3. When user edits in Grid, `UpdateOrderAsync()` is called
4. Service raises `DataChanged` event
5. Chart and Spreadsheet react to event and refresh

### Component Communication

Components communicate through:
1. **Service Events**: `DataChanged` event triggers refreshes
2. **Cascading Parameters**: Parent-to-child data passing
3. **Direct Service Calls**: Each component independently queries the service

## Production Deployment

### Architecture for Production

```
┌─────────────────────────────────────────────────┐
│   Blazor WebAssembly / Server App (This Code)   │
│  - UI Components (Grid, Chart, Spreadsheet)     │
│  - Client-side validation & logic               │
└──────────────┬──────────────────────────────────┘
               │ HTTP/REST
┌──────────────▼──────────────────────────────────┐
│      ASP.NET Core Backend API                   │
│  - Authentication & Authorization               │
│  - Business Logic                               │
│  - Audit Logging                                │
└──────────────┬──────────────────────────────────┘
               │ SQL/ORM
┌──────────────▼──────────────────────────────────┐
│    SQL Server / PostgreSQL Database             │
│  - Orders Table                                 │
│  - Users & Roles Table                          │
│  - Audit Log Table                              │
└─────────────────────────────────────────────────┘
```

### Implementation Steps

#### 1. Create Backend API Project

```bash
dotnet new webapi -n SalesOperationsAPI -f net8.0
cd SalesOperationsAPI
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package AutoMapper
```

#### 2. Add DbContext

```csharp
public class SalesDbContext : DbContext
{
    public DbSet<OrderRecord> Orders { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlServer("Server=YOUR_SERVER;Database=SalesDB;...");
    }
}
```

#### 3. Implement API Endpoints

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderRecord>>> GetOrders()
    {
        return await _context.Orders.ToListAsync();
    }
    
    [HttpPost]
    public async Task<ActionResult<OrderRecord>> CreateOrder(OrderRecord order)
    {
        _context.Orders.Add(order);
        await LogAuditAsync("CREATE", "Order", order.OrderID);
        await _context.SaveChangesAsync();
        return CreatedAtAction("GetOrder", new { id = order.OrderID }, order);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(string id, OrderRecord order)
    {
        if (id != order.OrderID) return BadRequest();
        
        _context.Update(order);
        await LogAuditAsync("UPDATE", "Order", id);
        await _context.SaveChangesAsync();
        return NoContent();
    }
    
    private async Task LogAuditAsync(string action, string entityType, string entityId)
    {
        var auditLog = new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            Timestamp = DateTime.UtcNow,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        };
        _context.AuditLogs.Add(auditLog);
    }
}
```

#### 4. Implement Authentication & Authorization

```csharp
public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
{
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = "https://your-auth-server";
            options.Audience = "your-api-audience";
        });
    
    services.AddAuthorization(options =>
    {
        options.AddPolicy("SalesManagerPolicy", policy =>
            policy.RequireRole("SalesManager", "Admin"));
        options.AddPolicy("SalesRepPolicy", policy =>
            policy.RequireRole("SalesRep", "SalesManager", "Admin"));
    });
    
    return services;
}
```

#### 5. Update Blazor Service to Call API

```csharp
public class OrderDataService : IOrderDataService
{
    private readonly HttpClient _httpClient;
    
    public async Task<List<OrderRecord>> GetOrdersAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<OrderRecord>>("api/orders");
    }
    
    public async Task<OrderRecord> UpdateOrderAsync(OrderRecord order)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/orders/{order.OrderID}", order);
        response.EnsureSuccessStatusCode();
        RaiseDataChanged();
        return order;
    }
}
```

#### 6. Database Schema

```sql
CREATE TABLE Orders (
    OrderID NVARCHAR(50) PRIMARY KEY,
    Customer NVARCHAR(100) NOT NULL,
    Region NVARCHAR(100),
    Status NVARCHAR(50),
    OrderDate DATETIME NOT NULL,
    DeliveryDate DATETIME,
    SalesRepresentative NVARCHAR(100),
    OrderValue DECIMAL(18,2),
    ForecastValue DECIMAL(18,2),
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME DEFAULT GETUTCDATE()
);

CREATE TABLE AuditLogs (
    AuditLogID INT PRIMARY KEY IDENTITY,
    Action NVARCHAR(50),
    EntityType NVARCHAR(100),
    EntityId NVARCHAR(50),
    UserId NVARCHAR(128),
    Timestamp DATETIME,
    IpAddress NVARCHAR(45)
);

CREATE INDEX idx_orders_status ON Orders(Status);
CREATE INDEX idx_orders_customer ON Orders(Customer);
CREATE INDEX idx_auditlogs_timestamp ON AuditLogs(Timestamp);
```

### Security Considerations

1. **Authentication**: Use Azure AD, Auth0, or IdentityServer4
2. **Authorization**: Implement role-based access control (RBAC)
3. **Data Validation**: Validate all input on server-side
4. **Encryption**: Use HTTPS, encrypt sensitive data at rest
5. **Audit Logging**: Log all data modifications with user & timestamp
6. **CORS**: Configure appropriate CORS policies
7. **Rate Limiting**: Implement API rate limiting
8. **Input Sanitization**: Prevent SQL injection and XSS attacks

### Performance Optimization

1. **Pagination**: Implement server-side pagination for large datasets
2. **Caching**: Use Redis for frequently accessed data
3. **Lazy Loading**: Load chart and spreadsheet data only when visible
4. **Database Indexing**: Create indexes on frequently queried columns
5. **Query Optimization**: Use `AsNoTracking()` for read-only queries
6. **Compression**: Enable gzip compression for API responses

## Testing

### Unit Testing OrderDataService

```bash
dotnet add package xunit
dotnet add package Moq
```

```csharp
public class OrderDataServiceTests
{
    [Fact]
    public async Task GetOrdersAsync_ReturnsAllOrders()
    {
        // Arrange
        var service = new OrderDataService();
        
        // Act
        var orders = await service.GetOrdersAsync();
        
        // Assert
        Assert.NotEmpty(orders);
        Assert.All(orders, o => Assert.NotNull(o.OrderID));
    }
    
    [Fact]
    public async Task UpdateOrderAsync_RaisesDataChangedEvent()
    {
        // Arrange
        var service = new OrderDataService();
        var eventRaised = false;
        service.DataChanged += (s, e) => eventRaised = true;
        var order = new OrderRecord { OrderID = "O001" };
        
        // Act
        await service.UpdateOrderAsync(order);
        
        // Assert
        Assert.True(eventRaised);
    }
}
```

## Troubleshooting

### Syncfusion Components Not Rendering

1. Verify Syncfusion NuGet packages are installed
2. Check `Program.cs` includes `services.AddSyncfusionBlazor()`
3. Verify `_Host.cshtml` or `app.razor` includes Syncfusion script tag
4. Clear browser cache and rebuild project

### Data Not Updating in Chart/Spreadsheet

1. Verify `DataChanged` event is being raised in `OrderDataService`
2. Check that components have `@ref` or direct method calls to refresh
3. Ensure `StateHasChanged()` is called after data updates
4. Review browser console for JavaScript errors

### Performance Issues with Large Datasets

1. Implement virtual scrolling in SfGrid
2. Use server-side pagination
3. Lazy-load chart and spreadsheet components
4. Consider splitting data into multiple smaller grids

## Support & Resources

- **Syncfusion Documentation**: https://www.syncfusion.com/blazor-components/blazor-grid
- **.NET 8 Documentation**: https://learn.microsoft.com/en-us/dotnet/
- **Blazor Documentation**: https://learn.microsoft.com/en-us/aspnet/core/blazor
- **Syncfusion Forums**: https://www.syncfusion.com/forums/

## License

- **Application Code**: Licensed as specified by your organization
- **Syncfusion Components**: Requires valid Syncfusion license (free for qualifying companies)

## Contributing

When contributing enhancements:

1. Follow C# coding conventions (PascalCase for public members)
2. Add XML documentation comments to public methods
3. Implement unit tests for business logic
4. Update this README with new features
5. Ensure responsive design compatibility

## Changelog

### v1.0.0 (Initial Release)
- Syncfusion Grid with editing and filtering
- Column chart visualization
- Spreadsheet reporting with export
- Summary KPI cards
- Activity logging
- In-memory data service
- Responsive dashboard layout

---

**Last Updated**: January 2025
**Syncfusion Version**: 25.1.34
**.NET Version**: 8.0
