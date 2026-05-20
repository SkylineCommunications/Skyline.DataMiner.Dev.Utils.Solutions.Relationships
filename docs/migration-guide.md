# Migration Guide: Object Linking → Relationships

This guide helps you migrate from the legacy Object Linking solution to the new Relationships library.

## Overview

The Relationships library is a redesign of Object Linking with improved performance and cleaner APIs. Key changes:

- **Namespace**: `Skyline.DataMiner.SDM.ObjectLinking` → `Skyline.DataMiner.Solutions.Relationships`
- **Manager**: `IObjectLinker` → `IRelationshipManager`
- **Entry Point**: `connection.GetObjectLinker()` → `engine.GetRelationshipManager()`
- **Entity Key**: Uses `SolutionID` (not `SolutionName`) for entity identity - both fields are required
- **Link Model**: `Link` → `Relation` (always directed, no `LinkDirection` enum)
- **API**: `Create()` → `Link()` / `Relate()`, `GetLinkedEntities()` → `From().GetNeighbors()`

## Migration Overview

Migration requires two steps:

1. **[Code Migration](#migration-steps-code)** - Update your packages, namespaces, and API calls
2. **[Data Migration](#data-migration)** - Run the migration script to convert existing Object Linking data

> [!IMPORTANT]
> Both code and data migration are required. The libraries use different DOM modules and cannot share data.

## Migration Steps (Code)

Update your code to use the new Relationships library APIs:

### 1. Update NuGet Packages

```powershell
# Uninstall old packages
Uninstall-Package Skyline.DataMiner.SDM.ObjectLinking.Automation
Uninstall-Package Skyline.DataMiner.SDM.ObjectLinking.Protocol
Uninstall-Package Skyline.DataMiner.SDM.ObjectLinking.GQI

# Install new packages
Install-Package Skyline.DataMiner.Dev.Utils.Solutions.Relationships.Automation
Install-Package Skyline.DataMiner.Dev.Utils.Solutions.Relationships.Protocol
Install-Package Skyline.DataMiner.Dev.Utils.Solutions.Relationships.GQI
```

### 2. Update Namespaces

```csharp
// Old
using Skyline.DataMiner.SDM.ObjectLinking;

// New (Automation/GQI/Common)
using Skyline.DataMiner.Solutions.Relationships;

// Protocol (different namespace)
using Skyline.DataMiner.Solutions.ObjectLinking;
```

### 3. Update Manager Acquisition

### 3. Update Manager Acquisition

```csharp
// Old
IObjectLinker linker = connection.GetObjectLinker();

// New
IRelationshipManager manager = engine.GetRelationshipManager();
```

### 4. Update Entity Creation

> [!NOTE]
> **Required Fields**: `ID`, `DisplayName`, `ModelName`, `SolutionID`, `SolutionName`. If you set `ParentID` or `ParentModelName`, both must be provided.

```csharp
// Old: Used SolutionName for entity key
EntityDescriptor entity = new EntityDescriptor
{
    ID = "123",
    DisplayName = "My Entity",
    ModelName = "Type",
    SolutionName = "solution",  // Was part of entity key
    Status = EntityStatus.Active
};

// New: Uses SolutionID for entity key (both SolutionID and SolutionName required)
EntityDescriptor entity = new EntityDescriptor
{
    ID = "123",
    DisplayName = "My Entity",     // Required
    ModelName = "Type",            // Required
    SolutionID = "solution",       // Required - now used for EntityKey
    SolutionName = "My Solution",  // Required - descriptive name
    Status = EntityStatus.Active
};
```

### 5. Update Relationship Creation

```csharp
// Old: Create() method
linker.Create(entityA, entityB);                       // Symmetric
linker.Create(source, target, LinkDirection.Forward);  // Directed

// New: Link() and Relate() methods
manager.Link(entityA, entityB);   // Bidirectional (creates 2 relations)
manager.Relate(source, target);   // Single direction (creates 1 relation)
```

### 6. Update Querying

```csharp
// Old: GetLinkedEntities()
IList<EntityDescriptor> neighbors = linker.GetLinkedEntities(entity);

// New: Fluent traversal API
IEnumerable<EntityDescriptor> neighbors = manager.From(entity).GetNeighbors();

// New: Multi-hop traversal
TraversalResult results = manager.From(entity).Traverse(maxDepth: 3);

// New: Server-side filtering
var filtered = manager.From(entity)
    .Where(EntityDescriptorExposers.ModelName.Equal("Service"))
    .GetNeighbors();
```

### 7. Update Model References

```csharp
// Old: Link and LinkDirection
IBulkRepository<Link> links = linker.Links;

// New: Relation (no direction enum)
IBulkRepository<Relation> relations = manager.Relations;
```

## Troubleshooting

| Problem | Solution |
|---|---|
| `'Relationships' does not exist` | Install `Skyline.DataMiner.Dev.Utils.Solutions.Relationships.*` package |
| `'GetObjectLinker' not found` | Use `engine.GetRelationshipManager()` instead |
| `'Create' not found` | Use `Link()` or `Relate()` instead |
| `'GetLinkedEntities' not found` | Use `manager.From(entity).GetNeighbors()` instead |
| `'Link' type not found` | Use `Relation` instead, remove `LinkDirection` |
| No results after migration | Old and new libraries use different DOM modules - run data migration script first |

## Data Migration

> [!IMPORTANT]
> Old Object Linking (`(slc)object_linking` DOM module) and new Relationships (`(sdm)relationships` module) use **different DOM modules**. 
>
> You must run the **`Relationships.Install.Migration`** automation script to migrate your existing data.

### Running the Migration

1. Install the Relationships solution package on your DataMiner System
2. Run the Automation script: **`Relationships.Install.Migration`**
3. Verify completion in the Automation logs

The migration script:
- Reads all `Link` instances from `(slc)object_linking`
- Creates `EntityDescriptor` and `Relation` instances in `(sdm)relationships`
- Deletes migrated links from the old module
- Cleans up the old module when complete

### Pre-Migration Checklist

**Clean up your data before migrating:**

1. **Fill in empty `ModelName` fields** → Empty values become `"Migrated"`
2. **Fill in empty `SolutionName` fields** → Becomes your `SolutionID`, empty values become `"Migrated"`
3. **Review link directionality** → Old unordered links become full-mesh of directed relations
4. **Remove stale/duplicate links** → Keeps migrated data lean

### Field Mapping

| Old | New | Notes |
|---|---|---|
| `SolutionName` | `SolutionID` | Copied to SolutionID (becomes entity key component) |
| `SolutionName` | `SolutionName` | Preserved as-is (both fields required in new library) |
| Empty `ModelName` | `"Migrated"` | Default when empty |
| Empty `SolutionName` | `"Migrated"` | Default for both SolutionID and SolutionName when empty |
| `Link` with N entities | N×(N-1) `Relation` pairs | Full-mesh of directed relations |

### Migration Example

```csharp
// OLD: Link with 3 entities (unordered)
Link { EntityDescriptors = [ A, B, C ] }  // 1 Link

// NEW: Directed Relations (full mesh)
// Creates 6 Relation objects:
// A → B, A → C
// B → A, B → C
// C → A, C → B
```

## See Also

- [Getting Started](./getting-started.md) - Installation and basic usage
- [API Reference](./api-reference.md) - Complete API documentation
- [Best Practices](./best-practices.md) - Guidelines for effective use
- [Examples](./examples.md) - Real-world scenarios
