namespace AIAssistantSalesOperationsDashboard.Models;

/// <summary>
/// Represents a sales order record with customer, delivery, and financial details.
/// This model is used across Grid, Chart, and Spreadsheet components.
/// </summary>
public class OrderRecord
{
    /// <summary>
    /// Unique identifier for the order (e.g., "O001")
    /// </summary>
    public string OrderID { get; set; } = string.Empty;

    /// <summary>
    /// Customer name
    /// </summary>
    public string Customer { get; set; } = string.Empty;

    /// <summary>
    /// Geographic region (e.g., "North America", "Europe", "Asia-Pacific")
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Order status: "New", "In Progress", "Delayed", "Completed"
    /// </summary>
    public string Status { get; set; } = "New";

    /// <summary>
    /// Date the order was placed
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// Expected or actual delivery date
    /// </summary>
    public DateTime DeliveryDate { get; set; }

    /// <summary>
    /// Name of the sales representative managing the order
    /// </summary>
    public string SalesRepresentative { get; set; } = string.Empty;

    /// <summary>
    /// Actual order value in USD
    /// </summary>
    public decimal OrderValue { get; set; }

    /// <summary>
    /// Forecasted order value (can be edited by users)
    /// </summary>
    public decimal ForecastValue { get; set; }

    /// <summary>
    /// Determines if the order is overdue
    /// </summary>
    public bool IsDelayed => Status == "Delayed" && DeliveryDate < DateTime.Now;

    /// <summary>
    /// Days until delivery (positive = future, negative = overdue)
    /// </summary>
    public int DaysUntilDelivery => (DeliveryDate - DateTime.Now).Days;

    /// <summary>
    /// Variance between forecast and actual order value
    /// </summary>
    public decimal ForecastVariance => Math.Abs(ForecastValue - OrderValue);

    /// <summary>
    /// Variance percentage
    /// </summary>
    public decimal ForecastVariancePercent => OrderValue != 0 ? (ForecastVariance / OrderValue) * 100 : 0;
}
