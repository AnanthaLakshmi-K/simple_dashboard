using AIAssistantSalesOperationsDashboard.Models;

namespace AIAssistantSalesOperationsDashboard.Services;

/// <summary>
/// Interface for order data service. Defines contract for accessing and managing order data.
/// Implemented by OrderDataService for in-memory storage.
/// In production, can be replaced with API-based implementation.
/// </summary>
public interface IOrderDataService
{
    /// <summary>
    /// Retrieves all order records
    /// </summary>
    /// <returns>List of all order records</returns>
    Task<List<OrderRecord>> GetOrdersAsync();

    /// <summary>
    /// Retrieves a single order by ID
    /// </summary>
    /// <param name="orderId">The order ID to retrieve</param>
    /// <returns>The order record, or null if not found</returns>
    Task<OrderRecord?> GetOrderByIdAsync(string orderId);

    /// <summary>
    /// Creates a new order record
    /// </summary>
    /// <param name="order">The order to create</param>
    /// <returns>The created order record</returns>
    Task<OrderRecord> CreateOrderAsync(OrderRecord order);

    /// <summary>
    /// Updates an existing order record with validation
    /// </summary>
    /// <param name="order">The order to update</param>
    /// <returns>The updated order record</returns>
    /// <exception cref="ArgumentException">If order is invalid or not found</exception>
    Task<OrderRecord> UpdateOrderAsync(OrderRecord order);

    /// <summary>
    /// Deletes an order record
    /// </summary>
    /// <param name="orderId">The order ID to delete</param>
    /// <exception cref="KeyNotFoundException">If order not found</exception>
    Task DeleteOrderAsync(string orderId);

    /// <summary>
    /// Adds an activity log entry
    /// </summary>
    /// <param name="log">The activity log to record</param>
    Task LogActivityAsync(ActivityLog log);

    /// <summary>
    /// Retrieves recent activity logs
    /// </summary>
    /// <param name="count">Number of recent entries to retrieve</param>
    /// <returns>List of activity logs, newest first</returns>
    Task<List<ActivityLog>> GetActivityLogsAsync(int count = 20);

    /// <summary>
    /// Gets summary statistics for all orders
    /// </summary>
    /// <returns>Dictionary with stats: TotalOrders, DelayedOrders, TotalOrderValue, AverageOrderValue</returns>
    Task<Dictionary<string, object>> GetOrderSummaryAsync();

    /// <summary>
    /// Event raised when data changes (orders or activity logs)
    /// Allows components to react to changes and refresh their displays
    /// </summary>
    event EventHandler? DataChanged;

    /// <summary>
    /// Resets all data to original state (for development/testing)
    /// </summary>
    Task ResetDataAsync();
}
