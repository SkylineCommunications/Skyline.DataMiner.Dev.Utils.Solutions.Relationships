# Relationships

When building DataMiner solutions, you often need to create relationships between different objects (such as elements, services, or custom data types). This library provides a standard way to create, manage, and query those links.

It comes with:
- A C# library for managing the relations in your DataMiner solution
- Helper classes and models to simplify automation and integration scenarios

## How to use

The entry point for the library is the **IRelationshipManager**. You can use it to create and query links between objects.

> [!IMPORTANT]
> You must install the [SDM Registration](https://catalog.dataminer.services/details/52173e49-9185-4772-9b60-c186ee365a81) before in order for Relationships to work.

### Getting the linker

```csharp
// Getting the Relationship Manager

// Automation Scripts -> nuget Skyline.DataMiner.Dev.Utils.Solutions.Relationships.Automation
var manager = engine.GetRelationshipManager();

// Connectors -> nuget Skyline.DataMiner.Dev.Utils.Solutions.Relationships.Protocol
var manager = protocol.GetRelationshipManager();

// GQI -> nuget Skyline.DataMiner.Dev.Utils.Solutions.Relationships.GQI
var manager = args.DMS.GetRelationshipManager(); // In the OnInit life cycle method

// Other -> nuget Skyline.DataMiner.Dev.Utils.Solutions.Relationships.Common
var manager = connection.GetRelationshipManager();
```

### Creating links

```csharp
// Creating a symmetric link between 2 entities.
var entity1 = new EntityDescriptor
{
	ID = "entity1",
	DisplayName = "Entity 1",
	ModelName = "Model A",
	SolutionID = "solution_x",
	SolutionName = "Solution X",
	ParentID = "parent1",
	ParentModelName = "Parent Model A",
};
var entity2 = new EntityDescriptor
{
	ID = "entity2",
	DisplayName = "Entity 2",
	ModelName = "Model B",
	SolutionID = "solution_y",
	SolutionName = "Solution Y",
	ParentID = "parent2",
	ParentModelName = "Parent Model B",
};

manager.Create(entity1, entity2);
```

### Querying relations

```csharp
// Querying relations between entities.
var entityId = "entity1";
var relatedEntities = manager.GetRelatedEntities(entityId);

// Custom filters
var filter = RelationExposers.Source.SolutionName.Equal("Solution X");
var relationsWithFilter = linker.Links.Read(filter);
```

### About DataMiner

DataMiner is a transformational platform that provides vendor-independent control and monitoring of devices and services. Out of the box and by design, it addresses key challenges such as security, complexity, multi-cloud, and much more. It has a pronounced open architecture and powerful capabilities enabling users to evolve easily and continuously.

The foundation of DataMiner is its powerful and versatile data acquisition and control layer. With DataMiner, there are no restrictions to what data users can access. Data sources may reside on premises, in the cloud, or in a hybrid setup.

A unique catalog of 7000+ connectors already exists. In addition, you can leverage DataMiner Development Packages to build your own connectors (also known as "protocols" or "drivers").

> **Note**
> See also: [About DataMiner](https://aka.dataminer.services/about-dataminer).

### About Skyline Communications

At Skyline Communications, we deal with world-class solutions that are deployed by leading companies around the globe. Check out [our proven track record](https://aka.dataminer.services/about-skyline) and see how we make our customers' lives easier by empowering them to take their operations to the next level.
