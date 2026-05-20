# Getting Started with DataMiner Relationships

This guide will help you set up and create your first relationship using the DataMiner Relationships library.

## Prerequisites

Before you begin, ensure you have:

1. **DataMiner System**: A running DataMiner 10.5.9+ Agent or cluster 
2. **SDM Registration Package**: Install the [SDM Registration](https://catalog.dataminer.services/details/52173e49-9185-4772-9b60-c186ee365a81) package from the DataMiner Catalog

> [!IMPORTANT]
> The Relationships library requires the SDM Registration package to be installed on your DataMiner system. Without it, the library will not function.

## Installation

The Relationships library provides different NuGet packages for different DataMiner development scenarios:

### For Automation Scripts

```powershell
Install-Package Skyline.DataMiner.Dev.Utils.Solutions.Relationships.Automation
```

Use this package when developing Automation Scripts (`.cs` files or C# Exe blocks in Automation).

### For Connectors/Protocols

```powershell
Install-Package Skyline.DataMiner.Dev.Utils.Solutions.Relationships.Protocol
```

Use this package when developing DataMiner connectors (also known as protocols or drivers).

### For GQI Data Sources

```powershell
Install-Package Skyline.DataMiner.Dev.Utils.Solutions.Relationships.GQI
```

Use this package when developing custom GQI data sources for Dashboards and Low-Code Apps.

### For Other Scenarios

```powershell
Install-Package Skyline.DataMiner.Dev.Utils.Solutions.Relationships.Common
```

Use this package for:
- Class libraries shared across multiple projects
- Console applications
- Unit tests
- Any scenario where you have direct access to an `IConnection` instance

## Your First Relationship

Let's create a simple relationship between two entities. This example works in an Automation Script, but the pattern is similar across all package types.

### Step 1: Get the Relationship Manager

```csharp
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Solutions.Relationships;

public class Script
{
    public void Run(IEngine engine)
    {
        // Get the relationship manager
        using (var manager = engine.GetRelationshipManager())
        {
            // Your code here...
        }
    }
}
```

> [!TIP]
> Always use the `using` statement or call `Dispose()` when done to properly clean up resources.

### Step 2: Create Entity Descriptors

An `EntityDescriptor` represents any entity in your DataMiner system. It requires several properties:

```csharp
var resource1 = new EntityDescriptor
{
    ID = "resource-guid-1",                      // Required: Unique identifier
    DisplayName = "Encoder Resource 1",          // Required: Human-readable name
    ModelName = "Resource",                      // Required: Type/category of entity
    SolutionID = "resource-management",          // Required: Solution identifier
    SolutionName = "Resource Management",        // Required: Solution display name
    ParentID = "pool-encoders",                  // Optional: Parent entity ID
    ParentModelName = "ResourcePool"             // Optional: Parent entity type
};

var resource2 = new EntityDescriptor
{
    ID = "resource-guid-2",
    DisplayName = "Decoder Resource 1",
    ModelName = "Resource",
    SolutionID = "resource-management",
    SolutionName = "Resource Management",
    ParentID = "pool-decoders",
    ParentModelName = "ResourcePool"
};
```

#### Property Descriptions

| Property | Required | Description |
|----------|----------|-------------|
| `ID` | Yes | Unique identifier for this entity (e.g., element ID, booking ID, custom key) |
| `DisplayName` | Yes | Human-readable name shown in UIs |
| `ModelName` | Yes | Category or type of entity (e.g., "Element", "Service", "Booking") |
| `SolutionID` | Yes | Unique identifier for the solution/application owning this entity |
| `SolutionName` | Yes | Display name of the solution/application |
| `ParentID` | No | ID of a parent entity (for hierarchical relationships) |
| `ParentModelName` | No | Model name of the parent entity |

> [!TIP]
> Keep your `SolutionID` consistent across your application. It's used to group related entities and filter queries efficiently.
> For standard solution use the ID from the SDM Registration entry.

### Step 3: Create the Relationship

Create a bidirectional link between the two entities:

```csharp
// Create a symmetric link (creates two relations: resource1 → resource2 and resource2 → resource1)
manager.Link(resource1, resource2);
```

That's it! You've created your first relationship. The `Link` method:
- Stores both entity descriptors if they don't exist, or updates them if they do
- Creates two complementary relations (bidirectional)
- Makes both entities queryable from each other

### Step 4: Query the Relationship

Now let's retrieve the related entities:

```csharp
// Get all entities directly related to the resource
var neighbors = manager.From(resource1).GetNeighbors();

foreach (var neighbor in neighbors)
{
    engine.GenerateInformation($"Related to: {neighbor.DisplayName} ({neighbor.ModelName})");
}
```

### Complete Example

Here's a complete Automation Script that creates and queries a relationship:

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
            // Get the relationship manager
            using (var manager = engine.GetRelationshipManager())
            {
            // Create entity descriptors
            var resource1 = new EntityDescriptor
            {
                ID = "resource-guid-1",
                DisplayName = "Encoder Resource 1",
                ModelName = "Resource",
                SolutionID = "resource-management",
                SolutionName = "Resource Management",
                ParentID = "pool-encoders",
                ParentModelName = "ResourcePool"
            };
            
            var resource2 = new EntityDescriptor
            {
                ID = "resource-guid-2",
                DisplayName = "Decoder Resource 1",
                ModelName = "Resource",
                SolutionID = "resource-management",
                SolutionName = "Resource Management",
                ParentID = "pool-decoders",
                ParentModelName = "ResourcePool"
            };
            
            // Create the relationship
            manager.Link(resource1, resource2);
            engine.GenerateInformation("Relationship created successfully!");
            
            // Query relationships
            var neighbors = manager.From(resource1).GetNeighbors();
            
            engine.GenerateInformation($"Found {neighbors.Count()} related entities:");
            foreach (var neighbor in neighbors)
            {
                engine.GenerateInformation($"  - {neighbor.DisplayName} ({neighbor.ModelName})");
            }            }        }
        catch (Exception ex)
        {
            engine.GenerateInformation($"Error: {ex.Message}");
            throw;
        }
    }
}
```

## Standard DataMiner Entities

When working with built-in DataMiner objects, use these standard values for consistency and interoperability:

| Entity Type | ModelName | ID Format | Example ID | SolutionID | SolutionName |
|-------------|-----------|-----------|------------|------------|--------------|
| **Element** | `"Element"` | `"{dmaId}/{elementId}"` | `"123/456"` | `"dataminer"` | `"DataMiner"` |
| **Service** | `"Service"` | `"{dmaId}/{serviceId}"` | `"123/789"` | `"dataminer"` | `"DataMiner"` |
| **View** | `"View"` | `"{viewId}"` | `"100"` | `"dataminer"` | `"DataMiner"` |
| **Booking** | `"Booking"` | `"{bookingGuid}"` | `"guid-string"` | `"srm"` | `"Service & Resource Management"` |
| **Resource** | `"Resource"` | `"{resourceGuid}"` | `"guid-string"` | `"srm"` | `"Service & Resource Management"` |
| **ResourcePool** | `"ResourcePool"` | `"{poolGuid}"` | `"guid-string"` | `"srm"` | `"Service & Resource Management"` |
| **Profile** | `"Profile"` | `"{profileGuid}"` | `"guid-string"` | `"profiles"` | `"Profile Management"` |
| **AutomationScript** | `"AutomationScript"` | `"{scriptName}"` | `"MyScript"` | `"automation"` | `"Automation"` |
| **Solution Object** | `"{ObjectType}"` | `"{objectId}"` | `"obj-guid"` | From SDM registration | From SDM registration |
| **DOM Instance** | `"DomInstance"` | `"{instanceId}"` | `"inst-guid"` | `"dom"` | `"my_module_id"` |
| **Custom Entity** | Your model name | Your ID format | Your example | `"your-solution-id"` | `"Your Solution Name"` |

> [!TIP]
> Using these standard values ensures consistency across your DataMiner system and makes relationships easier to query and understand.

### Example: Linking an Element to a Service

```csharp
var element = new EntityDescriptor
{
    ID = "123/456",
    DisplayName = "My Encoder",
    ModelName = "Element",
    SolutionID = "dataminer",
    SolutionName = "DataMiner"
};

var service = new EntityDescriptor
{
    ID = "123/789",
    DisplayName = "Broadcast Service",
    ModelName = "Service",
    SolutionID = "dataminer",
    SolutionName = "DataMiner"
};

manager.Link(element, service);
```

### Example: Linking a Booking to Resources

```csharp
var booking = new EntityDescriptor
{
    ID = bookingGuid.ToString(),
    DisplayName = "Live Event",
    ModelName = "Booking",
    SolutionID = "srm",
    SolutionName = "Service & Resource Management"
};

var resource = new EntityDescriptor
{
    ID = resourceGuid.ToString(),
    DisplayName = "Encoder Resource",
    ModelName = "Resource",
    SolutionID = "srm",
    SolutionName = "Service & Resource Management",
    ParentID = poolGuid.ToString(),
    ParentModelName = "ResourcePool"
};

manager.Link(booking, resource);
```

### Example: Linking Solution Objects

```csharp
// From the SDM model registration objects, use the ModelName
// and SolutionID/SolutionName from SDM solution registration objects
var ticket = new EntityDescriptor
{
    ID = ticketObject.ID,        // Use the Identifier property
    DisplayName = ticketObject.Name,     // Display name from the object
    ModelName = "Ticket",                // Model name from solution definition
    SolutionID = "c132decf-b918-4ee2-be25-1302f41e7705",  // From SDM registration
    SolutionName = "Ticketing System"    // From SDM registration
};

var asset = new EntityDescriptor
{
    ID = assetObject.ID,
    DisplayName = assetObject.Name,
    ModelName = "Asset",
    SolutionID = "5a1edac2-45aa-4498-8ab7-ee322d07da27",
    SolutionName = "InfraOps"
};

manager.Link(ticket, asset);
```

## Next Steps

Now that you understand the basics:

- **[Creating Relationships](./creating-relationships.md)** - Learn about directed relations vs. bidirectional links
- **[Querying Relationships](./querying-relationships.md)** - Explore graph traversal and advanced queries
- **[Advanced Usage](./advanced-usage.md)** - Filtering, batch operations, and performance optimization

## Common Issues

### "SDM Registration module not found"

**Solution**: Install the SDM Registration package on your DataMiner system from the [DataMiner Catalog](https://catalog.dataminer.services/details/52173e49-9185-4772-9b60-c186ee365a81).

### "NuGet package not found"

**Solution**: Ensure your NuGet package sources are configured correctly. Check your `nuget.config` file or Visual Studio's NuGet Package Manager settings.

### Entity not found after creation

**Solution**: The Relationships library has eventual consistency. In rare cases, there might be a small delay. If querying immediately after creation, consider adding the entities to your local cache or using the returned objects directly.

## FAQ

**Q: Can I use the same entity in multiple solutions?**  
A: Yes! The same physical entity should always use the same `EntityDescriptor` (same `SolutionID`, `ModelName`, and `ID`). The `SolutionID` identifies which domain/solution owns the entity, not which solution is creating relationships with it. For example, a Resource with ID "resource-123" from the "resource-management" solution should always use that same descriptor, even when different solutions (like "booking-system" or "workflow-manager") create relationships with it. Multiple solutions can link to the same entity, but they all use the same descriptor for that entity.

**Q: What happens if I create a link that already exists?**  
A: The library is idempotent. Creating an existing link will update the entity descriptors and ensure the relations exist, but won't create duplicates.

**Q: How do I delete a relationship?**  
A: Use `manager.Unlink(entity1, entity2)` to remove both directions, or `manager.Unrelate(source, target)` to remove a single directed relation. See [Creating Relationships](./creating-relationships.md) for details.

**Q: Can I use this with elements, services, bookings, etc.?**  
A: Absolutely! The library is entity-agnostic. You can create relationships between any DataMiner objects or even custom entities. Just create appropriate `EntityDescriptor` instances with meaningful IDs.

## See Also

- **[Migration Guide](./migration-guide.md)** - Migrating from Object Linking to Relationships
- **[Best Practices](./best-practices.md)** - Guidelines for using the library effectively
- [Creating Relationships](./creating-a-link-between-2-objects.md) - How to create links and relations
- [Querying Relationships](./retrieve-linked-objects.md) - Traversal and queries
- [Advanced Usage](./advanced-usage.md) - Advanced topics and optimization
- [Examples](./examples.md) - Real-world scenarios
- [API Reference](./api-reference.md) - Complete API documentation
