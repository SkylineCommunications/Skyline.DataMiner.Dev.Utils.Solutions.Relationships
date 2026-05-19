# DataMiner Relationships Library

This folder contains comprehensive guides for managing relationships between entities in DataMiner using the Relationships library.

## Overview

The Relationships library provides a standardized way to create, manage, and query connections between different DataMiner objects (elements, services, bookings, custom entities, etc.). It offers a powerful, fluent API for building relationship graphs and traversing them efficiently.

## Documentation Index

### Getting Started
- **[Getting Started](./getting-started.md)** - Installation, prerequisites, and your first relationship
- **[Creating Relationships](./creating-relationships.md)** - How to create bidirectional links and directed relations
- **[Querying Relationships](./querying-relationships.md)** - Retrieve related entities and traverse the relationship graph

### Advanced Topics
- **[Advanced Usage](./advanced-usage.md)** - Filtering, entity management, status tracking, and performance tips
- **[API Reference](./api-reference.md)** - Complete API documentation
- **[Examples](./examples.md)** - Real-world usage scenarios and patterns

## Quick Reference

### Installation

```bash
# For Automation Scripts
Install-Package Skyline.DataMiner.Dev.Utils.Solutions.Relationships.Automation

# For Connectors/Protocols
Install-Package Skyline.DataMiner.Dev.Utils.Solutions.Relationships.Protocol

# For GQI Data Sources
Install-Package Skyline.DataMiner.Dev.Utils.Solutions.Relationships.GQI

# For other scenarios (Class Libraries, etc.)
Install-Package Skyline.DataMiner.Dev.Utils.Solutions.Relationships.Common
```

### Getting the Relationship Manager

```csharp
// Automation Script
var manager = engine.GetRelationshipManager();

// Protocol/Connector
var manager = protocol.GetRelationshipManager();

// GQI
var manager = args.DMS.GetRelationshipManager(); // In OnInit method

// Other (with IConnection)
var manager = connection.GetRelationshipManager();
```

### Basic Operations

```csharp
// Create a bidirectional link
var entity1 = new EntityDescriptor
{
    ID = "element-123",
    DisplayName = "Router A",
    ModelName = "Router",
    SolutionID = "network-topology",
    SolutionName = "Network Topology"
};

var entity2 = new EntityDescriptor
{
    ID = "element-456",
    DisplayName = "Switch B",
    ModelName = "Switch",
    SolutionID = "network-topology",
    SolutionName = "Network Topology"
};

manager.Link(entity1, entity2);

// Query related entities
var neighbors = manager.From(entity1).GetNeighbors();

// Traverse the graph
var results = manager.From(entity1).Traverse(maxDepth: 3);
```

## Key Concepts

- **EntityDescriptor**: Represents any DataMiner entity (element, service, booking, etc.)
- **Relation**: A directed connection from one entity to another
- **Link**: A bidirectional (symmetric) relationship created by two complementary relations
- **Traversal**: Graph exploration starting from an entity, following relationships breadth-first
- **Repository Pattern**: Low-level access to entities and relations with full CRUD operations

## Prerequisites

> [!IMPORTANT]
> You must install the [SDM Registration](https://catalog.dataminer.services/details/52173e49-9185-4772-9b60-c186ee365a81) package in DataMiner before using the Relationships library.

## Support & Contributing

For issues, questions, or contributions, please refer to the main repository README.