# Skyline.DataMiner.Dev.Utils.Solutions.Relationships.Protocol

A NuGet package providing models and helper classes for interacting with the DataMiner Relationships solution from within DataMiner connectors (protocols/drivers).

## Getting Started

> **Prerequisites:** A running DataMiner 10.5.9+ system with the [SDM Registration](https://catalog.dataminer.services/details/52173e49-9185-4772-9b60-c186ee365a81) package installed.

Obtain an `IRelationshipManager` from the `protocol` object available in your connector QAction and use it to register entities and the relationships between them. Always wrap it in a `using` block to properly clean up resources.

**Link two entities (bidirectional):**

```csharp
using (var manager = protocol.GetRelationshipManager())
{
    var elementA = new EntityDescriptor
    {
        ID = "element-a",
        DisplayName = "Element A",
        ModelName = "Element",
        SolutionID = "my-solution",
        SolutionName = "My Solution"
    };

    var elementB = new EntityDescriptor
    {
        ID = "element-b",
        DisplayName = "Element B",
        ModelName = "Element",
        SolutionID = "my-solution",
        SolutionName = "My Solution"
    };

    manager.Link(elementA, elementB); // creates a ↔ b
}
```

**Get directly connected entities:**

```csharp
using (var manager = protocol.GetRelationshipManager())
{
    var neighbors = manager.From(elementA).GetNeighbors();
    foreach (var neighbor in neighbors)
        protocol.Log($"Connected to: {neighbor.DisplayName}");
}
```

**Traverse the graph up to N hops:**

```csharp
using (var manager = protocol.GetRelationshipManager())
{
    var results = manager.From(elementA).Traverse(maxDepth: 3);
    foreach (var result in results)
        protocol.Log($"{result.Entity.DisplayName} — {result.Depth} hop(s) away");
}
```

**Create a directed (one-way) relation:**

```csharp
manager.Relate(source, target); // creates source → target only
```

For more details see the [full documentation](https://github.com/SkylineCommunications/Skyline.DataMiner.Dev.Utils.Solutions.ObjectLinking/tree/1.0.X/docs).

## About

Contains the models and helper classes to interact with the relationships solution.

### About DataMiner

DataMiner is a transformational platform that provides vendor-independent control and monitoring of devices and services. Out of the box and by design, it addresses key challenges such as security, complexity, multi-cloud, and much more. It has a pronounced open architecture and powerful capabilities enabling users to evolve easily and continuously.

The foundation of DataMiner is its powerful and versatile data acquisition and control layer. With DataMiner, there are no restrictions to what data users can access. Data sources may reside on premises, in the cloud, or in a hybrid setup.

A unique catalog of 7000+ connectors already exists. In addition, you can leverage DataMiner Development Packages to build your own connectors (also known as "protocols" or "drivers").

> **Note**
> See also: [About DataMiner](https://aka.dataminer.services/about-dataminer).

### About Skyline Communications

At Skyline Communications, we deal in world-class solutions that are deployed by leading companies around the globe. Check out [our proven track record](https://aka.dataminer.services/about-skyline) and see how we make our customers' lives easier by empowering them to take their operations to the next level.

<!-- Uncomment below and add more info to provide more information about how to use this package. -->
<!-- ## Getting Started -->
