# Quick Start Guide (5 Minutes)

## TL;DR - Get Running Now

### 1. Prerequisites
- Install .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0

### 2. Create Project
```bash
dotnet new blazor -n AIAssistantSalesOperationsDashboard -f net8.0
cd AIAssistantSalesOperationsDashboard
```

### 3. Install Syncfusion
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

### 4. Copy Source Files
- Copy all `.cs` files to your project
- Copy all `.razor` files to `Components/` folder
- Copy `.css` files to `wwwroot/css/`
- Copy `index.html` to `wwwroot/`

### 5. Run
```bash
dotnet run
```

Dashboard opens at `https://localhost:7201` ✅

## What You Get

| Component | Purpose |
|-----------|---------|
| **Summary Cards** | 6 KPI cards showing orders, revenue, delays |
| **Grid** | Editable table with filtering, sorting, currency formatting |
| **Chart** | Column chart: Order Value by Customer |
| **Spreadsheet** | Report with formulas, totals, Excel export |
| **Activity Panel** | Real-time action log |

## Features in 30 Seconds

### Editing an Order
1. Click the grid row or double-click a cell
2. Edit the fields (dates, values, status)
3. For Forecast Value: confirmation dialog appears
4. Click Save → Chart & Spreadsheet auto-update

### Viewing Data
- **Grid**: Sort by column headers, filter with icons
- **Chart**: Hover over bars to see values
- **Spreadsheet**: See calculated totals and averages
- **Activity Panel**: Watch changes in real-time

### Resetting Data
- Click **Reset Data** button to restore original 12 orders

## Sample Data (12 Orders)

| ID | Customer | Region | Status | Order Value | Forecast |
|----|----------|--------|--------|-------------|----------|
| O001 | Acme Corporation | North America | Completed | $45,000 | $47,500 |
| O002 | Global Tech Solutions | Europe | In Progress | $62,500 | $60,000 |
| O003 | Pacific Innovations | Asia-Pacific | New | $35,800 | $36,000 |
| ... | 9 more orders | ... | ... | ... | ... |

## Troubleshooting

### "Grid shows no data"
```bash
# Rebuild project
dotnet clean
dotnet build
dotnet run
```

### "Components don't render"
- Clear browser cache (Ctrl+Shift+Delete)
- Check browser console (F12)
- Verify `Program.cs` has `AddSyncfusionBlazor()`

### ".NET 8 not found"
```bash
dotnet --version
# If not 8.0.x, download from:
# https://dotnet.microsoft.com/download/dotnet/8.0
```

### "Port 7201 in use"
- Edit `Properties/launchSettings.json`
- Change port number to 7202 or another available port

## Next Steps

1. **Understand the code**: Read `README.md`
2. **Customize UI**: Edit `wwwroot/css/app.css`
3. **Add more data**: Modify `Services/OrderDataService.cs`
4. **Go to production**: Follow `PRODUCTION.md`

## Key Files

```
Components/
├── Dashboard.razor ........... Main component, orchestrates all
├── OrderGrid.razor ........... Syncfusion Grid with editing
├── OrderChart.razor .......... Syncfusion Chart visualization
├── OrderSpreadsheet.razor .... Syncfusion Spreadsheet report
├── SummaryCards.razor ........ KPI cards
└── ActivityPanel.razor ....... Activity log

Models/
├── OrderRecord.cs ............ Order data model
└── ActivityLog.cs ............ Activity model

Services/
├── IOrderDataService.cs ...... Interface for data access
└── OrderDataService.cs ....... In-memory implementation

Program.cs .................... Main entry point
App.razor ..................... Root component
wwwroot/css/app.css ........... Styling
```

## Common Customizations

### Change Colors
Edit `wwwroot/css/app.css`:
```css
.dashboard-header {
    background: linear-gradient(135deg, #YOUR_COLOR1 0%, #YOUR_COLOR2 100%);
}
```

### Add More Orders
Edit `Services/OrderDataService.cs`:
```csharp
_orders = new List<OrderRecord>
{
    new OrderRecord { OrderID = "O013", ... },
    // Add more...
};
```

### Change Chart Type
Edit `Components/OrderChart.razor`:
```csharp
Type="ChartSeriesType.Bar" // Column, Bar, Line, Area, etc.
```

## Performance Tips

### For Large Datasets (1000+ rows)
In `Components/OrderGrid.razor`:
```csharp
<SfGrid AllowVirtualization="true" RowHeight="35">
```

### For Slow Networks
- Paginate data on server (see `PRODUCTION.md`)
- Compress API responses
- Cache frequently accessed data

## Reference URLs

- **Syncfusion Grid Docs**: https://www.syncfusion.com/blazor-components/blazor-grid
- **Syncfusion Chart Docs**: https://www.syncfusion.com/blazor-components/blazor-charts
- **Syncfusion Spreadsheet Docs**: https://www.syncfusion.com/blazor-components/blazor-spreadsheet
- **.NET 8 Docs**: https://learn.microsoft.com/en-us/dotnet/core/
- **Blazor Docs**: https://learn.microsoft.com/en-us/aspnet/core/blazor

## Support

**Issue? Check:**
1. Browser console (F12) for errors
2. `SETUP.md` for detailed setup
3. `README.md` for features
4. `PRODUCTION.md` for backend setup

**Still stuck?**
- Syncfusion Forums: https://www.syncfusion.com/forums/
- Stack Overflow: Tag with `blazor` and `syncfusion`

---

**Ready to ship to production?** → Read `PRODUCTION.md`

**Version**: 1.0.0 | **.NET**: 8.0 | **Syncfusion**: 25.1.34
