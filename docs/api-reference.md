# API Reference

Complete API reference for the DataMiner Relationships library.

## Namespaces

```csharp
using Skyline.DataMiner.Solutions.Relationships;
using Skyline.DataMiner.Solutions.Relationships.Repositories;
using Skyline.DataMiner.Solutions.Relationships.Query;
using Skyline.DataMiner.Net.Messages.SLDataGateway;  // For filters
```

## Core Interfaces

### IRelationshipManager

The main entry point for the Relationships library.

```csharp
public interface IRelationshipManager : IDisposable
{
    IRelationRepository Relations { get; }
    IEntityDescriptorRepository Entities { get; }
}
```

**Properties:**
- `Relations`: Repository for managing `Relation` objects
- `Entities`: Repository for managing `EntityDescriptor` objects

**Usage:**
```csharp
using (var manager = engine.GetRelationshipManager()) { }  // Automation
using (var manager = protocol.GetRelationshipManager()) { }  // Protocol
using (var manager = args.DMS.GetRelationshipManager()) { }  // GQI
using (var manager = connection.GetRelationshipManager()) { }  // Common
```

### IEntityDescriptorRepository

Repository for managing entity descriptors.

```csharp
public interface IEntityDescriptorRepository : IBulkRepository<EntityDescriptor>
{
    // Inherited from IBulkRepository:
    void Create(EntityDescriptor entity);
    void Create(IEnumerable<EntityDescriptor> entities);
    IEnumerable<EntityDescriptor> Read(FilterElement<EntityDescriptor> filter);
    IEnumerable<IPagedResult<EntityDescriptor>> ReadPaged(FilterElement<EntityDescriptor> filter);
    IEnumerable<IPagedResult<EntityDescriptor>> ReadPaged(FilterElement<EntityDescriptor> filter, int pageSize);
    void Update(EntityDescriptor entity);
    void Update(IEnumerable<EntityDescriptor> entities);
    void Delete(EntityDescriptor entity);
    void Delete(IEnumerable<EntityDescriptor> entities);
    void CreateOrUpdate(EntityDescriptor entity);
    void CreateOrUpdate(IEnumerable<EntityDescriptor> entities);
}
```

**Methods:**

| Method | Description |
|--------|-------------|
| `Create(entity)` | Create a single entity |
| `Create(entities)` | Create multiple entities in a batch |
| `Read(filter)` | Query entities with a filter |
| `ReadPaged(filter)` | Query entities with pagination (default page size: 500) |
| `ReadPaged(filter, pageSize)` | Query entities with pagination and custom page size |
| `Update(entity)` | Update a single entity |
| `Update(entities)` | Update multiple entities in a batch |
| `Delete(entity)` | Delete a single entity (also deletes all its relations) |
| `Delete(entities)` | Delete multiple entities in a batch |
| `CreateOrUpdate(entity)` | Upsert a single entity |
| `CreateOrUpdate(entities)` | Upsert multiple entities in a batch |

**Pagination Details:**

The `ReadPaged` methods return `IEnumerable<IPagedResult<EntityDescriptor>>` where each `IPagedResult` represents one page of results. You iterate through the pages:

```csharp
var pages = manager.Entities.ReadPaged(filter, pageSize: 100);

foreach (var page in pages)
{
    // Each 'page' is an IPagedResult<EntityDescriptor> that implements IEnumerable
    foreach (var entity in page)
    {
        // Process entity
    }
}
```

> [!TIP]
> The default page size is 500. Adjust based on your performance requirements.

### IRelationRepository

Repository for managing relations.

```csharp
public interface IRelationRepository :
    IReadableRepository<Relation>,
    IPageableRepository<Relation>,
    IBulkCreatableRepository<Relation>,
    IBulkDeletableRepository<Relation>
{
    // Inherited methods:
    void Create(Relation relation);
    void Create(IEnumerable<Relation> relations);
    IEnumerable<Relation> Read(FilterElement<Relation> filter);
    IEnumerable<IPagedResult<Relation>> ReadPaged(FilterElement<Relation> filter);
    IEnumerable<IPagedResult<Relation>> ReadPaged(FilterElement<Relation> filter, int pageSize);
    void Delete(Relation relation);
    void Delete(IEnumerable<Relation> relations);
}
```

**Methods:**

| Method | Description |
|--------|-------------|
| `Create(relation)` | Create a single relation |
| `Create(relations)` | Create multiple relations in a batch |
| `Read(filter)` | Query relations with a filter |
| `ReadPaged(filter)` | Query relations with pagination (default page size: 500) |
| `ReadPaged(filter, pageSize)` | Query relations with pagination and custom page size |
| `Delete(relation)` | Delete a single relation |
| `Delete(relations)` | Delete multiple relations in a batch |

**Pagination Details:**

The `ReadPaged` methods return `IEnumerable<IPagedResult<Relation>>`:

```csharp
var pages = manager.Relations.ReadPaged(filter, pageSize: 100);

foreach (var page in pages)
{
    // Each 'page' contains up to 100 relations
    foreach (var relation in page)
    {
        // Process relation
    }
}
```

## Extension Methods

### RelationshipManagerExtensions

High-level API for common operations.

```csharp
public static class RelationshipManagerExtensions
{
    // Traversal
    public static EntityTraversal From(this IRelationshipManager manager, EntityDescriptor entity);
    public static EntityTraversal From(this IRelationshipManager manager, EntityKey key);
    
    // Create relationships
    public static void Link(this IRelationshipManager manager, EntityDescriptor a, EntityDescriptor b);
    public static void Relate(this IRelationshipManager manager, EntityDescriptor source, EntityDescriptor target);
    
    // Remove relationships
    public static void Unlink(this IRelationshipManager manager, EntityDescriptor a, EntityDescriptor b);
    public static void Unrelate(this IRelationshipManager manager, EntityDescriptor source, EntityDescriptor target);
}
```

#### Link

Creates a bidirectional (symmetric) link between two entities.

```csharp
public static void Link(this IRelationshipManager manager, EntityDescriptor a, EntityDescriptor b)
```

**Parameters:**
- `manager`: The relationship manager instance
- `a`: First entity
- `b`: Second entity

**Behavior:**
- Creates or updates both entities
- Creates two relations: `a → b` and `b → a`

**Example:**
```csharp
manager.Link(router, switch);
```

#### Relate

Creates a single directed relation from source to target.

```csharp
public static void Relate(this IRelationshipManager manager, EntityDescriptor source, EntityDescriptor target)
```

**Parameters:**
- `manager`: The relationship manager instance
- `source`: Source entity
- `target`: Target entity

**Behavior:**
- Creates or updates both entities
- Creates one relation: `source → target`

**Example:**
```csharp
manager.Relate(application, database);
```

#### Unlink

Removes all relations between two entities in both directions.

```csharp
public static void Unlink(this IRelationshipManager manager, EntityDescriptor a, EntityDescriptor b)
```

**Parameters:**
- `manager`: The relationship manager instance
- `a`: First entity
- `b`: Second entity

**Behavior:**
- Removes `a → b` relation (if exists)
- Removes `b → a` relation (if exists)
- Does not delete the entities themselves

**Example:**
```csharp
manager.Unlink(router, switch);
```

#### Unrelate

Removes a single directed relation.

```csharp
public static void Unrelate(this IRelationshipManager manager, EntityDescriptor source, EntityDescriptor target)
```

**Parameters:**
- `manager`: The relationship manager instance
- `source`: Source entity
- `target`: Target entity

**Behavior:**
- Removes only `source → target` relation
- Does not affect `target → source` (if exists)
- Does not delete the entities themselves

**Example:**
```csharp
manager.Unrelate(application, database);
```

#### From (EntityDescriptor)

Starts a traversal from an entity descriptor.

```csharp
public static EntityTraversal From(this IRelationshipManager manager, EntityDescriptor entity)
```

**Parameters:**
- `manager`: The relationship manager instance
- `entity`: Starting entity

**Returns:** `EntityTraversal` instance for chaining

**Example:**
```csharp
var neighbors = manager.From(myEntity).GetNeighbors();
```

#### From (EntityKey)

Starts a traversal from an entity key.

```csharp
public static EntityTraversal From(this IRelationshipManager manager, EntityKey key)
```

**Parameters:**
- `manager`: The relationship manager instance
- `key`: Key of the starting entity

**Returns:** `EntityTraversal` instance for chaining

**Throws:** `KeyNotFoundException` if entity doesn't exist

**Example:**
```csharp
var key = new EntityKey("solution", "Model", "id");
var neighbors = manager.From(key).GetNeighbors();
```

### Package-Specific Extensions

#### EngineExtensions (Automation)

```csharp
public static IRelationshipManager GetRelationshipManager(this IEngine engine)
```

**Example:**
```csharp
using Skyline.DataMiner.Solutions.Relationships;

public void Run(IEngine engine)
{
    using (var manager = engine.GetRelationshipManager())
    {
        // Use manager...
    }
}
```

#### ProtocolExtensions (Connector)

```csharp
public static IRelationshipManager GetRelationshipManager(this SLProtocol protocol)
```

**Example:**
```csharp
using Skyline.DataMiner.Solutions.ObjectLinking;  // Different namespace!

public static void Run(SLProtocol protocol)
{
    using (var manager = protocol.GetRelationshipManager())
    {
        // Use manager...
    }
}
```

> [!NOTE]
> Protocol extensions are in the `Skyline.DataMiner.Solutions.ObjectLinking` namespace.

#### GqiExtensions (GQI)

```csharp
public static IRelationshipManager GetRelationshipManager(this GQIDMS dms)
public static IRelationshipManager GetRelationshipManager(this OnInitInputArgs args)
```

**Example:**
```csharp
using Skyline.DataMiner.Solutions.Relationships;

public OnInitOutputArgs OnInit(OnInitInputArgs args)
{
    using (var manager = args.GetRelationshipManager())
    {
        // Use manager...
    }
    // Return statement would be here
}
```

#### ConnectionExtensions (Common)

```csharp
public static IRelationshipManager GetRelationshipManager(this IConnection connection)
```

**Example:**
```csharp
using Skyline.DataMiner.Solutions.Relationships;

var connection = ConnectionSettings.GetConnection("localhost");
using (var manager = connection.GetRelationshipManager())
{
    // Use manager
}
```

## Classes

### EntityDescriptor

Represents an entity in the relationship graph.

```csharp
public sealed class EntityDescriptor : SdmObject<EntityDescriptor>
{
    public override string Identifier { get; set; }
    public EntityKey Key { get; }
    public string ID { get; set; }
    public string DisplayName { get; set; }
    public string ModelName { get; set; }
    public string SolutionID { get; set; }
    public string SolutionName { get; set; }
    public string ParentID { get; set; }
    public string ParentModelName { get; set; }
    public EntityStatus Status { get; set; }
}
```

**Properties:**

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Identifier` | `string` | Auto | Internal DOM identifier (auto-generated) |
| `Key` | `EntityKey` | Read-only | Composite key: `(SolutionID, ModelName, ID)` |
| `ID` | `string` | Yes | Unique identifier within the solution and model |
| `DisplayName` | `string` | Yes | Human-readable name |
| `ModelName` | `string` | Yes | Type/category of entity |
| `SolutionID` | `string` | Yes | Unique solution identifier |
| `SolutionName` | `string` | Yes | Display name of the solution |
| `ParentID` | `string` | No | ID of parent entity (for hierarchy) |
| `ParentModelName` | `string` | No | Model name of parent entity |
| `Status` | `EntityStatus` | Auto | Lifecycle status (Active or Inactive) |

**Example:**
```csharp
var entity = new EntityDescriptor
{
    ID = "element-123",
    DisplayName = "Main Router",
    ModelName = "Element",
    SolutionID = "network-topology",
    SolutionName = "Network Topology",
    ParentID = "site-1",
    ParentModelName = "Site",
    Status = EntityStatus.Active
};
```

### Relation

Represents a directed relation from a source entity to a target entity.

```csharp
public sealed class Relation : SdmObject<Relation>
{
    public override string Identifier { get; set; }
    public EntityKey SourceKey { get; internal set; }
    public EntityStatus SourceState { get; internal set; }
    public EntityKey TargetKey { get; internal set; }
    public EntityStatus TargetState { get; internal set; }
    
    public static Relation Between(EntityDescriptor source, EntityDescriptor target);
}
```

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `Identifier` | `string` | Internal DOM identifier (auto-generated) |
| `SourceKey` | `EntityKey` | Key of the source entity |
| `SourceState` | `EntityStatus` | Status of the source entity |
| `TargetKey` | `EntityKey` | Key of the target entity |
| `TargetState` | `EntityStatus` | Status of the target entity |

**Methods:**

#### Between

Creates a relation from source to target.

```csharp
public static Relation Between(EntityDescriptor source, EntityDescriptor target)
```

**Parameters:**
- `source`: Source entity
- `target`: Target entity

**Returns:** New `Relation` instance

**Example:**
```csharp
var relation = Relation.Between(entityA, entityB);
manager.Relations.Create(relation);
```

### EntityKey

Composite key identifying an entity.

```csharp
public class EntityKey : IEquatable<EntityKey>
{
    public static readonly EntityKey Empty { get; }
    
    public string SolutionID { get; }
    public string ModelName { get; }
    public string ID { get; }
    
    public EntityKey(string solutionID, string modelName, string id);
    public static EntityKey Parse(string key);
    
    public bool Equals(EntityKey other);
    public override bool Equals(object obj);
    public override int GetHashCode();
    public override string ToString();
    
    public static bool operator ==(EntityKey left, EntityKey right);
    public static bool operator !=(EntityKey left, EntityKey right);
}
```

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `Empty` | `EntityKey` (static) | Empty key instance with all fields set to empty strings |
| `SolutionID` | `string` | Solution identifier (read-only) |
| `ModelName` | `string` | Model/type name (read-only) |
| `ID` | `string` | Entity identifier (read-only) |

**Constructor:**

```csharp
public EntityKey(string solutionID, string modelName, string id)
```

Creates a new entity key with the specified components.

**Static Methods:**

#### Parse

Parses a string representation of an entity key.

```csharp
public static EntityKey Parse(string key)
```

**Parameters:**
- `key`: String in the format `[SolutionID] ModelName (ID: ID)`

**Returns:** Parsed `EntityKey`

**Throws:**
- `ArgumentException`: If key is null or whitespace
- `FormatException`: If key format is invalid

**Example:**
```csharp
var key = EntityKey.Parse("[network-topology] Router (ID: router-1)");
```

**Instance Methods:**

#### ToString

Returns the string representation of the key.

```csharp
public override string ToString()
```

**Returns:** String in the format `[SolutionID] ModelName (ID: ID)`

**Example:**
```csharp
var key = new EntityKey("network-topology", "Router", "router-1");
Console.WriteLine(key.ToString());  // [network-topology] Router (ID: router-1)
```

**Usage Example:**
```csharp
var key = new EntityKey("network-topology", "Router", "router-1");
var neighbors = manager.From(key).GetNeighbors();

// Parse from string
var parsedKey = EntityKey.Parse("[network-topology] Router (ID: router-1)");

// Equality comparison
if (key == parsedKey)
{
    Console.WriteLine("Keys are equal");
}
```

### EntityTraversal

Fluent API for graph traversal.

```csharp
public sealed class EntityTraversal
{
    public EntityTraversal Where(FilterElement<EntityDescriptor> filter);
    public IEnumerable<EntityDescriptor> GetNeighbors();
    public IEnumerable<Relation> GetRelations();
    public IEnumerable<TraversalResult> Traverse(int maxDepth = 1);
}
```

**Methods:**

#### Where

Filters entities during traversal (pushed to DOM query).

```csharp
public EntityTraversal Where(FilterElement<EntityDescriptor> filter)
```

**Parameters:**
- `filter`: DOM filter to apply

**Returns:** Same `EntityTraversal` instance for chaining

**Example:**
```csharp
var results = manager.From(entity)
    .Where(EntityDescriptorExposers.ModelName.Equal("Service"))
    .Where(EntityDescriptorExposers.Status.Equal(EntityStatus.Active))
    .Traverse(maxDepth: 3);
```

#### GetNeighbors

Returns entities directly connected to the starting entity (1 hop).

```csharp
public IEnumerable<EntityDescriptor> GetNeighbors()
```

**Returns:** Collection of neighboring entities

**Example:**
```csharp
var neighbors = manager.From(router).GetNeighbors();
```

#### GetRelations

Returns all relations where the starting entity appears as source or target.

```csharp
public IEnumerable<Relation> GetRelations()
```

**Returns:** Collection of relations

**Example:**
```csharp
var relations = manager.From(router).GetRelations();
```

#### Traverse

Performs breadth-first graph traversal.

```csharp
public IEnumerable<TraversalResult> Traverse(int maxDepth = 1)
```

**Parameters:**
- `maxDepth`: Maximum number of hops (default: 1)

**Returns:** Collection of `TraversalResult` objects

**Behavior:**
- Breadth-first search
- Bidirectional (follows edges in both directions)
- Deduplicates entities (each visited once)
- Batches queries per depth level

**Example:**
```csharp
var results = manager.From(entity).Traverse(maxDepth: 3);

foreach (var result in results)
{
    Console.WriteLine($"{result.Entity.DisplayName} at depth {result.Depth}");
}
```

### TraversalResult

Represents an entity discovered during traversal.

```csharp
public sealed class TraversalResult
{
    public EntityDescriptor Entity { get; }
    public int Depth { get; }
    public EntityDescriptor Via { get; }
    public IReadOnlyList<Relation> Relations { get; }
}
```

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `Entity` | `EntityDescriptor` | The discovered entity |
| `Depth` | `int` | Number of hops from start (1, 2, 3, ...) |
| `Via` | `EntityDescriptor` | The entity from which this was reached |
| `Relations` | `IReadOnlyList<Relation>` | Relations connecting Via to Entity (2 for bidirectional links) |

**Example:**
```csharp
var results = manager.From(start).Traverse(maxDepth: 3);

foreach (var result in results)
{
    Console.WriteLine($"Entity: {result.Entity.DisplayName}");
    Console.WriteLine($"  Depth: {result.Depth}");
    Console.WriteLine($"  Via: {result.Via.DisplayName}");
    Console.WriteLine($"  Relations: {result.Relations.Count}");
}
```

## Enums

### EntityStatus

Lifecycle status of an entity.

```csharp
public enum EntityStatus
{
    Active = 0,
    Inactive = 1
}
```

**Values:**
- `Active`: Entity is active and operational
- `Inactive`: Entity has been marked as inactive

**Usage:**
```csharp
entity.Status = EntityStatus.Inactive;
manager.Entities.Update(entity);

// Query only active entities
var active = manager.Entities.Read(
    EntityDescriptorExposers.Status.Equal(EntityStatus.Active)
);
```

## Exposers (Auto-Generated)

### EntityDescriptorExposers

Filter builders for `EntityDescriptor` properties.

```csharp
public static class EntityDescriptorExposers
{
    public static EntityKeyExposer<EntityDescriptor> Key { get; }
    public static FilterElement<EntityDescriptor> Identifier { get; }
    public static FilterElement<EntityDescriptor> ID { get; }
    public static FilterElement<EntityDescriptor> DisplayName { get; }
    public static FilterElement<EntityDescriptor> ModelName { get; }
    public static FilterElement<EntityDescriptor> SolutionID { get; }
    public static FilterElement<EntityDescriptor> SolutionName { get; }
    public static FilterElement<EntityDescriptor> ParentID { get; }
    public static FilterElement<EntityDescriptor> ParentModelName { get; }
    public static FilterElement<EntityDescriptor> Status { get; }
}
```

**Exposers:**

| Exposer | Type | Description |
|---------|------|-------------|
| `Key` | `EntityKeyExposer<EntityDescriptor>` | Composite key exposer (supports `Equal` and `NotEqual` for whole-key filtering) |
| `Identifier` | `FilterElement<EntityDescriptor>` | Internal DOM identifier (GUID, auto-generated) |
| `ID` | `FilterElement<EntityDescriptor>` | Entity ID |
| `DisplayName` | `FilterElement<EntityDescriptor>` | Display name |
| `ModelName` | `FilterElement<EntityDescriptor>` | Model/type name |
| `SolutionID` | `FilterElement<EntityDescriptor>` | Solution identifier |
| `SolutionName` | `FilterElement<EntityDescriptor>` | Solution display name |
| `ParentID` | `FilterElement<EntityDescriptor>` | Parent entity ID |
| `ParentModelName` | `FilterElement<EntityDescriptor>` | Parent model name |
| `Status` | `FilterElement<EntityDescriptor>` | Entity status |

**Example:**
```csharp
var filter = EntityDescriptorExposers.ModelName.Equal("Router");
var routers = manager.Entities.Read(filter);

// Filter by complete key
var key = new EntityKey("solution", "Model", "id");
var entity = manager.Entities.Read(
    EntityDescriptorExposers.Key.Equal(key)
).FirstOrDefault();
```

### RelationExposers

Filter builders for `Relation` properties.

```csharp
public static class RelationExposers
{
    public static FilterElement<Relation> Identifier { get; }
    public static EntityKeyExposer<Relation> SourceKey { get; }
    public static EntityKeyExposer<Relation> TargetKey { get; }
    public static FilterElement<Relation> SourceState { get; }
    public static FilterElement<Relation> TargetState { get; }
}
```

**Exposers:**

| Exposer | Type | Description |
|---------|------|-------------|
| `Identifier` | `FilterElement<Relation>` | Internal DOM identifier (GUID, auto-generated) |
| `SourceKey` | `EntityKeyExposer<Relation>` | Composite key of the source entity |
| `TargetKey` | `EntityKeyExposer<Relation>` | Composite key of the target entity |
| `SourceState` | `FilterElement<Relation>` | Status of the source entity |
| `TargetState` | `FilterElement<Relation>` | Status of the target entity |

**Example:**
```csharp
var key = new EntityKey("solution", "Model", "id");
var outgoing = manager.Relations.Read(
    RelationExposers.SourceKey.Equal(key)
);
```

## Filter Operators

### Comparison Operators

```csharp
// Equality
filter.Equal(value)

// String operations
filter.StartsWith(value)
filter.Contains(value)
filter.EndsWith(value)

// Note: Other operators like GreaterThan, LessThan depend on property type
```

### Logical Operators

```csharp
using Skyline.DataMiner.Net.Messages.SLDataGateway;

// AND: All conditions must match
var andFilter = new ANDFilterElement<EntityDescriptor>(
    EntityDescriptorExposers.SolutionID.Equal("solution"),
    EntityDescriptorExposers.ModelName.Equal("Device"),
    EntityDescriptorExposers.Status.Equal(EntityStatus.Active)
);

// OR: Any condition must match
var orFilter = new ORFilterElement<EntityDescriptor>(
    EntityDescriptorExposers.ModelName.Equal("Router"),
    EntityDescriptorExposers.ModelName.Equal("Switch")
);

// Nested: Complex combinations
var nested = new ORFilterElement<EntityDescriptor>(
    new ANDFilterElement<EntityDescriptor>(
        EntityDescriptorExposers.SolutionID.Equal("solution-1"),
        EntityDescriptorExposers.Status.Equal(EntityStatus.Active)
    ),
    new ANDFilterElement<EntityDescriptor>(
        EntityDescriptorExposers.SolutionID.Equal("solution-2"),
        EntityDescriptorExposers.Status.Equal(EntityStatus.Active)
    )
);
```

## Exceptions

### ValidationException

Base exception for validation errors.

```csharp
public class ValidationException : Exception
{
    public ValidationException();
    public ValidationException(string message);
    public ValidationException(string message, Exception innerException);
}
```

**Namespace:** `Skyline.DataMiner.Solutions.Relationships.Exceptions`

**Description:** Represents errors that occur during validation operations in the Relationships library.

### InvalidRelationException

Exception thrown when a relation fails validation.

```csharp
public class InvalidRelationException : ValidationException
{
    public InvalidRelationException(string message, string relationIdentifier);
    public SdmObjectReference<Relation> Relation { get; }
}
```

**Namespace:** `Skyline.DataMiner.Solutions.Relationships.Exceptions`

**Properties:**
- `Relation`: Reference to the relation that failed validation

**Description:** Thrown when a relation entity fails validation checks.

### InvalidRelationEntityException

Exception thrown when an entity referenced by a relation fails validation.

```csharp
public class InvalidRelationEntityException : InvalidRelationException
{
    public InvalidRelationEntityException(string message, string relationIdentifier, string entityId);
    public string EntityID { get; }
}
```

**Namespace:** `Skyline.DataMiner.Solutions.Relationships.Exceptions`

**Properties:**
- `Relation`: Reference to the relation (inherited)
- `EntityID`: Identifier of the entity that failed validation

**Description:** Thrown when an entity referenced by a relation (source or target) fails validation checks.

**Example:**
```csharp
try
{
    manager.Link(entityA, entityB);
}
catch (InvalidRelationEntityException ex)
{
    engine.GenerateInformation($"Entity validation failed: {ex.EntityID}");
    engine.GenerateInformation($"Relation: {ex.Relation.Identifier}");
}
catch (ValidationException ex)
{
    engine.GenerateInformation($"Validation error: {ex.Message}");
}
```

## NuGet Packages

| Package | Use Case | Namespace |
|---------|----------|-----------|
| `Skyline.DataMiner.Dev.Utils.Solutions.Relationships.Automation` | Automation Scripts | `Skyline.DataMiner.Solutions.Relationships` |
| `Skyline.DataMiner.Dev.Utils.Solutions.Relationships.Protocol` | Connectors/Protocols | `Skyline.DataMiner.Solutions.ObjectLinking` |
| `Skyline.DataMiner.Dev.Utils.Solutions.Relationships.GQI` | GQI Data Sources | `Skyline.DataMiner.Solutions.Relationships` |
| `Skyline.DataMiner.Dev.Utils.Solutions.Relationships.Common` | Other scenarios | `Skyline.DataMiner.Solutions.Relationships` |

## See Also

- **[Best Practices](./best-practices.md)** - Guidelines for using the library effectively
- [Getting Started](./getting-started.md) - Installation and setup
- [Creating Relationships](./creating-a-link-between-2-objects.md) - How to create links and relations
- [Querying Relationships](./retrieve-linked-objects.md) - Traversal and queries
- [Advanced Usage](./advanced-usage.md) - Advanced patterns and optimization
- [Examples](./examples.md) - Real-world examples
