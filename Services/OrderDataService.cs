using AIAssistantSalesOperationsDashboard.Models;

namespace AIAssistantSalesOperationsDashboard.Services;

/// <summary>
/// In-memory implementation of IOrderDataService.
/// 
/// This service manages all order records and activity logs in memory.
/// Data is NOT persisted to a database — refreshing the page restores original records.
/// 
/// In production, replace with an API-based implementation that calls an ASP.NET Core backend:
/// - Use HttpClient to call REST endpoints (GET/POST/PUT/DELETE)
/// - Implement proper error handling and retry logic
/// - Add authentication headers (JWT, etc.)
/// - Handle server-side validation responses
/// 
/// See README.md "Production Deployment" section for implementation details.
/// </summary>
public class OrderDataService : IOrderDataService
{
    // In-memory storage
    private List<OrderRecord> _orders = new();
    private List<ActivityLog> _activityLogs = new();

    // Store original data for reset functionality
    private List<OrderRecord> _originalOrders = new();

    // Event for notifying subscribers of data changes
    public event EventHandler? DataChanged;

    public OrderDataService()
    {
        InitializeData();
    }

    /// <summary>
    /// Initialize with 12 realistic sample orders
    /// </summary>
    private void InitializeData()
    {
        _orders = new List<OrderRecord>
        {
            new OrderRecord
            {
                OrderID = "O001",
                Customer = "Acme Corporation",
                Region = "North America",
                Status = "Completed",
                OrderDate = new DateTime(2024, 1, 15),
                DeliveryDate = new DateTime(2024, 2, 10),
                SalesRepresentative = "John Smith",
                OrderValue = 45000m,
                ForecastValue = 47500m
            },
            new OrderRecord
            {
                OrderID = "O002",
                Customer = "Global Tech Solutions",
                Region = "Europe",
                Status = "In Progress",
                OrderDate = new DateTime(2024, 2, 01),
                DeliveryDate = new DateTime(2024, 3, 15),
                SalesRepresentative = "Sarah Johnson",
                OrderValue = 62500m,
                ForecastValue = 60000m
            },
            new OrderRecord
            {
                OrderID = "O003",
                Customer = "Pacific Innovations",
                Region = "Asia-Pacific",
                Status = "New",
                OrderDate = new DateTime(2024, 2, 20),
                DeliveryDate = new DateTime(2024, 4, 20),
                SalesRepresentative = "Michael Chen",
                OrderValue = 35800m,
                ForecastValue = 36000m
            },
            new OrderRecord
            {
                OrderID = "O004",
                Customer = "Nordic Manufacturing",
                Region = "Europe",
                Status = "Delayed",
                OrderDate = new DateTime(2023, 12, 10),
                DeliveryDate = new DateTime(2024, 1, 30),
                SalesRepresentative = "Emma Oslo",
                OrderValue = 89200m,
                ForecastValue = 85000m
            },
            new OrderRecord
            {
                OrderID = "O005",
                Customer = "American Logistics",
                Region = "North America",
                Status = "In Progress",
                OrderDate = new DateTime(2024, 1, 25),
                DeliveryDate = new DateTime(2024, 3, 20),
                SalesRepresentative = "Robert Davis",
                OrderValue = 51300m,
                ForecastValue = 50000m
            },
            new OrderRecord
            {
                OrderID = "O006",
                Customer = "Eastern Enterprises",
                Region = "Asia-Pacific",
                Status = "Completed",
                OrderDate = new DateTime(2023, 12, 05),
                DeliveryDate = new DateTime(2024, 1, 20),
                SalesRepresentative = "Lisa Wong",
                OrderValue = 73400m,
                ForecastValue = 75000m
            },
            new OrderRecord
            {
                OrderID = "O007",
                Customer = "Central Trading",
                Region = "Europe",
                Status = "Completed",
                OrderDate = new DateTime(2023, 11, 20),
                DeliveryDate = new DateTime(2024, 1, 10),
                SalesRepresentative = "Pierre Dubois",
                OrderValue = 42900m,
                ForecastValue = 41500m
            },
            new OrderRecord
            {
                OrderID = "O008",
                Customer = "Mountain Peak Industries",
                Region = "North America",
                Status = "New",
                OrderDate = new DateTime(2024, 2, 15),
                DeliveryDate = new DateTime(2024, 4, 10),
                SalesRepresentative = "James Wilson",
                OrderValue = 67200m,
                ForecastValue = 65000m
            },
            new OrderRecord
            {
                OrderID = "O009",
                Customer = "Sunrise Imports",
                Region = "Asia-Pacific",
                Status = "In Progress",
                OrderDate = new DateTime(2024, 1, 30),
                DeliveryDate = new DateTime(2024, 3, 25),
                SalesRepresentative = "Kenji Tanaka",
                OrderValue = 58600m,
                ForecastValue = 60000m
            },
            new OrderRecord
            {
                OrderID = "O010",
                Customer = "Westend Holdings",
                Region = "Europe",
                Status = "New",
                OrderDate = new DateTime(2024, 2, 18),
                DeliveryDate = new DateTime(2024, 4, 15),
                SalesRepresentative = "Klaus Mueller",
                OrderValue = 44500m,
                ForecastValue = 45000m
            },
            new OrderRecord
            {
                OrderID = "O011",
                Customer = "Desert Resources",
                Region = "Africa",
                Status = "Delayed",
                OrderDate = new DateTime(2023, 11, 15),
                DeliveryDate = new DateTime(2024, 1, 15),
                SalesRepresentative = "Ahmed Hassan",
                OrderValue = 95700m,
                ForecastValue = 92000m
            },
            new OrderRecord
            {
                OrderID = "O012",
                Customer = "Southern Alliance",
                Region = "South America",
                Status = "Completed",
                OrderDate = new DateTime(2024, 1, 05),
                DeliveryDate = new DateTime(2024, 2, 20),
                SalesRepresentative = "Carlos Rodriguez",
                OrderValue = 52400m,
                ForecastValue = 52500m
            }
        };

        // Store original data for reset
        _originalOrders = _orders.Select(o => new OrderRecord
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
        }).ToList();

        // Log initialization
        _activityLogs.Add(ActivityLog.Create(
            "System",
            $"Dashboard initialized with {_orders.Count} order records"));
    }

    public Task<List<OrderRecord>> GetOrdersAsync()
    {
        return Task.FromResult(_orders.OrderBy(o => o.OrderID).ToList());
    }

    public Task<OrderRecord?> GetOrderByIdAsync(string orderId)
    {
        var order = _orders.FirstOrDefault(o => o.OrderID == orderId);
        return Task.FromResult(order);
    }

    public Task<OrderRecord> CreateOrderAsync(OrderRecord order)
    {
        if (string.IsNullOrEmpty(order.OrderID))
            throw new ArgumentException("OrderID cannot be empty");

        if (_orders.Any(o => o.OrderID == order.OrderID))
            throw new ArgumentException($"Order with ID {order.OrderID} already exists");

        _orders.Add(order);

        LogActivityAsync(ActivityLog.Create(
            "Create",
            $"Order {order.OrderID} created for {order.Customer}",
            order.OrderID)).Wait();

        RaiseDataChanged();
        return Task.FromResult(order);
    }

    public Task<OrderRecord> UpdateOrderAsync(OrderRecord order)
    {
        if (string.IsNullOrEmpty(order.OrderID))
            throw new ArgumentException("OrderID cannot be empty");

        var existingOrder = _orders.FirstOrDefault(o => o.OrderID == order.OrderID);
        if (existingOrder == null)
            throw new KeyNotFoundException($"Order with ID {order.OrderID} not found");

        // Validate ForecastValue
        if (order.ForecastValue <= 0)
            throw new ArgumentException("Forecast Value must be greater than zero");

        // Track what changed
        var changes = new List<string>();

        if (existingOrder.Customer != order.Customer)
            changes.Add($"Customer: {existingOrder.Customer} → {order.Customer}");
        if (existingOrder.Status != order.Status)
            changes.Add($"Status: {existingOrder.Status} → {order.Status}");
        if (existingOrder.ForecastValue != order.ForecastValue)
            changes.Add($"Forecast Value: ${existingOrder.ForecastValue:N2} → ${order.ForecastValue:N2}");
        if (existingOrder.OrderValue != order.OrderValue)
            changes.Add($"Order Value: ${existingOrder.OrderValue:N2} → ${order.OrderValue:N2}");

        // Update the order
        existingOrder.Customer = order.Customer;
        existingOrder.Region = order.Region;
        existingOrder.Status = order.Status;
        existingOrder.OrderDate = order.OrderDate;
        existingOrder.DeliveryDate = order.DeliveryDate;
        existingOrder.SalesRepresentative = order.SalesRepresentative;
        existingOrder.OrderValue = order.OrderValue;
        existingOrder.ForecastValue = order.ForecastValue;

        LogActivityAsync(ActivityLog.Create(
            "Update",
            string.Join("; ", changes.Count > 0 ? changes : new List<string> { "No changes" }),
            order.OrderID)).Wait();

        RaiseDataChanged();
        return Task.FromResult(existingOrder);
    }

    public Task DeleteOrderAsync(string orderId)
    {
        var order = _orders.FirstOrDefault(o => o.OrderID == orderId);
        if (order == null)
            throw new KeyNotFoundException($"Order with ID {orderId} not found");

        _orders.Remove(order);

        LogActivityAsync(ActivityLog.Create(
            "Delete",
            $"Order {orderId} deleted from {order.Customer}",
            orderId)).Wait();

        RaiseDataChanged();
        return Task.CompletedTask;
    }

    public Task LogActivityAsync(ActivityLog log)
    {
        _activityLogs.Add(log);
        return Task.CompletedTask;
    }

    public Task<List<ActivityLog>> GetActivityLogsAsync(int count = 20)
    {
        var logs = _activityLogs.OrderByDescending(l => l.Timestamp).Take(count).ToList();
        return Task.FromResult(logs);
    }

    public Task<Dictionary<string, object>> GetOrderSummaryAsync()
    {
        var totalOrders = _orders.Count;
        var delayedOrders = _orders.Count(o => o.Status == "Delayed");
        var totalOrderValue = _orders.Sum(o => o.OrderValue);
        var averageOrderValue = totalOrders > 0 ? totalOrderValue / totalOrders : 0;

        return Task.FromResult(new Dictionary<string, object>
        {
            { "TotalOrders", totalOrders },
            { "DelayedOrders", delayedOrders },
            { "TotalOrderValue", totalOrderValue },
            { "AverageOrderValue", averageOrderValue },
            { "CompletedOrders", _orders.Count(o => o.Status == "Completed") },
            { "InProgressOrders", _orders.Count(o => o.Status == "In Progress") },
            { "NewOrders", _orders.Count(o => o.Status == "New") }
        });
    }

    public Task ResetDataAsync()
    {
        _orders = _originalOrders.Select(o => new OrderRecord
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
        }).ToList();

        _activityLogs.Clear();
        _activityLogs.Add(ActivityLog.Create("System", "Dashboard data reset to original state"));

        RaiseDataChanged();
        return Task.CompletedTask;
    }

    private void RaiseDataChanged()
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }
}
