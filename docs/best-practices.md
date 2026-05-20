# Best Practices

This guide consolidates best practices for using the DataMiner Relationships library effectively and efficiently.

## Entity Design

### 1. Use Stable, Meaningful IDs

Entity IDs should be unique, stable, and meaningful. Avoid random GUIDs unless you persist them.

```csharp
// ✅ Good: Use stable DataMiner identifiers
ID = $"{dmaId}/{elementId}"                    // Elements
ID = $"{dmaId}/{serviceId}"                    // Services
ID = bookingGuid.ToString()                     // Bookings (GUID already exists)
ID = $"{domDefId}/{domInstanceId}"             // DOM instances
ID = resourceId.ToString()                      // Resources (GUID already exists)

// ❌ Avoid: Random GUIDs without persistence
ID = Guid.NewGuid().ToString()  // Only if you store this GUID somewhere!
```

**Why:** Stable IDs ensure you can reliably query and update entities over time. Random GUIDs make it impossible to find entities later.

### 2. Define Solution Constants

Use constants for `SolutionID` and `SolutionName` to ensure consistency across your code.

```csharp
// ✅ Good: Define constants
public static class MySolution
{
    public const string ID = "my-solution-id";
    public const string Name = "My Solution";
}

var entity = new EntityDescriptor
{
    ID = "entity-1",
    DisplayName = "Entity 1",
    ModelName = "Device",
    SolutionID = MySolution.ID,
    SolutionName = MySolution.Name
};

// ❌ Avoid: Hard-coding strings everywhere
var entity = new EntityDescriptor
{
    SolutionID = "my-solution-id",  // Typo-prone, inconsistent
    // ...
};
```

**Why:** Prevents typos, makes refactoring easier, and ensures all entities from your solution use the same identifiers.

### 3. Use Consistent Model Names

Use `ModelName` to categorize entities for easy filtering and grouping.

```csharp
// ✅ Good: Consistent model names
ModelName = "Element"
ModelName = "Service"
ModelName = "Booking"
ModelName = "NetworkDevice"
ModelName = "Ticket"

// Group related types logically
ModelName = "VideoEncoder"
ModelName = "VideoDecoder"
ModelName = "AudioProcessor"
```

**Why:** Consistent naming enables efficient filtering with `EntityDescriptorExposers.ModelName.Equal()` and makes your relationship graph easier to understand.

### 4. Leverage Parent Relationships

Use `ParentID` and `ParentModelName` for hierarchical context when entities have natural parent-child relationships.

```csharp
var device = new EntityDescriptor
{
    ID = "device-123",
    DisplayName = "Switch 123",
    ModelName = "Device",
    SolutionID = "inventory",
    SolutionName = "Inventory Management",
    ParentID = "rack-5",              // This device is in rack-5
    ParentModelName = "Rack"
};
```

**Why:** Parent properties enable efficient hierarchical queries and provide context without requiring additional relations.

### 5. Mark Inactive Entities Instead of Deleting

Use `EntityStatus.Inactive` instead of deleting entities when you need audit trails or historical data.

```csharp
// ✅ Good: Soft delete with status
entity.Status = EntityStatus.Inactive;
manager.Entities.Update(entity);

// Query only active entities
var activeEntities = manager.Entities.Read(
    new ANDFilterElement<EntityDescriptor>(
        EntityDescriptorExposers.SolutionID.Equal("my-solution"),
        EntityDescriptorExposers.Status.Equal(EntityStatus.Active)
    )
);

// ❌ Avoid: Hard delete (unless you really don't need the data)
manager.Entities.Delete(entity);  // Gone forever
```

**Why:** Soft deletion preserves audit trails, enables historical analysis, and prevents broken relationships.

## Performance Optimization

### 1. Always Use Batch Operations

Process multiple entities in single batch calls instead of loops with individual calls.

```csharp
// ✅ Good: Single batch operation
var entities = Enumerable.Range(1, 100).Select(i => new EntityDescriptor
{
    ID = $"entity-{i}",
    DisplayName = $"Entity {i}",
    ModelName = "Device",
    SolutionID = "solution",
    SolutionName = "Solution"
}).ToList();

manager.Entities.CreateOrUpdate(entities);

// ❌ Avoid: Individual operations in a loop
for (int i = 1; i <= 100; i++)
{
    var entity = new EntityDescriptor { /* ... */ };
    manager.Entities.CreateOrUpdate(entity);  // 100 individual DOM calls!
}
```

**Why:** Batch operations drastically reduce network overhead and DOM transaction costs. A single batch call is orders of magnitude faster than N individual calls.

### 2. Use Pagination for Large Result Sets

When querying large numbers of entities or relations, use pagination to avoid memory issues and timeouts.

```csharp
// ✅ Good: Paginated processing
var pageSize = 100;
var processedCount = 0;

var pages = manager.Entities.ReadPaged(
    filter: EntityDescriptorExposers.SolutionID.Equal("my-solution"),
    pageSize: pageSize
);

foreach (var page in pages)
{
    foreach (var entity in page)
    {
        // Process entity
        processedCount++;
    }
}

engine.GenerateInformation($"Processed {processedCount} entities");
```

**Why:** Pagination prevents memory exhaustion and keeps operations responsive even with thousands of entities.

### 3. Limit Traversal Depth Appropriately

Set traversal depth based on your actual needs. Excessive depth causes exponential query growth.

```csharp
// ✅ Good: Limit depth to what you need
var neighbors = manager.From(entity).Traverse(maxDepth: 1);  // Direct neighbors only
var nearby = manager.From(entity).Traverse(maxDepth: 2);     // 2 hops
var breadth = manager.From(entity).Traverse(maxDepth: 3);    // 3 hops (use cautiously)

// ❌ Avoid: Excessive depth on large graphs
var everything = manager.From(entity).Traverse(maxDepth: 100);  // May fetch thousands of entities!
```

**Why:** Each additional depth level exponentially increases the number of entities retrieved. Depth 5 on a highly connected graph can retrieve thousands of entities.

### 4. Filter Early, Not Late

Push filters to the DOM query layer instead of fetching everything and filtering in memory.

```csharp
// ✅ Good: Filter during traversal (pushed to DOM query)
var results = manager.From(entity)
    .Where(EntityDescriptorExposers.ModelName.Equal("Device"))
    .Where(EntityDescriptorExposers.Status.Equal(EntityStatus.Active))
    .Traverse(maxDepth: 3);

// ❌ Avoid: Fetching everything then filtering in memory
var results = manager.From(entity)
    .Traverse(maxDepth: 3)
    .Where(r => r.Entity.ModelName == "Device" && r.Entity.Status == EntityStatus.Active);
```

**Why:** Filters applied via `.Where(EntityDescriptorExposers...)` are pushed to the database query, reducing data transfer and memory usage.

### 5. Use GetNeighbors() for Direct Links

For simple neighbor queries, use the dedicated `GetNeighbors()` method instead of `Traverse()`.

```csharp
// ✅ Good: Simple and efficient
var neighbors = manager.From(entity).GetNeighbors();

// ❌ Avoid: Overkill for direct neighbors
var neighbors = manager.From(entity).Traverse(maxDepth: 1).Select(r => r.Entity);
```

**Why:** `GetNeighbors()` is optimized for the single-hop case and returns entities directly without additional traversal overhead.

### 6. Batch Related Operations

If you need multiple filtered views of the same traversal, do one traversal and post-process.

```csharp
// ✅ Good: Single traversal with post-processing
var results = manager.From(entity).Traverse(maxDepth: 3).ToList();
var tickets = results.Where(r => r.Entity.ModelName == "Ticket");
var assets = results.Where(r => r.Entity.ModelName == "NetworkDevice");

// ❌ Avoid: Multiple traversals for different filters
var tickets = manager.From(entity)
    .Where(EntityDescriptorExposers.ModelName.Equal("Ticket"))
    .Traverse(maxDepth: 3);
var assets = manager.From(entity)
    .Where(EntityDescriptorExposers.ModelName.Equal("NetworkDevice"))
    .Traverse(maxDepth: 3);
```

**Why:** Multiple traversals repeat the expensive graph walk. One traversal with in-memory filtering is faster when you need multiple result sets.

### 7. Cache Frequently Accessed Entities

If you repeatedly access the same entities, cache them to avoid redundant queries.

```csharp
// ✅ Good: Cache frequently accessed entities
var entityCache = new Dictionary<string, EntityDescriptor>();

foreach (var id in entityIds)
{
    if (!entityCache.ContainsKey(id))
    {
        var filter = EntityDescriptorExposers.ID.Equal(id);
        var entity = manager.Entities.Read(filter).FirstOrDefault();
        if (entity != null)
            entityCache[id] = entity;
    }
    
    // Use cached entity
    var cached = entityCache[id];
}
```

**Why:** Caching eliminates redundant DOM queries for entities you access multiple times.

## Querying & Traversal

### 1. Combine Multiple Where() Calls for AND Logic

Multiple `.Where()` calls are automatically combined with AND logic.

```csharp
// These are equivalent:
var filtered = manager.From(entity)
    .Where(EntityDescriptorExposers.ModelName.Equal("Device"))
    .Where(EntityDescriptorExposers.Status.Equal(EntityStatus.Active))
    .GetNeighbors();

// Same as:
var filtered = manager.From(entity)
    .Where(new ANDFilterElement<EntityDescriptor>(
        EntityDescriptorExposers.ModelName.Equal("Device"),
        EntityDescriptorExposers.Status.Equal(EntityStatus.Active)
    ))
    .GetNeighbors();
```

**Why:** Multiple `.Where()` calls are more readable and automatically combined into an efficient AND filter.

### 2. Use OR Filters for Multiple Values

When filtering by multiple possible values, use `ORFilterElement`.

```csharp
// ✅ Good: OR filter for multiple IDs
var ids = new[] { "id-1", "id-2", "id-3" };
var filters = ids.Select(id => EntityDescriptorExposers.ID.Equal(id)).ToArray();
var orFilter = new ORFilterElement<EntityDescriptor>(filters);

var entities = manager.Entities.Read(orFilter);

// ❌ Avoid: Multiple individual queries
var entities = new List<EntityDescriptor>();
foreach (var id in ids)
{
    var entity = manager.Entities.Read(
        EntityDescriptorExposers.ID.Equal(id)
    ).FirstOrDefault();
    if (entity != null)
        entities.Add(entity);
}
```

**Why:** OR filters retrieve all matching entities in a single query instead of N individual queries.

### 3. Understand Link vs Relate

Use `Link()` for bidirectional relationships and `Relate()` for directional dependencies.

```csharp
// ✅ Use Link for symmetric relationships
manager.Link(deviceA, deviceB);  // Creates: A→B and B→A

// ✅ Use Relate for directional dependencies
manager.Relate(application, database);  // Creates: Application→Database (one-way)
```

**Why:** Choosing the right method makes your intent clear and creates the appropriate relationship structure.

## Resource Management

### 1. Always Dispose with Using Statements

The `IRelationshipManager` implements `IDisposable` and must be properly disposed.

```csharp
// ✅ Good: using statement (preferred)
using (var manager = engine.GetRelationshipManager())
{
    // Use manager
}

// ✅ Good: Explicit try-finally
var manager = engine.GetRelationshipManager();
try
{
    // Use manager
}
finally
{
    manager.Dispose();
}

// ❌ Avoid: Not disposing
var manager = engine.GetRelationshipManager();
// Use manager without disposal - resource leak!
```

**Why:** Proper disposal ensures internal caches are cleared and resources are released.

## Error Handling

### 1. Handle Specific Exceptions

Catch specific exceptions for better error handling and diagnostics.

```csharp
try
{
    using (var manager = engine.GetRelationshipManager())
    {
        // Operations
    }
}
catch (KeyNotFoundException ex)
{
    // Entity not found during From(key) operation
    engine.GenerateInformation($"Entity not found: {ex.Message}");
}
catch (InvalidRelationEntityException ex)
{
    // Entity referenced by relation failed validation
    engine.GenerateInformation($"Invalid entity: {ex.EntityID}");
}
catch (ValidationException ex)
{
    // Validation error
    engine.GenerateInformation($"Validation failed: {ex.Message}");
}
catch (Exception ex)
{
    // Unexpected errors
    engine.ExitFail($"Error: {ex.Message}");
}
```

**Why:** Specific exception handling enables appropriate error responses and better diagnostics.

### 2. Validate Entity Descriptors Before Saving

Ensure required properties are set before creating or updating entities.

```csharp
// ✅ Good: Validate before saving
if (string.IsNullOrEmpty(entity.ID) || 
    string.IsNullOrEmpty(entity.ModelName) || 
    string.IsNullOrEmpty(entity.SolutionID))
{
    throw new ArgumentException("Required entity properties are missing");
}

manager.Entities.CreateOrUpdate(entity);
```

**Why:** Early validation prevents cryptic errors from the underlying DOM layer.

## Cross-Solution Integration

### 1. Use Relationships for Cross-Solution Linking Only

The Relationships library is designed for linking entities **across different solutions**. If entities must always exist together, they should be in the same solution.

```csharp
// ✅ Good: Cross-solution links
var ticket = new EntityDescriptor
{
    SolutionID = "ticketing-system",  // Different solution
    // ...
};

var asset = new EntityDescriptor
{
    SolutionID = "asset-management",  // Different solution
    // ...
};

manager.Link(ticket, asset);  // Link across solutions

// ❌ Avoid: Using relationships within a single solution for core dependencies
// If your solution can't function without both entities, use proper solution design instead
```

**Why:** Relationships add overhead. Use them for optional cross-solution integration, not for core solution dependencies.

### 2. Document Cross-Solution Contracts

When linking across solutions, document the entity structure both solutions expect.

```csharp
/// <summary>
/// Ticket entities from the ticketing system.
/// Expected structure:
/// - SolutionID: "ticketing-system"
/// - ModelName: "Ticket"
/// - ID format: "TICKET-YYYY-NNNNNN"
/// </summary>
public const string TICKETING_SOLUTION_ID = "ticketing-system";
public const string TICKET_MODEL_NAME = "Ticket";
```

**Why:** Documentation prevents integration issues when solutions evolve independently.

## Testing

### 1. Test with Representative Data Volumes

Test traversal and queries with realistic data volumes to catch performance issues early.

```csharp
// ✅ Good: Test with realistic scale
[TestMethod]
public void Test_TraversalPerformance_With1000Entities()
{
    // Create 1000 entities with realistic relationship density
    var entities = CreateTestEntities(1000);
    manager.Entities.CreateOrUpdate(entities);
    
    // Test traversal performance
    var stopwatch = Stopwatch.StartNew();
    var results = manager.From(entities[0]).Traverse(maxDepth: 3).ToList();
    stopwatch.Stop();
    
    Assert.IsTrue(stopwatch.ElapsedMilliseconds < 2000, "Traversal too slow");
}
```

**Why:** Performance issues often only appear at scale. Test with representative volumes early.

### 2. Clean Up Test Data

Always clean up test entities after test execution.

```csharp
[TestCleanup]
public void Cleanup()
{
    using (var manager = GetTestManager())
    {
        var testEntities = manager.Entities.Read(
            EntityDescriptorExposers.SolutionID.Equal("test-solution")
        );
        
        manager.Entities.Delete(testEntities);
    }
}
```

**Why:** Test data pollution can cause test failures and makes debugging difficult.

## See Also

- **[Migration Guide](./migration-guide.md)** - Migrating from Object Linking to Relationships
- [Getting Started](./getting-started.md) - Installation and setup
- [Creating Relationships](./creating-a-link-between-2-objects.md) - How to create links and relations
- [Querying Relationships](./retrieve-linked-objects.md) - Traversal and queries
- [Advanced Usage](./advanced-usage.md) - Advanced patterns and optimization
- [Examples](./examples.md) - Real-world scenarios
- [API Reference](./api-reference.md) - Complete API documentation
