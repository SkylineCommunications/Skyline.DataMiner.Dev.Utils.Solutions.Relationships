# Querying Relationships

This guide explains how to query and traverse relationships using the DataMiner Relationships library.

## Query Methods Overview

The Relationships library provides several ways to query relationships:

| Method | Use Case | Returns |
|--------|----------|---------|
| `GetNeighbors()` | Get directly connected entities (1 hop) | `IEnumerable<EntityDescriptor>` |
| `GetRelations()` | Get all relations for an entity | `IEnumerable<Relation>` |
| `Traverse()` | Explore the graph with depth control | `IEnumerable<TraversalResult>` |
| `Entities.Read()` | Low-level entity queries with filters | `IEnumerable<EntityDescriptor>` |
| `Relations.Read()` | Low-level relation queries with filters | `IEnumerable<Relation>` |

## Getting Direct Neighbors

The simplest way to get related entities is to use `GetNeighbors()`:

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

// Get all entities directly linked to the router (1 hop)
var neighbors = manager.From(router).GetNeighbors();

foreach (var neighbor in neighbors)
{
    engine.GenerateInformation($"Connected to: {neighbor.DisplayName} ({neighbor.ModelName})");
}
}
If you only have the entity's key components, you can use `EntityKey`:

```csharp
using Skyline.DataMiner.Solutions.Relationships;

var key = new EntityKey(
    solutionID: "network",
    modelName: "Router",
    id: "router-1"
);

var neighbors = manager.From(key).GetNeighbors();
```

> [!WARNING]
> Using `From(EntityKey)` throws `KeyNotFoundException` if the entity doesn't exist in storage.

## Graph Traversal

Traverse the relationship graph to explore multi-hop connections.

### Basic Traversal

```csharp
// Traverse up to 3 hops from the starting entity
var results = manager.From(router).Traverse(maxDepth: 3);

foreach (var result in results)
{
    engine.GenerateInformation($"Entity: {result.Entity.DisplayName}");
    engine.GenerateInformation($"  Depth: {result.Depth} hops away");
    engine.GenerateInformation($"  Reached via: {result.Via.DisplayName}");
    engine.GenerateInformation($"  Relations: {result.Relations.Count}");
}
```

### Traversal Result Properties

Each `TraversalResult` contains:

```csharp
public class TraversalResult
{
    // The entity discovered during traversal
    public EntityDescriptor Entity { get; }
    
    // Number of hops from the starting entity (1, 2, 3, ...)
    public int Depth { get; }
    
    // The entity from which this entity was reached
    public EntityDescriptor Via { get; }
    
    // All relations connecting Via to Entity
    // (Contains 2 entries for bidirectional links: Via→Entity and Entity→Via)
    public IReadOnlyList<Relation> Relations { get; }
}
```

### Traversal Behavior

- **Breadth-first search**: Explores all entities at depth N before moving to depth N+1
- **Bidirectional**: Follows relations in both directions (A→B is traversable from both A and B)
- **Deduplication**: Each entity is visited only once (prevents infinite loops)
- **Efficient batching**: Fetches all entities at each depth level in a single query

### Traversal Examples

```csharp
// Example 1: Find all elements within 2 hops
var elements = manager.From(myService)
    .Traverse(maxDepth: 2)
    .Where(r => r.Entity.ModelName == "Element")
    .Select(r => r.Entity);

// Example 2: Build a depth map
var depthMap = manager.From(myEntity)
    .Traverse(maxDepth: 5)
    .GroupBy(r => r.Depth)
    .ToDictionary(g => g.Key, g => g.Select(r => r.Entity).ToList());

engine.GenerateInformation($"Entities at depth 1: {depthMap[1].Count}");
engine.GenerateInformation($"Entities at depth 2: {depthMap[2].Count}");

// Example 3: Find the shortest path to a specific entity
var targetId = "target-entity-id";
var path = manager.From(start)
    .Traverse(maxDepth: 10)
    .FirstOrDefault(r => r.Entity.ID == targetId);

if (path != null)
{
    engine.GenerateInformation($"Found at depth {path.Depth}");
    engine.GenerateInformation($"Reached via {path.Via.DisplayName}");
}
```

## Filtering During Traversal

Use the `Where()` method to filter entities during traversal (applied at the DOM query level).

### Basic Filtering

```csharp
using Skyline.DataMiner.Net.Messages.SLDataGateway;

// Only traverse through active entities
var results = manager.From(router)
    .Where(EntityDescriptorExposers.Status.Equal(EntityStatus.Active))
    .Traverse(maxDepth: 3);
```

### Combining Filters

```csharp
// Multiple Where() calls are combined with AND
var results = manager.From(router)
    .Where(EntityDescriptorExposers.SolutionID.Equal("my-solution"))
    .Where(EntityDescriptorExposers.ModelName.Equal("Element"))
    .Traverse(maxDepth: 2);

// Equivalent to:
var filter = new ANDFilterElement<EntityDescriptor>(
    EntityDescriptorExposers.SolutionID.Equal("my-solution"),
    EntityDescriptorExposers.ModelName.Equal("Element")
);
var results = manager.From(router)
    .Where(filter)
    .Traverse(maxDepth: 2);
```

### Filter Before vs. After Traversal

```csharp
// Filter DURING traversal (more efficient, pushed to DOM query)
// Entities not matching the filter are not fetched or traversed further
var servicesOnly = manager.From(element)
    .Where(EntityDescriptorExposers.ModelName.Equal("Service"))
    .Traverse(maxDepth: 3);

// Filter AFTER traversal (LINQ filter on results)
// All entities are fetched, then filtered in memory
var servicesOnly = manager.From(element)
    .Traverse(maxDepth: 3)
    .Where(r => r.Entity.ModelName == "Service");

// Often you want both:
// - DOM filter for broad criteria (solution, status)
// - LINQ filter for computed properties or complex logic
var results = manager.From(element)
    .Where(EntityDescriptorExposers.SolutionID.Equal("my-solution"))  // DOM filter
    .Traverse(maxDepth: 3)
    .Where(r => r.Depth >= 2 && r.Entity.DisplayName.StartsWith("A"));  // LINQ filter
```

### Available Exposers

The library auto-generates exposer classes for filtering:

```csharp
// EntityDescriptor exposers
EntityDescriptorExposers.ID
EntityDescriptorExposers.DisplayName
EntityDescriptorExposers.ModelName
EntityDescriptorExposers.SolutionID
EntityDescriptorExposers.SolutionName
EntityDescriptorExposers.ParentID
EntityDescriptorExposers.ParentModelName
EntityDescriptorExposers.Status

// Relation exposers
RelationExposers.SourceKey
RelationExposers.TargetKey
RelationExposers.SourceState
RelationExposers.TargetState
```

## Querying Relations

Get the raw `Relation` objects for an entity:

```csharp
// Get all relations where the entity appears as source or target
var relations = manager.From(router).GetRelations();

foreach (var relation in relations)
{
    engine.GenerateInformation($"Relation: {relation.SourceKey} → {relation.TargetKey}");
    engine.GenerateInformation($"  Source Status: {relation.SourceState}");
    engine.GenerateInformation($"  Target Status: {relation.TargetState}");
}
```

### Understanding Relation Direction

```csharp
// For a Link (bidirectional):
manager.Link(entityA, entityB);
// Creates 2 relations:
//   Relation 1: SourceKey = entityA.Key, TargetKey = entityB.Key
//   Relation 2: SourceKey = entityB.Key, TargetKey = entityA.Key

// For a Relate (directed):
manager.Relate(source, target);
// Creates 1 relation:
//   Relation: SourceKey = source.Key, TargetKey = target.Key
```

## Low-Level Repository Queries

For advanced scenarios, query the repositories directly.

### Querying Entities Repository

```csharp
using Skyline.DataMiner.Net.Messages.SLDataGateway;

// Find all entities of a specific model
var filter = EntityDescriptorExposers.ModelName.Equal("Router");
var routers = manager.Entities.Read(filter);

// Complex filter: All active elements in a specific solution
var complexFilter = new ANDFilterElement<EntityDescriptor>(
    EntityDescriptorExposers.Status.Equal(EntityStatus.Active),
    EntityDescriptorExposers.ModelName.Equal("Element"),
    EntityDescriptorExposers.SolutionID.Equal("network-topology")
);
var elements = manager.Entities.Read(complexFilter);

// OR filter: Find entities by multiple IDs
var ids = new[] { "entity-1", "entity-2", "entity-3" };
var orFilters = ids.Select(id => EntityDescriptorExposers.ID.Equal(id)).ToArray();
var orFilter = new ORFilterElement<EntityDescriptor>(orFilters);
var entities = manager.Entities.Read(orFilter);
```

### Querying Relations Repository

```csharp
// Find all relations from a specific source
var sourceKey = new EntityKey("solution-id", "ModelName", "source-id");
var filter = RelationExposers.SourceKey.Equal(sourceKey);
var outgoingRelations = manager.Relations.Read(filter);

// Find all relations to a specific target
var targetKey = new EntityKey("solution-id", "ModelName", "target-id");
var incomingRelations = manager.Relations.Read(
    RelationExposers.TargetKey.Equal(targetKey)
);

// Find all relations between two entities (both directions)
var sourceKey = new EntityKey("solution", "Model", "entity-1");
var targetKey = new EntityKey("solution", "Model", "entity-2");

var bothDirections = new ORFilterElement<Relation>(
    new ANDFilterElement<Relation>(
        RelationExposers.SourceKey.Equal(sourceKey),
        RelationExposers.TargetKey.Equal(targetKey)
    ),
    new ANDFilterElement<Relation>(
        RelationExposers.SourceKey.Equal(targetKey),
        RelationExposers.TargetKey.Equal(sourceKey)
    )
);
var relations = manager.Relations.Read(bothDirections);
```

### Pagination

For large result sets, use pagination:

```csharp
// ReadPaged returns an enumerable of pages
var pageSize = 100;
var pages = manager.Entities.ReadPaged(
    filter: EntityDescriptorExposers.SolutionID.Equal("my-solution"),
    pageSize: pageSize
);

// Iterate through pages
foreach (var page in pages)
{
    // Each page is an IPagedResult<EntityDescriptor> containing the entities
    foreach (var entity in page)
    {
        // Process entity
    }
}

// Or process a specific number of pages
var processedPages = 0;
foreach (var page in pages)
{
    foreach (var entity in page)
    {
        // Process entity
    }
    
    processedPages++;
    if (processedPages >= 10) // Stop after 10 pages
        break;
}
```

## Common Query Patterns

### Pattern 1: Find All Tickets Related to an Asset

```csharp
var asset = new EntityDescriptor
{
    ID = "ASSET-SW-101",
    DisplayName = "Core Switch - Building A",
    ModelName = "NetworkDevice",
    SolutionID = "asset-management",
    SolutionName = "Asset Management"
};

var tickets = manager.From(asset)
    .Where(EntityDescriptorExposers.ModelName.Equal("Ticket"))
    .GetNeighbors();

engine.GenerateInformation($"Asset has {tickets.Count()} related tickets");
```

### Pattern 2: Find All Resources in a Booking

```csharp
var booking = new EntityDescriptor
{
    ID = "JOB #0005",
    DisplayName = bookingName,
    ModelName = "Job",
    SolutionID = "1b67a623-4ca6-4d25-8b3d-ed4e39496a75",
    SolutionName = "MediaOps Plan"
};

var resources = manager.From(booking)
    .Where(EntityDescriptorExposers.ModelName.Equal("Resource"))
    .GetNeighbors();
```

### Pattern 3: Find All Dependencies of a Component

```csharp
// Find all components this component depends on (directed relations)
var component = new EntityDescriptor { /* ... */ };

// Get all outgoing relations (component → others)
var componentKey = component.Key;
var dependencies = manager.Relations
    .Read(RelationExposers.SourceKey.Equal(componentKey))
    .Select(r => r.TargetKey)
    .ToList();

// Get the actual entity descriptors
var dependencyEntities = dependencies
    .Select(key =>
    {
        var filter = new ANDFilterElement<EntityDescriptor>(
            EntityDescriptorExposers.SolutionID.Equal(key.SolutionID),
            EntityDescriptorExposers.ModelName.Equal(key.ModelName),
            EntityDescriptorExposers.ID.Equal(key.ID)
        );
        return manager.Entities.Read(filter).FirstOrDefault();
    })
    .Where(e => e != null)
    .ToList();
```

### Pattern 4: Find All Entities in a Solution

```csharp
// This could be a large query depending on the solution
var allEntities = manager.Entities.Read(
    EntityDescriptorExposers.SolutionID.Equal("my-solution-id")
);

// Group by model
var byModel = allEntities
    .GroupBy(e => e.ModelName)
    .ToDictionary(g => g.Key, g => g.ToList());

foreach (var kvp in byModel)
{
    engine.GenerateInformation($"{kvp.Key}: {kvp.Value.Count} entities");
}
```

### Pattern 5: Check if Two Entities Are Connected

```csharp
bool AreConnected(IRelationshipManager manager, EntityDescriptor a, EntityDescriptor b, int maxDepth = 5)
{
    var reachable = manager.From(a)
        .Traverse(maxDepth)
        .Any(r => r.Entity.Key.Equals(b.Key));
    
    return reachable;
}

// Usage
if (AreConnected(manager, entityA, entityB))
{
    engine.GenerateInformation("Entities are connected!");
}
```

### Pattern 6: Build a Complete Subgraph

```csharp
// Get all entities and relations within a solution
var solutionId = "my-solution";

var entities = manager.Entities.Read(
    EntityDescriptorExposers.SolutionID.Equal(solutionId)
).ToList();

var entityKeys = entities.Select(e => e.Key).ToHashSet();

// Get all relations where the source is one of our entities
// Build OR filter for all entity keys
var sourceFilters = entityKeys
    .Select(key => RelationExposers.SourceKey.Equal(key))
    .ToArray();

var orFilter = new ORFilterElement<Relation>(sourceFilters);
var allRelations = manager.Relations.Read(orFilter).ToList();

// Build adjacency list
var graph = new Dictionary<EntityKey, List<EntityKey>>();
foreach (var relation in allRelations)
{
    if (!graph.ContainsKey(relation.SourceKey))
        graph[relation.SourceKey] = new List<EntityKey>();
    
    graph[relation.SourceKey].Add(relation.TargetKey);
}
```

## Performance Considerations

### 1. Use Appropriate Traversal Depth

```csharp
// Good: Limit depth to what you need
var neighbors = manager.From(entity).Traverse(maxDepth: 1);  // Direct neighbors only
var nearby = manager.From(entity).Traverse(maxDepth: 2);     // 2 hops

// Avoid: Excessive depth on large graphs
var everything = manager.From(entity).Traverse(maxDepth: 100);  // May fetch thousands of entities!
```

### 2. Filter Early

```csharp
// Good: Filter during traversal (pushed to DOM query)
var services = manager.From(element)
    .Where(EntityDescriptorExposers.ModelName.Equal("Service"))
    .Traverse(maxDepth: 3);

// Avoid: Fetching everything then filtering
var services = manager.From(element)
    .Traverse(maxDepth: 3)
    .Where(r => r.Entity.ModelName == "Service");
```

### 3. Use GetNeighbors() for Direct Links

```csharp
// Good: Simple and efficient
var neighbors = manager.From(entity).GetNeighbors();

// Avoid: Overkill for direct neighbors
var neighbors = manager.From(entity).Traverse(maxDepth: 1).Select(r => r.Entity);
```

### 4. Batch Related Operations

```csharp
// Good: Single traversal with post-processing
var results = manager.From(entity).Traverse(maxDepth: 3).ToList();
var services = results.Where(r => r.Entity.ModelName == "Service");
var elements = results.Where(r => r.Entity.ModelName == "Element");

// Avoid: Multiple traversals
var services = manager.From(entity)
    .Where(EntityDescriptorExposers.ModelName.Equal("Service"))
    .Traverse(maxDepth: 3);
var elements = manager.From(entity)
    .Where(EntityDescriptorExposers.ModelName.Equal("Element"))
    .Traverse(maxDepth: 3);
```

## Next Steps

- **[Advanced Usage](./advanced-usage.md)** - Entity management, status tracking, and optimization
- **[Examples](./examples.md)** - Complete real-world scenarios
- **[API Reference](./api-reference.md)** - Detailed API documentation

## See Also

- [Creating Relationships](./creating-relationships.md) - How to create links and relations
- [Getting Started](./getting-started.md) - Installation and basic setup
