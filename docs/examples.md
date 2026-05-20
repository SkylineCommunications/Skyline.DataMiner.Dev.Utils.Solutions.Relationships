# Examples

Real-world examples demonstrating common use cases for the DataMiner Relationships library.

## Table of Contents

1. [Ticketing-Asset Integration](#ticketing-asset-integration)
2. [Booking-Resource Tracking](#booking-resource-tracking)
3. [Booking-Device Integration](#booking-device-integration)
4. [GQI Data Source Integration](#gqi-data-source-integration)

---

## Ticketing-Asset Integration

Link tickets from a ticketing solution to assets from an asset management solution for comprehensive issue tracking.

### Automation Script: Link Ticket to Assets

```csharp
using System;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
using Skyline.DataMiner.Solutions.Relationships;

public class Script
{
    public void Run(IEngine engine)
    {
        try
        {
            using (var manager = engine.GetRelationshipManager())
            {
                // Create ticket entity from ticketing solution
                var ticket = new EntityDescriptor
                {
                    ID = "TICKET-2024-001234",
                    DisplayName = "Network Outage - Building A",
                    ModelName = "Ticket",
                    SolutionID = "ticketing-system",
                    SolutionName = "Ticketing System"
                };
                
                // Define affected assets from asset management solution
                var assets = new[]
                {
                    new EntityDescriptor
                    {
                        ID = "ASSET-SW-101",
                        DisplayName = "Core Switch - Building A",
                        ModelName = "NetworkDevice",
                        SolutionID = "asset-management",
                        SolutionName = "Asset Management"
                    },
                    new EntityDescriptor
                    {
                        ID = "ASSET-RT-205",
                        DisplayName = "Edge Router - Floor 2",
                        ModelName = "NetworkDevice",
                        SolutionID = "asset-management",
                        SolutionName = "Asset Management"
                    },
                    new EntityDescriptor
                    {
                        ID = "ASSET-FW-001",
                        DisplayName = "Firewall - Main Gateway",
                        ModelName = "NetworkDevice",
                        SolutionID = "asset-management",
                        SolutionName = "Asset Management"
                    }
                };
                
                // Link ticket to affected assets (bidirectional)
                foreach (var asset in assets)
                {
                    manager.Link(ticket, asset);
                }
            
                engine.GenerateInformation($"Linked ticket to {assets.Length} assets");
            }
        }
        catch (Exception ex)
        {
            engine.GenerateInformation($"Error: {ex.Message}");
            throw;
        }
    }
}
```

### Automation Script: Find All Tickets for an Asset

```csharp
using System;
using System.Linq;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Solutions.Relationships;

public class Script
{
    public void Run(IEngine engine)
    {
        try
        {
            using (var manager = engine.GetRelationshipManager())
            {
                // Asset to query from asset management
                var asset = new EntityDescriptor
                {
                    ID = "ASSET-SW-101",
                    DisplayName = "Core Switch - Building A",
                    ModelName = "NetworkDevice",
                    SolutionID = "asset-management",
                    SolutionName = "Asset Management"
                };
                
                // Find all tickets linked to this asset
                var tickets = manager.From(asset)
                    .Where(EntityDescriptorExposers.ModelName.Equal("Ticket"))
                    .GetNeighbors();
                
                engine.GenerateInformation($"Asset has {tickets.Count()} related tickets:");
                
                foreach (var ticket in tickets)
                {
                    engine.GenerateInformation($"  - {ticket.DisplayName} (ID: {ticket.ID})");
                }
                
                // Use for impact analysis
                if (tickets.Any())
                {
                    engine.GenerateInformation("\nWARNING: Asset has open tickets. Maintenance may impact active issues.");
                }
            }
        }
        catch (Exception ex)
        {
            engine.GenerateInformation($"Error: {ex.Message}");
        }
    }
}
```

---

## Booking-Resource Tracking

Link bookings to their assigned resources for quick lookups and impact analysis.

### Automation Script: Create Booking with Resources

```csharp
using System;
using System.Linq;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Net.ResourceManager.Objects;
using Skyline.DataMiner.Solutions.Relationships;

public class Script
{
    public void Run(IEngine engine)
    {
        try
        {
            using (var manager = engine.GetRelationshipManager())
            {
                // Booking info
                var bookingId = Guid.Parse("12345678-1234-1234-1234-123456789abc");
                var bookingName = "Live Event Broadcast";
                
                // Resource IDs used in booking
                var resourceIds = new[]
                {
                    Guid.Parse("resource-guid-1"),
                    Guid.Parse("resource-guid-2"),
                    Guid.Parse("resource-guid-3")
                };
                
                // Create booking entity
                var booking = new EntityDescriptor
                {
                    ID = bookingId.ToString(),
                    DisplayName = bookingName,
                    ModelName = "Booking",
                    SolutionID = "resource-scheduler",
                    SolutionName = "Resource Scheduler"
                };
                
                // Link booking to resources
                foreach (var resourceId in resourceIds)
                {
                    var resource = new EntityDescriptor
                    {
                        ID = resourceId.ToString(),
                        DisplayName = GetResourceName(engine, resourceId),
                        ModelName = "Resource",
                        SolutionID = "resource-scheduler",
                        SolutionName = "Resource Scheduler"
                    };
                    
                    manager.Link(booking, resource);
                }
                
                engine.GenerateInformation($"Booking linked to {resourceIds.Length} resources");
            }
        }
        catch (Exception ex)
        {
            engine.GenerateInformation($"Error: {ex.Message}");
        }
    }
    
    private string GetResourceName(IEngine engine, Guid resourceId)
    {
        // Implementation to get resource name
        return $"Resource {resourceId}";
    }
}
```

### Automation Script: Find All Bookings Using a Resource

```csharp
using System;
using System.Linq;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Solutions.Relationships;

public class Script
{
    public void Run(IEngine engine)
    {
        try
        {
            using (var manager = engine.GetRelationshipManager())
            {
                var resourceId = Guid.Parse("resource-guid-1");
                
                var resource = new EntityDescriptor
                {
                    ID = resourceId.ToString(),
                    DisplayName = "Resource",
                    ModelName = "Resource",
                    SolutionID = "resource-scheduler",
                    SolutionName = "Resource Scheduler"
                };
                
                // Find all bookings using this resource
                var bookings = manager.From(resource)
                    .Where(EntityDescriptorExposers.ModelName.Equal("Booking"))
                    .GetNeighbors();
                
                engine.GenerateInformation($"Resource is used in {bookings.Count()} bookings:");
                
                foreach (var booking in bookings)
                {
                    engine.GenerateInformation($"  - {booking.DisplayName}");
                }
            }
        }
        catch (Exception ex)
        {
            engine.GenerateInformation($"Error: {ex.Message}");
        }
    }
}
```

---
## Booking-Device Integration

Link bookings from resource scheduling to network devices for equipment reservation and utilization tracking.

### Automation Script: Link Booking to Network Devices

```csharp
using System;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Solutions.Relationships;

public class Script
{
    public void Run(IEngine engine)
    {
        try
        {
            using (var manager = engine.GetRelationshipManager())
            {
                // Booking entity (from resource scheduling solution)
                var booking = new EntityDescriptor
                {
                    ID = Guid.NewGuid().ToString(),
                    DisplayName = "Live Sports Event",
                    ModelName = "Booking",
                    SolutionID = "resource-scheduler",
                    SolutionName = "Resource Scheduler"
                };
                
                // Network devices (from network management solution)
                var encoder = new EntityDescriptor
                {
                    ID = "device-encoder-1",
                    DisplayName = "Encoder 1",
                    ModelName = "Encoder",
                    SolutionID = "network-management",
                    SolutionName = "Network Management"
                };
                
                var transmitter = new EntityDescriptor
                {
                    ID = "device-transmitter-1",
                    DisplayName = "Transmitter 1",
                    ModelName = "Transmitter",
                    SolutionID = "network-management",
                    SolutionName = "Network Management"
                };
                
                // Link booking to network devices
                manager.Link(booking, encoder);
                manager.Link(booking, transmitter);
                
                engine.GenerateInformation("Booking-device relationships created");
                
                // Find all network devices used in any booking
                var networkDevices = manager.Entities.Read(
                    EntityDescriptorExposers.SolutionID.Equal("network-management")
                );
                
                foreach (var device in networkDevices)
                {
                    var bookings = manager.From(device)
                        .Where(EntityDescriptorExposers.SolutionID.Equal("resource-scheduler"))
                        .Where(EntityDescriptorExposers.ModelName.Equal("Booking"))
                        .GetNeighbors();
                    
                    if (bookings.Any())
                    {
                        engine.GenerateInformation($"{device.DisplayName} used in {bookings.Count()} bookings");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            engine.GenerateInformation($"Error: {ex.Message}");
        }
    }
}
```

---

## GQI Data Source Integration

Use relationships in custom GQI data sources for dashboards and low-code apps.

### GQI Data Source: Show Related Entities

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using Skyline.DataMiner.Analytics.GenericInterface;
using Skyline.DataMiner.Solutions.Relationships;

[GQIMetaData(Name = "Related Entities")]
public class RelatedEntitiesDataSource : IGQIDataSource, IGQIInputArguments, IGQIOnInit
{
    private GQIDMS _dms;
    private GQIStringArgument _entityIdArg = new GQIStringArgument("Entity ID") { IsRequired = true };
    private GQIStringArgument _solutionIdArg = new GQIStringArgument("Solution ID") { IsRequired = true };
    private GQIStringArgument _modelNameArg = new GQIStringArgument("Model Name") { IsRequired = true };
    private GQIIntArgument _maxDepthArg = new GQIIntArgument("Max Depth") { IsRequired = false, DefaultValue = 1 };
    
    private string _entityId;
    private string _solutionId;
    private string _modelName;
    private int _maxDepth;
    
    public GQIArgument[] GetInputArguments()
    {
        return new GQIArgument[] { _entityIdArg, _solutionIdArg, _modelNameArg, _maxDepthArg };
    }
    
    public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
    {
        _entityId = args.GetArgumentValue(_entityIdArg);
        _solutionId = args.GetArgumentValue(_solutionIdArg);
        _modelName = args.GetArgumentValue(_modelNameArg);
        _maxDepth = args.GetArgumentValue(_maxDepthArg);
        
        return new OnArgumentsProcessedOutputArgs();
    }
    
    public OnInitOutputArgs OnInit(OnInitInputArgs args)
    {
        _dms = args.DMS;
        return new OnInitOutputArgs();
    }
    
    public GQIColumn[] GetColumns()
    {
        return new GQIColumn[]
        {
            new GQIStringColumn("Entity ID"),
            new GQIStringColumn("Display Name"),
            new GQIStringColumn("Model Name"),
            new GQIStringColumn("Solution ID"),
            new GQIIntColumn("Depth"),
            new GQIStringColumn("Via")
        };
    }
    
    public GQIPage GetNextPage(GetNextPageInputArgs args)
    {
        var rows = new List<GQIRow>();
        
        try
        {
            using (var manager = _dms.GetRelationshipManager())
            {
                var key = new EntityKey(_solutionId, _modelName, _entityId);
                
                var results = manager.From(key).Traverse(maxDepth: _maxDepth);
                
                foreach (var result in results)
                {
                    var cells = new[]
                    {
                        new GQICell { Value = result.Entity.ID },
                        new GQICell { Value = result.Entity.DisplayName },
                        new GQICell { Value = result.Entity.ModelName },
                        new GQICell { Value = result.Entity.SolutionID },
                        new GQICell { Value = result.Depth },
                        new GQICell { Value = result.Via.DisplayName }
                    };
                    
                    rows.Add(new GQIRow(cells));
                }
            }
        catch (Exception ex)
        {
            // Log error
        }
        
        return new GQIPage(rows.ToArray());
    }
}
```

### GQI Data Source: Asset Ticket History

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using Skyline.DataMiner.Analytics.GenericInterface;
using Skyline.DataMiner.Solutions.Relationships;

[GQIMetaData(Name = "Asset Ticket History")]
public class AssetTicketHistoryDataSource : IGQIDataSource, IGQIInputArguments, IGQIOnInit
{
    private GQIDMS _dms;
    private GQIStringArgument _assetIdArg = new GQIStringArgument("Asset ID") { IsRequired = true };
    
    private string _assetId;
    
    public GQIArgument[] GetInputArguments()
    {
        return new GQIArgument[] { _assetIdArg };
    }
    
    public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
    {
        _assetId = args.GetArgumentValue(_assetIdArg);
        return new OnArgumentsProcessedOutputArgs();
    }
    
    public OnInitOutputArgs OnInit(OnInitInputArgs args)
    {
        _dms = args.DMS;
        return new OnInitOutputArgs();
    }
    
    public GQIColumn[] GetColumns()
    {
        return new GQIColumn[]
        {
            new GQIStringColumn("Ticket ID"),
            new GQIStringColumn("Ticket Description"),
            new GQIStringColumn("Related Assets Count"),
            new GQIStringColumn("Impact Scope")
        };
    }
    
    public GQIPage GetNextPage(GetNextPageInputArgs args)
    {
        var rows = new List<GQIRow>();
        
        try
        {
            using (var manager = _dms.GetRelationshipManager())
            {
                var asset = new EntityDescriptor
                {
                    ID = _assetId,
                    DisplayName = "Asset",
                    ModelName = "NetworkDevice",
                    SolutionID = "asset-management",
                    SolutionName = "Asset Management"
                };
                
                // Find all tickets related to this asset
                var tickets = manager.From(asset)
                    .Where(EntityDescriptorExposers.ModelName.Equal("Ticket"))
                    .GetNeighbors()
                    .ToList();
                
                // For each ticket, count how many assets are affected
                foreach (var ticket in tickets)
                {
                    var affectedAssets = manager.From(ticket)
                        .Where(EntityDescriptorExposers.ModelName.Equal("NetworkDevice"))
                        .GetNeighbors()
                        .Count();
                    
                    // Determine impact scope
                    var impactScope = affectedAssets == 1 ? "Single Asset" : 
                                      affectedAssets <= 3 ? "Limited" : 
                                      affectedAssets <= 10 ? "Moderate" : "Wide";
                    
                    var cells = new[]
                    {
                        new GQICell { Value = ticket.ID },
                        new GQICell { Value = ticket.DisplayName },
                        new GQICell { Value = affectedAssets.ToString() },
                        new GQICell { Value = impactScope }
                    };
                    
                    rows.Add(new GQIRow(cells));
                }
            }
        }
        catch (Exception ex)
        {
            // Log error
        }
        
        return new GQIPage(rows.ToArray());
    }
}
```

---

## See Also

- **[Best Practices](./best-practices.md)** - Comprehensive guidelines for using the library effectively
- [Getting Started](./getting-started.md) - Installation and setup
- [Creating Relationships](./creating-a-link-between-2-objects.md) - How to create links and relations
- [Querying Relationships](./retrieve-linked-objects.md) - Traversal and queries
- [Advanced Usage](./advanced-usage.md) - Advanced patterns and optimization
- [API Reference](./api-reference.md) - Complete API documentation
- [Querying Relationships](./querying-relationships.md) - Traversal and queries
- [Advanced Usage](./advanced-usage.md) - Advanced patterns and optimization
- [API Reference](./api-reference.md) - Complete API documentation
