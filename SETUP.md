# Setup & Installation Guide

## Quick Start (5 minutes)

### Prerequisites

- **Windows 10/11** or **macOS 10.15+** or **Linux** (any recent distro)
- **.NET 8 SDK** - Download from https://dotnet.microsoft.com/download/dotnet/8.0
- **Visual Studio 2022** (Community Edition is free) OR **VS Code** with C# extension

### Step 1: Verify .NET 8 Installation

```bash
dotnet --version
# Should output: 8.0.x
```

If you don't have .NET 8, install it first before proceeding.

### Step 2: Create the Project

```bash
# Option A: Create from scratch
dotnet new blazor -n AIAssistantSalesOperationsDashboard -f net8.0
cd AIAssistantSalesOperationsDashboard

# Option B: Use provided source code
# Copy all files from the provided repository into a new directory
cd AIAssistantSalesOperationsDashboard
```

### Step 3: Install Syncfusion Blazor Packages

```bash
# Run from project root directory

# Core Syncfusion packages
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

### Step 4: Update Program.cs

Ensure Program.cs contains:

```csharp
using Syncfusion.Blazor;

builder.Services.AddSyncfusionBlazor();
```

### Step 5: Run the Application

```bash
# Development server
dotnet run

# Or with watch mode (auto-reload on file changes)
dotnet watch run
```

The dashboard will open automatically at `https://localhost:7201`

### Step 6: Verify Dashboard

1. Check if you see the main dashboard with:
   - ✅ Summary cards (Total Orders, Delayed, Revenue, etc.)
   - ✅ Order Grid with 12 sample records
   - ✅ Column chart
   - ✅ Spreadsheet report
   - ✅ Activity panel
   - ✅ Data note explaining in-memory storage

## Visual Studio Setup

### Visual Studio 2022

1. **Create New Project**
   - File → New → Project
   - Select "Blazor Web App"
   - Set target framework to ".NET 8.0"

2. **Install Syncfusion Packages**
   - Tools → NuGet Package Manager → Package Manager Console
   - Run commands from Step 3 above

3. **Run Project**
   - Press F5 or click "Run" button
   - Browser opens automatically

### Visual Studio Code

1. **Open Terminal**
   - Ctrl+` or View → Terminal

2. **Install C# Extension**
   - Extensions (Ctrl+Shift+X)
   - Search "C#"
   - Install "C# Dev Kit" by Microsoft

3. **Run Project**
   - Terminal → `dotnet run`
   - Navigate to `https://localhost:7201`

## Docker Setup (Optional)

If you want to run in Docker:

### Create Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "AIAssistantSalesOperationsDashboard.dll"]
```

### Build and Run

```bash
# Build image
docker build -t sales-dashboard .

# Run container
docker run -p 5000:80 sales-dashboard

# Access at http://localhost:5000
```

## Common Issues & Solutions

### Issue 1: ".NET 8 SDK not found"

**Solution:**
```bash
# Check installed versions
dotnet --list-sdks

# Download and install .NET 8
# From: https://dotnet.microsoft.com/download/dotnet/8.0
```

### Issue 2: "Syncfusion package not found"

**Solution:**
- Ensure you're running commands from project root (where .csproj file is)
- Check internet connection
- Clear NuGet cache:
  ```bash
  dotnet nuget locals all --clear
  dotnet restore
  ```

### Issue 3: "Port 7201 already in use"

**Solution:**
- Edit `Properties/launchSettings.json`
- Change port number (e.g., 7202)
- Or kill process using port:
  ```bash
  # Windows
  netstat -ano | findstr :7201
  taskkill /PID <PID> /F
  
  # macOS/Linux
  lsof -i :7201
  kill -9 <PID>
  ```

### Issue 4: "JavaScript interop error with Syncfusion"

**Solution:**
- Clear browser cache (Ctrl+Shift+Delete)
- Verify index.html has Syncfusion script tags
- Check browser console for specific errors (F12)

### Issue 5: "Grid not showing data"

**Solution:**
- Verify `IOrderDataService` is registered in `Program.cs`
- Check browser console for errors
- Ensure `OrderDataService.cs` initializes with sample data
- Try `dotnet clean && dotnet build`

### Issue 6: "Syncfusion Trial Period Expired"

**Solution:**
- Option 1: Use community license (free for companies < $1M revenue)
  - Register at https://www.syncfusion.com/account/downloads
  - Get community license key
  - Add to Program.cs:
    ```csharp
    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_COMMUNITY_KEY");
    ```

- Option 2: Use open-source alternatives
  - Replace SfGrid with custom table
  - Replace SfChart with Chart.js or similar
  - See alternatives in README.md

## Project File Structure

After setup, your project should look like:

```
AIAssistantSalesOperationsDashboard/
├── Components/
│   ├── Dashboard.razor
│   ├── OrderGrid.razor
│   ├── OrderChart.razor
│   ├── OrderSpreadsheet.razor
│   ├── SummaryCards.razor
│   └── ActivityPanel.razor
├── Models/
│   ├── OrderRecord.cs
│   └── ActivityLog.cs
├── Services/
│   ├── IOrderDataService.cs
│   └── OrderDataService.cs
├── Layout/
│   └── MainLayout.razor
├── wwwroot/
│   ├── css/
│   │   └── app.css
│   └── index.html
├── Properties/
│   └── launchSettings.json
├── App.razor
├── Program.cs
├── AIAssistantSalesOperationsDashboard.csproj
├── README.md
└── SETUP.md
```

## NuGet Packages Summary

```xml
<!-- Core Syncfusion Packages -->
<PackageReference Include="Syncfusion.Blazor.Grids" Version="25.1.34" />
<PackageReference Include="Syncfusion.Blazor.Charts" Version="25.1.34" />
<PackageReference Include="Syncfusion.Blazor.Spreadsheet" Version="25.1.34" />
<PackageReference Include="Syncfusion.Blazor.Calendars" Version="25.1.34" />
<PackageReference Include="Syncfusion.Blazor.DropDowns" Version="25.1.34" />
<PackageReference Include="Syncfusion.Blazor.Buttons" Version="25.1.34" />
<PackageReference Include="Syncfusion.Blazor.Inputs" Version="25.1.34" />
<PackageReference Include="Syncfusion.Blazor.Popups" Version="25.1.34" />
<PackageReference Include="Syncfusion.Blazor.Notifications" Version="25.1.34" />

<!-- ASP.NET Core (included with .NET 8) -->
<PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="8.0.0" />
```

## Upgrade Syncfusion Version

To upgrade to a newer Syncfusion version:

```bash
# Replace 25.1.34 with desired version number
dotnet add package Syncfusion.Blazor.Grids --version 25.2.x
```

Check https://www.nuget.org/packages/Syncfusion.Blazor.Grids/ for latest version.

## Build for Production

### Release Build

```bash
# Create optimized release build
dotnet publish -c Release -o ./publish

# Output in ./publish directory
```

### Deployment

See README.md "Production Deployment" section for:
- ASP.NET Core backend setup
- Database integration
- Authentication/authorization
- Security considerations

## Performance Tuning

### Blazor Performance

```csharp
// In Program.cs - enable AOT compilation for better performance
builder.Services.AddBlazorWebAssemblyLogging();
```

### Grid Performance

For large datasets (1000+ rows):

```csharp
// Enable virtualization in OrderGrid.razor
<SfGrid AllowVirtualization="true" 
        RowHeight="35"
        VirtualScrollMode="VirtualScrollMode.Continuous">
```

### Chart Performance

For many data points:

```csharp
// Consider server-side aggregation
// Send pre-aggregated data instead of raw records
```

## Next Steps

1. ✅ Dashboard is running
2. 📚 Read README.md for feature overview
3. 🔧 Customize styling in `wwwroot/css/app.css`
4. 📊 Modify sample data in `Services/OrderDataService.cs`
5. 🌐 For production: Follow "Production Deployment" in README.md

## Support

- **Syncfusion Documentation**: https://www.syncfusion.com/blazor-components
- **.NET 8 Docs**: https://learn.microsoft.com/en-us/dotnet/
- **Blazor Docs**: https://learn.microsoft.com/en-us/aspnet/core/blazor
- **Stack Overflow**: Tag with `blazor` and `syncfusion-blazor`

---

**Last Updated**: January 2025
**Minimum Requirements**: .NET 8 SDK, Visual Studio 2022 / VS Code
