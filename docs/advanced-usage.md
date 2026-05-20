# Advanced Usage

This guide covers advanced topics for working with the DataMiner Relationships library including filtering, entity management, performance optimization, and testing.

## Entity Management

### Entity CRUD Operations

The `IEntityDescriptorRepository` provides full CRUD (Create, Read, Update, Delete) operations:

```csharp
using (var manager = engine.GetRelationshipManager())
{
    // CREATE - Add a new entity
    var entity = new EntityDescriptor
    {
        ID = "entity-1",
        DisplayName = "Entity 1",
        ModelName = "Device",
        SolutionID = "solution-x",
        SolutionName = "Solution X"
    };

    manager.Entities.Create(entity);

    // READ - Query entities
    var filter = EntityDescriptorExposers.ID.Equal("entity-1");
    var retrieved = manager.Entities.Read(filter).FirstOrDefault();

    // UPDATE - Modify an entity
    if (retrieved != null)
    {
        retrieved.DisplayName = "Updated Entity 1";
        manager.Entities.Update(retrieved);
    }

    // DELETE - Remove an entity
    // Note: Deleting an entity also deletes all its relations
    manager.Entities.Delete(retrieved);
}
```

Use `CreateOrUpdate` for upsert operations:

```csharp
// Creates if not exists, updates if exists
var entity = new EntityDescriptor
{
    ID = "entity-1",
    DisplayName = "Entity 1",
    ModelName = "Device",
    SolutionID = "solution-x",
    SolutionName = "Solution X"
};

manager.Entities.CreateOrUpdate(entity);

// Batch upsert
var entities = new List<EntityDescriptor>
{
    new EntityDescriptor { /* ... */ },
    new EntityDescriptor { /* ... */ },
    new EntityDescriptor { /* ... */ }
};

manager.Entities.CreateOrUpdate(entities);
```

### Managing Entity Status

Entities can be marked as active or inactive using the `Status` property:

```csharp
// Mark entity as inactive
var entity = manager.Entities.Read(filter).FirstOrDefault();
if (entity != null)
{
    entity.Status = EntityStatus.Inactive;
    manager.Entities.Update(entity);
}

// Query only active entities
var activeEntities = manager.Entities.Read(
    new ANDFilterElement<EntityDescriptor>(
        EntityDescriptorExposers.SolutionID.Equal("my-solution"),
        EntityDescriptorExposers.Status.Equal(EntityStatus.Active)
    )
);

// Include all entities (both active and inactive)
var allEntities = manager.Entities.Read(
    EntityDescriptorExposers.SolutionID.Equal("my-solution")
);
```

### Entity Status Propagation

When an entity's status changes, related `Relation` objects are automatically updated:

```csharp
var entityA = new EntityDescriptor
{
    ID = "a",
    DisplayName = "Entity A",
    ModelName = "Device",
    SolutionID = "solution",
    SolutionName = "Solution",
    Status = EntityStatus.Active
};

var entityB = new EntityDescriptor
{
    ID = "b",
    DisplayName = "Entity B",
    ModelName = "Device",
    SolutionID = "solution",
    SolutionName = "Solution",
    Status = EntityStatus.Active
};

manager.Link(entityA, entityB);

// Later: Mark entityA as inactive
entityA.Status = EntityStatus.Inactive;
manager.Entities.Update(entityA);

// Relations are automatically updated with SourceState = Inactive or TargetState = Inactive
var relations = manager.Relations.Read(
    RelationExposers.SourceKey.Equal(entityA.Key)
);

foreach (var relation in relations)
{
    // relation.SourceState == EntityStatus.Inactive
    Console.WriteLine($"Source State: {relation.SourceState}");
}
```

### Relation CRUD Operations

```csharp
using var manager = engine.GetRelationshipManager();

// CREATE - Add relations
var entity1 = new EntityDescriptor { /* ... */ };
var entity2 = new EntityDescriptor { /* ... */ };

var relation = Relation.Between(entity1, entity2);
manager.Relations.Create(relation);

// Batch create
var relations = new List<Relation>
{
    Relation.Between(entity1, entity2),
    Relation.Between(entity2, entity3),
    Relation.Between(entity3, entity4)
};
manager.Relations.Create(relations);

// READ - Query relations
var sourceKey = new EntityKey("solution", "Model", "entity-1");
var outgoing = manager.Relations.Read(
    RelationExposers.SourceKey.Equal(sourceKey)
);

// DELETE - Remove relations
manager.Relations.Delete(outgoing.ToList());
```

### Preventing Orphaned Relations

The library includes middleware that validates relations:

```csharp
// This will throw an exception if entities don't exist:
var relation = Relation.Between(nonExistentEntity1, nonExistentEntity2);
manager.Relations.Create(relation);
// Throws: RelationEntityExistenceMiddleware prevents orphaned relations

// Correct approach: Ensure entities exist first
manager.Entities.CreateOrUpdate([entity1, entity2]);
manager.Relations.Create(Relation.Between(entity1, entity2));

// Or use the high-level Link/Relate methods (they handle this automatically)
manager.Link(entity1, entity2);
```

## Advanced Filtering

### Complex Filter Combinations

```csharp
using Skyline.DataMiner.Net.Messages.SLDataGateway;

// AND filter: All conditions must match
var andFilter = new ANDFilterElement<EntityDescriptor>(
    EntityDescriptorExposers.SolutionID.Equal("solution-x"),
    EntityDescriptorExposers.ModelName.Equal("Device"),
    EntityDescriptorExposers.Status.Equal(EntityStatus.Active)
);

var devices = manager.Entities.Read(andFilter);

// OR filter: Any condition must match
var orFilter = new ORFilterElement<EntityDescriptor>(
    EntityDescriptorExposers.ModelName.Equal("Router"),
    EntityDescriptorExposers.ModelName.Equal("Switch"),
    EntityDescriptorExposers.ModelName.Equal("Firewall")
);

var networkDevices = manager.Entities.Read(orFilter);

// Nested filters: (A AND B) OR (C AND D)
var nestedFilter = new ORFilterElement<EntityDescriptor>(
    new ANDFilterElement<EntityDescriptor>(
        EntityDescriptorExposers.SolutionID.Equal("solution-1"),
        EntityDescriptorExposers.ModelName.Equal("Device")
    ),
    new ANDFilterElement<EntityDescriptor>(
        EntityDescriptorExposers.SolutionID.Equal("solution-2"),
        EntityDescriptorExposers.ModelName.Equal("Service")
    )
);

var entities = manager.Entities.Read(nestedFilter);
```

### String Filters

```csharp
// Starts with
var filter = EntityDescriptorExposers.DisplayName.StartsWith("Router");

// Contains
var filter = EntityDescriptorExposers.DisplayName.Contains("Core");

// Equals (case-sensitive)
var filter = EntityDescriptorExposers.ID.Equal("element-123");

// Multiple values (OR)
var ids = new[] { "id-1", "id-2", "id-3" };
var filters = ids.Select(id => EntityDescriptorExposers.ID.Equal(id)).ToArray();
var orFilter = new ORFilterElement<EntityDescriptor>(filters);
```

### Filtering on EntityKey

The `EntityKey` is a composite identifier:

```csharp
public class EntityKey
{
    public string SolutionID { get; set; }
    public string ModelName { get; set; }
    public string ID { get; set; }
}

// Filter by complete key
var key = new EntityKey("solution-id", "Device", "device-123");

var filter = new ANDFilterElement<EntityDescriptor>(
    EntityDescriptorExposers.SolutionID.Equal(key.SolutionID),
    EntityDescriptorExposers.ModelName.Equal(key.ModelName),
    EntityDescriptorExposers.ID.Equal(key.ID)
);

var entity = manager.Entities.Read(filter).FirstOrDefault();

// Filter relations by key
var relations = manager.Relations.Read(
    RelationExposers.SourceKey.Equal(key)
);
```

## Performance Optimization

### 1. Batch Operations

Always use batch operations when working with multiple entities:

```csharp
// Good: Single batch operation
var entities = Enumerable.Range(1, 100).Select(i => new EntityDescriptor
{
    ID = $"entity-{i}",
    DisplayName = $"Entity {i}",
    ModelName = "Device",
    SolutionID = "solution",
    SolutionName = "Solution"
}).ToList();

manager.Entities.CreateOrUpdate(entities);

// Avoid: Individual operations in a loop
for (int i = 1; i <= 100; i++)
{
    var entity = new EntityDescriptor { /* ... */ };
    manager.Entities.CreateOrUpdate(entity);  // 100 individual DOM calls!
}
```

### 2. Traversal Depth Limits

```csharp
// Good: Reasonable depth for your use case
var neighbors = manager.From(entity).Traverse(maxDepth: 2);

// Avoid: Unbounded or excessive depth
var everything = manager.From(entity).Traverse(maxDepth: 1000);
```

### 3. Early Filtering

```csharp
// Good: Filter during traversal (pushed to DOM)
var results = manager.From(entity)
    .Where(EntityDescriptorExposers.ModelName.Equal("Device"))
    .Where(EntityDescriptorExposers.Status.Equal(EntityStatus.Active))
    .Traverse(maxDepth: 3);

// Less efficient: Fetch everything then filter
var results = manager.From(entity)
    .Traverse(maxDepth: 3)
    .Where(r => r.Entity.ModelName == "Device" && r.Entity.Status == EntityStatus.Active);
```

### 4. Pagination for Large Results

```csharp
// Process large result sets in pages
var pageSize = 100;
var processedCount = 0;

var pages = manager.Entities.ReadPaged(
    filter: EntityDescriptorExposers.SolutionID.Equal("my-solution"),
    pageSize: pageSize
);

foreach (var page in pages)
{
    // Process this page
    foreach (var entity in page)
    {
        // Process entity
        processedCount++;
    }
}

engine.GenerateInformation($"Processed {processedCount} entities");

// Alternative: Process with progress tracking
var pageNumber = 0;
foreach (var page in pages)
{
    engine.GenerateInformation($"Processing page {pageNumber}...");
    
    foreach (var entity in page)
    {
        // Process entity
    }
    
    pageNumber++;
}
```

### 5. Cache Frequently Accessed Entities

```csharp
// Cache entity descriptors you'll use multiple times
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
    
    var cachedEntity = entityCache[id];
    // Use cachedEntity
}
```

## Testing

### Unit Testing with Mocks

Each package provides an override mechanism for testing:

```csharp
using Moq;
using NUnit.Framework;
using Skyline.DataMiner.Solutions.Relationships;
using Skyline.DataMiner.Automation;

[TestFixture]
public class MyScriptTests
{
    [Test]
    public void TestMyScript()
    {
        // Arrange
        var mockEngine = new Mock<IEngine>();
        var mockManager = new Mock<IRelationshipManager>();
        
        // Configure the override
        EngineExtensions.OverrideGetRelationshipManager = (engine) => mockManager.Object;
        
        // Setup mock behavior
        mockManager.Setup(m => m.Entities.Read(It.IsAny<FilterElement<EntityDescriptor>>()))
            .Returns(new List<EntityDescriptor>
            {
                new EntityDescriptor
                {
                    ID = "test-entity",
                    DisplayName = "Test Entity",
                    ModelName = "Device",
                    SolutionID = "test",
                    SolutionName = "Test"
                }
            });
        
        // Act
        var manager = mockEngine.Object.GetRelationshipManager();
        var entities = manager.Entities.Read(EntityDescriptorExposers.SolutionID.Equal("test"));
        
        // Assert
        Assert.That(entities.Count(), Is.EqualTo(1));
        Assert.That(entities.First().ID, Is.EqualTo("test-entity"));
    }
    
    [TearDown]
    public void TearDown()
    {
        // Reset override after each test
        EngineExtensions.OverrideGetRelationshipManager = null;
    }
}
```

### Integration Testing

For integration tests, use a real DataMiner connection:

```csharp
[TestFixture]
public class RelationshipIntegrationTests
{
    private IConnection _connection;
    private IRelationshipManager _manager;
    
    [SetUp]
    public void Setup()
    {
        // Connect to a test DMA
        _connection = ConnectionSettings.GetConnection("localhost");
        _manager = _connection.GetRelationshipManager();
    }
    
    [Test]
    public void CanCreateAndQueryRelationship()
    {
        // Arrange
        var entity1 = new EntityDescriptor
        {
            ID = $"test-{Guid.NewGuid()}",
            DisplayName = "Test Entity 1",
            ModelName = "TestDevice",
            SolutionID = "integration-test",
            SolutionName = "Integration Test"
        };
        
        var entity2 = new EntityDescriptor
        {
            ID = $"test-{Guid.NewGuid()}",
            DisplayName = "Test Entity 2",
            ModelName = "TestDevice",
            SolutionID = "integration-test",
            SolutionName = "Integration Test"
        };
        
        try
        {
            // Act
            _manager.Link(entity1, entity2);
            
            var neighbors = _manager.From(entity1).GetNeighbors().ToList();
            
            // Assert
            Assert.That(neighbors, Has.Count.EqualTo(1));
            Assert.That(neighbors[0].ID, Is.EqualTo(entity2.ID));
        }
        finally
        {
            // Cleanup
            _manager.Unlink(entity1, entity2);
            _manager.Entities.Delete([entity1, entity2]);
        }
    }
    
    [TearDown]
    public void TearDown()
    {
        _manager?.Dispose();
    }
}
```

## Error Handling

### Common Exceptions

```csharp
using (var manager = engine.GetRelationshipManager())
{
    try
    {
        // KeyNotFoundException: Entity doesn't exist
        var key = new EntityKey("solution", "Model", "non-existent");
        var neighbors = manager.From(key).GetNeighbors();
    }
    catch (KeyNotFoundException ex)
    {
        engine.GenerateInformation($"Entity not found: {ex.Message}");
    }

    try
    {
        // Validation exceptions from middleware
        var entity = new EntityDescriptor
        {
            // Missing required properties
            ID = "entity-1"
            // Missing DisplayName, ModelName, SolutionID, SolutionName
        };
        
        manager.Entities.Create(entity);
    }
    catch (Exception ex)
    {
        engine.GenerateInformation($"Validation error: {ex.Message}");
    }

    try
    {
        // Orphaned relation error
        var orphanedRelation = Relation.Between(
            new EntityDescriptor { /* ... */ },
            new EntityDescriptor { /* ... */ }
        );
        
        manager.Relations.Create(orphanedRelation);
    }
    catch (Exception ex)
    {
        engine.GenerateInformation($"Entities must exist before creating relations: {ex.Message}");
    }
}
```

### Defensive Programming

```csharp
// Check if entity exists before querying
bool EntityExists(IRelationshipManager manager, EntityKey key)
{
    var filter = new ANDFilterElement<EntityDescriptor>(
        EntityDescriptorExposers.SolutionID.Equal(key.SolutionID),
        EntityDescriptorExposers.ModelName.Equal(key.ModelName),
        EntityDescriptorExposers.ID.Equal(key.ID)
    );
    
    return manager.Entities.Count(filter) > 0;
}

// Safe traversal
var key = new EntityKey("solution", "Model", "entity-id");

using (var manager = engine.GetRelationshipManager())
{
    if (EntityExists(manager, key))
    {
        var neighbors = manager.From(key).GetNeighbors();
    }
    else
    {
        engine.GenerateInformation("Entity does not exist");
    }
}
```

## Working with Parent Relationships

Use `ParentID` and `ParentModelName` for hierarchical context:

```csharp
// Create a hierarchy: Site → Rack → Device
var site = new EntityDescriptor
{
    ID = "site-ny",
    DisplayName = "New York Site",
    ModelName = "Site",
    SolutionID = "infrastructure",
    SolutionName = "Infrastructure"
};

var rack = new EntityDescriptor
{
    ID = "rack-a01",
    DisplayName = "Rack A01",
    ModelName = "Rack",
    SolutionID = "infrastructure",
    SolutionName = "Infrastructure",
    ParentID = site.ID,
    ParentModelName = site.ModelName
};

var device = new EntityDescriptor
{
    ID = "device-001",
    DisplayName = "Router 001",
    ModelName = "Device",
    SolutionID = "infrastructure",
    SolutionName = "Infrastructure",
    ParentID = rack.ID,
    ParentModelName = rack.ModelName
};

// Store entities
manager.Entities.CreateOrUpdate([site, rack, device]);

// Query by parent
var devicesInRack = manager.Entities.Read(
    new ANDFilterElement<EntityDescriptor>(
        EntityDescriptorExposers.ParentID.Equal(rack.ID),
        EntityDescriptorExposers.ParentModelName.Equal(rack.ModelName)
    )
);
```

## Custom Helper Extensions

Create your own extension methods for common patterns:

```csharp
public static class MyRelationshipExtensions
{
    /// <summary>
    /// Gets all tickets related to a specific asset.
    /// </summary>
    public static IEnumerable<EntityDescriptor> GetRelatedTickets(
        this IRelationshipManager manager,
        string assetId)
    {
        var asset = new EntityDescriptor
        {
            ID = assetId,
            DisplayName = "Asset",  // Can be any value for query
            ModelName = "NetworkDevice",
            SolutionID = "asset-management",
            SolutionName = "Asset Management"
        };
        
        return manager.From(asset)
            .Where(EntityDescriptorExposers.ModelName.Equal("Ticket"))
            .GetNeighbors();
    }
    
    /// <summary>
    /// Checks if two entities are connected within a given depth.
    /// </summary>
    public static bool AreConnected(
        this IRelationshipManager manager,
        EntityDescriptor a,
        EntityDescriptor b,
        int maxDepth = 5)
    {
        return manager.From(a)
            .Traverse(maxDepth)
            .Any(r => r.Entity.Key.Equals(b.Key));
    }
    
    /// <summary>
    /// Gets all entities of a specific model in a solution.
    /// </summary>
    public static IEnumerable<EntityDescriptor> GetEntitiesByModel(
        this IRelationshipManager manager,
        string solutionId,
        string modelName)
    {
        var filter = new ANDFilterElement<EntityDescriptor>(
            EntityDescriptorExposers.SolutionID.Equal(solutionId),
            EntityDescriptorExposers.ModelName.Equal(modelName),
            EntityDescriptorExposers.Status.Equal(EntityStatus.Active)
        );
        
        return manager.Entities.Read(filter);
    }
}

// Usage
using (var manager = engine.GetRelationshipManager())
{
    var tickets = manager.GetRelatedTickets("ASSET-SW-101");
    var allDevices = manager.GetEntitiesByModel("infrastructure", "Device");
}
```

## Next Steps

- **[API Reference](./api-reference.md)** - Complete API documentation
- **[Examples](./examples.md)** - Real-world usage scenarios
- **[Creating Relationships](./creating-a-link-between-2-objects.md)** - How to create links and relations

## See Also

- **[Best Practices](./best-practices.md)** - Comprehensive guidelines for effective use
- [Querying Relationships](./retrieve-linked-objects.md) - Traversal and query patterns
- [Getting Started](./getting-started.md) - Installation and basic setup
- [API Reference](./api-reference.md) - Complete API documentation
