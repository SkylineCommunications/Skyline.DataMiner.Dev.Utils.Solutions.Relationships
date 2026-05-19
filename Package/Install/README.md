# Skyline.DataMiner.Solutions.Relationships

A DataMiner solution that lets you register, store, and traverse directed relationships between entities — across protocols, Automation scripts, and GQI queries.

---

## What it does

- **Register entities** with a unique key (`SolutionID` + `ModelName` + `ID`).
- **Create relations** between entities (directed: Source → Target).
- **Traverse** the relationship graph to find neighbors or reachable entities up to a configurable depth.
- Works in **Automation**, **Protocol (QAction)**, and **GQI** contexts.

---

## Getting started

### 1. Install the package

Deploy the solution package to your DataMiner System. This registers the required DOM modules automatically.

### 2. Add the NuGet references

| Context | Package |
|---|---|
| Automation | `Skyline.DataMiner.Solutions.Relationships.Automation` |
| Protocol / QAction | `Skyline.DataMiner.Solutions.Relationships.Protocol` |
| GQI | `Skyline.DataMiner.Solutions.Relationships.GQI` |
| Any `IConnection` | `Skyline.DataMiner.Solutions.Relationships.Common` |

### 3. Create a manager and use it

**Automation script**
```csharp
using var manager = engine.GetRelationshipManager();

// Register entities
var server = new EntityDescriptor { ID = "srv-01", DisplayName = "Server 01", ModelName = "Server", SolutionID = "my-solution" };
var rack   = new EntityDescriptor { ID = "rack-01", DisplayName = "Rack A",    ModelName = "Rack",   SolutionID = "my-solution" };

manager.Entities.Create(server);
manager.Entities.Create(rack);

// Link them (bidirectional helper)
manager.Link(server, rack);

// Traverse neighbors
var neighbors = manager.From(server).GetNeighbors();
```

**Protocol (QAction)**
```csharp
using var manager = protocol.GetRelationshipManager();
// same API as above
```

**GQI**
```csharp
public OnInitOutputArgs OnInit(OnInitInputArgs args)
{
    _manager = args.GetRelationshipManager();
    return new OnInitOutputArgs();
}
```

---

## Key concepts

| Concept | Description |
|---|---|
| `EntityDescriptor` | Describes an entity. Identified by `SolutionID + ModelName + ID`. |
| `Relation` | A directed link from a source `EntityDescriptor` to a target `EntityDescriptor`. |
| `IRelationshipManager` | Entry point. Exposes `Entities`, `Relations`, and traversal helpers. |
| `EntityTraversal` | Fluent graph traversal starting from an entity. Use `.GetNeighbors()` or `.Traverse(maxDepth)`. |

---

## Migrating from Object Linking

If your DataMiner System previously used the **SLC Object Linking** solution (`(slc)object_linking` DOM module), a built-in migration script handles the transition automatically.

### What the migration does

- Reads all existing `Link` DOM instances from the `(slc)object_linking` module.
- Translates each link into one or more `Relation` and `EntityDescriptor` instances in the new `(sdm)relationships` module.
- Deletes the migrated links from the old module.
- Removes the old `(slc)object_linking` module settings once all links have been migrated.

### Before you run it

Before triggering the migration, it is strongly recommended to review your existing Object Linking data and bring it as close to the new structure as possible. This reduces the chance of ending up with incomplete or ambiguous data on the other side.

Specifically:

- **Fill in `ModelName`** on every entity where it is currently empty. Entries without a `ModelName` will be migrated with the value `"Migrated"`, which may be hard to work with afterwards.
- **Fill in `SolutionName`** on every entity where it is currently empty. This field becomes `SolutionID` in the new solution; empty values will also default to `"Migrated"`.
- **Review link directionality.** Object Linking stores entities in a link as an unordered set. The migration creates a directed full-mesh of relations (every entity → every other entity in the same link). If your use case requires a specific source/target direction, restructure your links into pairs before migrating.
- **Remove duplicate or stale links** you no longer need. Cleaning up beforehand keeps the migrated data lean.

### How to run it

1. Make sure the **Relationships** solution package is installed (step 1 above).
2. Run the Automation script **`Relationships.Install.Migration`** on your DataMiner System.
3. The script exits cleanly when done. If any links could not be migrated it will exit with a failure message — check the Automation logs for details.

### Things to know before migrating

| Old field | Maps to |
|---|---|
| `SolutionName` | `SolutionID` (used as fallback — no true SolutionID existed in Object Linking) |
| `ModelName` (empty) | Defaults to `"Migrated"` |
| `SolutionName` (empty) | Defaults to `"Migrated"` |
| Each `Link` with N entities | Creates N×(N-1) directed `Relation` pairs (full mesh) |

> **Note:** The old Object Linking solution stored links as an unordered set of entities. The migration expands each link into a directed full-mesh of relations. Review the resulting data if your use case requires a specific directionality.

---

## Unit testing

Each entry-point extension exposes a static override for easy mocking:

```csharp
EngineExtensions.OverrideGetRelationshipManager = _ => myFakeManager;
```

> **Note:** Do not run tests that use this override in parallel.
