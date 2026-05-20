# Creating Relationships

This guide explains the different ways to create relationships between entities using the DataMiner Relationships library.

## Understanding Relationships

The Relationships library supports two types of connections:

1. **Bidirectional Links (Symmetric)**: Two-way relationships where both entities are equal participants
2. **Directed Relations (Asymmetric)**: One-way relationships with a clear source and target

### Link vs. Relation

| Aspect | Link | Relation |
|--------|------|----------|
| Direction | Bidirectional (A ↔ B) | Unidirectional (A → B) |
| Storage | Two complementary relations | Single relation |
| Use Case | Peers, connections, associations | Hierarchies, dependencies, flows |
| Method | `manager.Link(a, b)` | `manager.Relate(source, target)` |

## Creating Bidirectional Links

Use bidirectional links when entities have a symmetric relationship (neither is "parent" or "source").

### Basic Link Creation

```csharp
using (var manager = engine.GetRelationshipManager())
{
    var router = new EntityDescriptor
    {
        ID = "router-1",
        DisplayName = "Core Router",
        ModelName = "Router",
        SolutionID = "network-topology",
        SolutionName = "Network Topology"
    };

    var switch = new EntityDescriptor
    {
        ID = "switch-1",
        DisplayName = "Access Switch",
        ModelName = "Switch",
        SolutionID = "network-topology",
        SolutionName = "Network Topology"
    };

    // Creates two relations: router → switch AND switch → router
    manager.Link(router, switch);
}
```

### What Happens Behind the Scenes

When you call `manager.Link(a, b)`, the library:

1. **Stores or updates both entities** in the entity repository
2. **Creates two relations**:
   - `a → b` (source: a, target: b)
   - `b → a` (source: b, target: a)
3. **Maintains status information** for lifecycle tracking

### Link Examples

```csharp
// Example 1: Connecting elements
var element1 = new EntityDescriptor
{
    ID = $"{dmaId}/{elementId1}",
    DisplayName = "Video Encoder 1",
    ModelName = "Element",
    SolutionID = "broadcasting",
    SolutionName = "Broadcasting Solution"
};

var element2 = new EntityDescriptor
{
    ID = $"{dmaId}/{elementId2}",
    DisplayName = "Multiplexer 1",
    ModelName = "Element",
    SolutionID = "broadcasting",
    SolutionName = "Broadcasting Solution"
};

manager.Link(element1, element2);

// Example 2: Connecting a booking to a service
var booking = new EntityDescriptor
{
    ID = bookingId.ToString(),
    DisplayName = "Live Broadcast Event",
    ModelName = "Booking",
    SolutionID = "resource-scheduling",
    SolutionName = "Resource Scheduling"
};

var service = new EntityDescriptor
{
    ID = $"{dmaId}/{serviceId}",
    DisplayName = "Broadcast Service",
    ModelName = "Service",
    SolutionID = "resource-scheduling",
    SolutionName = "Resource Scheduling"
};

manager.Link(booking, service);

// Example 3: Connecting custom business entities
var contract = new EntityDescriptor
{
    ID = "contract-2024-001",
    DisplayName = "Annual Service Contract",
    ModelName = "Contract",
    SolutionID = "business-management",
    SolutionName = "Business Management"
};

var customer = new EntityDescriptor
{
    ID = "customer-acme",
    DisplayName = "Acme Corporation",
    ModelName = "Customer",
    SolutionID = "business-management",
    SolutionName = "Business Management"
};

manager.Link(contract, customer);
```

## Creating Directed Relations

Use directed relations when there's a clear source-to-target relationship (dependencies, hierarchies, flows).

### Basic Relation Creation

```csharp
using (var manager = engine.GetRelationshipManager())
{
    var source = new EntityDescriptor
    {
        ID = "input-1",
        DisplayName = "Video Input",
        ModelName = "Input",
        SolutionID = "media-processing",
        SolutionName = "Media Processing"
    };

    var target = new EntityDescriptor
    {
        ID = "processor-1",
        DisplayName = "Video Processor",
        ModelName = "Processor",
        SolutionID = "media-processing",
        SolutionName = "Media Processing"
    };

    // Creates only: source → target
    manager.Relate(source, target);
}
```

### Relation Examples

```csharp
// Example 1: Dependency relationship
var database = new EntityDescriptor
{
    ID = "db-server-1",
    DisplayName = "Database Server",
    ModelName = "Database",
    SolutionID = "infrastructure",
    SolutionName = "Infrastructure"
};

var webApp = new EntityDescriptor
{
    ID = "webapp-1",
    DisplayName = "Web Application",
    ModelName = "Application",
    SolutionID = "infrastructure",
    SolutionName = "Infrastructure"
};

// webApp depends on database
manager.Relate(webApp, database);

// Example 2: Data flow
var collector = new EntityDescriptor
{
    ID = "collector-1",
    DisplayName = "Data Collector",
    ModelName = "Collector",
    SolutionID = "data-pipeline",
    SolutionName = "Data Pipeline"
};

var processor = new EntityDescriptor
{
    ID = "processor-1",
    DisplayName = "Data Processor",
    ModelName = "Processor",
    SolutionID = "data-pipeline",
    SolutionName = "Data Pipeline"
};

var storage = new EntityDescriptor
{
    ID = "storage-1",
    DisplayName = "Data Storage",
    ModelName = "Storage",
    SolutionID = "data-pipeline",
    SolutionName = "Data Pipeline"
};

// Create a processing pipeline
manager.Relate(collector, processor);
manager.Relate(processor, storage);

// Example 3: Hierarchical relationship
var datacenter = new EntityDescriptor
{
    ID = "dc-ny-1",
    DisplayName = "New York Datacenter",
    ModelName = "Datacenter",
    SolutionID = "infrastructure",
    SolutionName = "Infrastructure"
};

var rack = new EntityDescriptor
{
    ID = "rack-ny-1-a01",
    DisplayName = "Rack A01",
    ModelName = "Rack",
    SolutionID = "infrastructure",
    SolutionName = "Infrastructure"
};

// Hierarchy: rack is part of datacenter
manager.Relate(rack, datacenter);
```

## Removing Relationships

You can remove relationships using the inverse operations.

### Removing Bidirectional Links

```csharp
// Remove both directions of a link
manager.Unlink(entity1, entity2);
```

This removes:
- `entity1 → entity2`
- `entity2 → entity1`

### Removing Directed Relations

```csharp
// Remove only the source → target relation
manager.Unrelate(source, target);
```

This removes only the specified direction. If a reverse relation exists, it remains intact.

### Complete Removal Example

```csharp
using (var manager = engine.GetRelationshipManager())
{
    var router = new EntityDescriptor
    {
        ID = "router-1",
        DisplayName = "Core Router",
        ModelName = "Router",
        SolutionID = "network",
        SolutionName = "Network"
    };

    var switch = new EntityDescriptor
    {
        ID = "switch-1",
        DisplayName = "Access Switch",
        ModelName = "Switch",
        SolutionID = "network",
        SolutionName = "Network"
    };

    // Create a link
    manager.Link(router, switch);

    // Later, remove it
    manager.Unlink(router, switch);

    // For directed relations
    var source = new EntityDescriptor { /* ... */ };
    var target = new EntityDescriptor { /* ... */ };

    manager.Relate(source, target);
    manager.Unrelate(source, target);  // Only removes source → target
}
```

## Batch Operations

Create multiple relationships efficiently using batch operations directly with repositories.

### Creating Multiple Links

```csharp
using (var manager = engine.GetRelationshipManager())
{
    // Create multiple entity descriptors
    var entities = new List<EntityDescriptor>
    {
        new EntityDescriptor
        {
            ID = "entity-1",
            DisplayName = "Entity 1",
            ModelName = "Device",
            SolutionID = "solution-x",
            SolutionName = "Solution X"
        },
        new EntityDescriptor
        {
            ID = "entity-2",
            DisplayName = "Entity 2",
            ModelName = "Device",
            SolutionID = "solution-x",
            SolutionName = "Solution X"
        },
        new EntityDescriptor
        {
            ID = "entity-3",
            DisplayName = "Entity 3",
            ModelName = "Device",
            SolutionID = "solution-x",
            SolutionName = "Solution X"
        }
    };

    // Store all entities at once
    manager.Entities.CreateOrUpdate(entities);

    // Create multiple relations
    var relations = new List<Relation>
    {
        Relation.Between(entities[0], entities[1]),
        Relation.Between(entities[1], entities[0]),
        Relation.Between(entities[1], entities[2]),
        Relation.Between(entities[2], entities[1])
    };

    manager.Relations.Create(relations);
}
```

### Mesh/Star Topologies

```csharp
// Star topology: Connect all devices to a central hub
var hub = new EntityDescriptor
{
    ID = "hub-central",
    DisplayName = "Central Hub",
    ModelName = "Hub",
    SolutionID = "topology",
    SolutionName = "Network Topology"
};

var devices = Enumerable.Range(1, 5).Select(i => new EntityDescriptor
{
    ID = $"device-{i}",
    DisplayName = $"Device {i}",
    ModelName = "Device",
    SolutionID = "topology",
    SolutionName = "Network Topology"
}).ToList();

// Link each device to the hub
foreach (var device in devices)
{
    manager.Link(hub, device);
}

// Full mesh: Connect every device to every other device
for (int i = 0; i < devices.Count; i++)
{
    for (int j = i + 1; j < devices.Count; j++)
    {
        manager.Link(devices[i], devices[j]);
    }
}
```

## Entity Lifecycle and Status

Entities have a `Status` property that tracks their lifecycle state:

```csharp
public enum EntityStatus
{
    Active = 0,    // Entity is active and operational
    Inactive = 1   // Entity has been marked as inactive
}
```

### Marking Entities as Inactive

```csharp
// Get the entity
var key = new EntityKey("solution-id", "ModelName", "entity-id");
var filter = new ANDFilterElement<EntityDescriptor>(
    EntityDescriptorExposers.SolutionID.Equal(key.SolutionID),
    EntityDescriptorExposers.ModelName.Equal(key.ModelName),
    EntityDescriptorExposers.ID.Equal(key.ID)
);

var entity = manager.Entities.Read(filter).FirstOrDefault();

if (entity != null)
{
    // Mark as inactive
    entity.Status = EntityStatus.Inactive;
    manager.Entities.Update(entity);
}
```

When you mark an entity as `Inactive`:
- The entity descriptor remains in storage
- Associated relations are automatically marked with the inactive status
- Queries can filter out inactive entities

> [!TIP]
> Marking entities as inactive (using `EntityStatus.Inactive`) is preferred over hard deletion for audit trails and historical data.

## Best Practices

### 1. Consistent Solution IDs

Use consistent `SolutionID` values across your application:

```csharp
// Good: Define constants
public static class MySolution
{
    public const string ID = "my-solution-id";
    public const string Name = "My Solution";
}

var entity = new EntityDescriptor
{
    ID = "entity-1",
    DisplayName = "Entity 1",
    ModelName = "Type",
    SolutionID = MySolution.ID,
    SolutionName = MySolution.Name
};

// Avoid: Hard-coding strings everywhere
```

### 2. Meaningful IDs

Use IDs that are unique, stable, and meaningful:

```csharp
// Good: Use stable DataMiner identifiers
ID = $"{dmaId}/{elementId}"                    // Elements
ID = $"{dmaId}/{serviceId}"                    // Services
ID = bookingGuid.ToString()                     // Bookings
ID = $"{domInstanceId}"       // DOM instances

// Avoid: Temporary or auto-generated IDs without persistence
ID = Guid.NewGuid().ToString()  // Only if you store this GUID somewhere!
```

### 3. Model Names for Grouping

Use `ModelName` to categorize entities for easy filtering:

```csharp
// Good: Consistent model names
ModelName = "Element"
ModelName = "Service"
ModelName = "Booking"
ModelName = "Customer"

// Group custom entities logically
ModelName = "VideoEncoder"
ModelName = "VideoDecoder"
```

### 4. Parent Relationships

Use `ParentID` and `ParentModelName` for hierarchical context:

```csharp
var device = new EntityDescriptor
{
    ID = "device-123",
    DisplayName = "Device 123",
    ModelName = "Device",
    SolutionID = "inventory",
    SolutionName = "Inventory Management",
    ParentID = "rack-5",              // This device is in rack-5
    ParentModelName = "Rack"
};
```

### 5. Avoid Duplicate Links

The library won't create duplicate relations, but be mindful:

```csharp
manager.Link(a, b);
manager.Link(a, b);  // Idempotent: won't create duplicates but will update entities

// However, this creates 4 relations total (2 links):
manager.Link(a, b);
manager.Link(b, c);
```

### 6. Clean Up When Done

Always dispose of the manager:

```csharp
// Option 1: using statement (preferred)
using (var manager = engine.GetRelationshipManager())
{
    // ... use manager ...
}

// Option 2: explicit disposal
var manager = engine.GetRelationshipManager();
try
{
    // ... use manager ...
}
finally
{
    manager.Dispose();
}
```

## Common Scenarios

### Scenario 1: Ticketing-Asset Integration

```csharp
// Link tickets to affected assets across solutions
var ticket = new EntityDescriptor
{
    ID = "TICKET-2024-001234",
    DisplayName = "Network Outage - Building A",
    ModelName = "Ticket",
    SolutionID = "ticketing-system",
    SolutionName = "Ticketing System"
};

var assetIds = new[] { "ASSET-SW-101", "ASSET-RT-205", "ASSET-FW-001" };
foreach (var assetId in assetIds)
{
    var asset = new EntityDescriptor
    {
        ID = assetId,
        DisplayName = assetId.Replace("ASSET-", ""),
        ModelName = "NetworkDevice",
        SolutionID = "asset-management",
        SolutionName = "Asset Management"
    };
    
    // Use Link for bidirectional relationship
    manager.Link(ticket, asset);
}
```

### Scenario 2: Booking Resources

```csharp
// Link a booking to its resources
var booking = new EntityDescriptor
{
    ID = bookingId.ToString(),
    DisplayName = bookingName,
    ModelName = "Booking",
    SolutionID = "resource-scheduler",
    SolutionName = "Resource Scheduler"
};

foreach (var resourceId in resourceIds)
{
    var resource = new EntityDescriptor
    {
        ID = resourceId.ToString(),
        DisplayName = GetResourceName(resourceId),
        ModelName = "Resource",
        SolutionID = "resource-scheduler",
        SolutionName = "Resource Scheduler"
    };
    
    manager.Link(booking, resource);
}
```

### Scenario 3: Signal Flow Chain

```csharp
// Model a signal processing chain
var input = new EntityDescriptor { /* Input source */ };
var encoder = new EntityDescriptor { /* Encoder */ };
var multiplexer = new EntityDescriptor { /* Multiplexer */ };
var transmitter = new EntityDescriptor { /* Transmitter */ };

// Create a directional flow
manager.Relate(input, encoder);
manager.Relate(encoder, multiplexer);
manager.Relate(multiplexer, transmitter);
```

## Next Steps

- **[Querying Relationships](./retrieve-linked-objects.md)** - Learn how to retrieve and traverse relationships
- **[Advanced Usage](./advanced-usage.md)** - Explore filtering, performance optimization, and advanced patterns
- **[Examples](./examples.md)** - See complete real-world examples

## See Also

- **[Best Practices](./best-practices.md)** - Comprehensive guidelines for effective use
- [API Reference](./api-reference.md) - Complete API documentation
- [Getting Started](./getting-started.md) - Installation and setup guide
- [Querying Relationships](./retrieve-linked-objects.md) - Traversal and query patterns
