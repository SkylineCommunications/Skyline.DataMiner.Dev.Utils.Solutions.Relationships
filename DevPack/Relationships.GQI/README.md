# Skyline.DataMiner.Dev.Utils.Solutions.Relationships.GQI

A NuGet package providing models and helper classes for interacting with the DataMiner Relationships solution from within custom GQI data sources used in Dashboards and Low-Code Apps.

## Getting Started

> **Prerequisites:** A running DataMiner 10.5.9+ system with the [SDM Registration](https://catalog.dataminer.services/details/52173e49-9185-4772-9b60-c186ee365a81) package installed.

Obtain an `IRelationshipManager` from the `OnInitInputArgs` provided in your GQI data source's `OnInit` method and store it for use across the data source lifecycle. Always dispose it in `OnDestroy`.

**Set up the manager in a GQI data source:**

```csharp
using Skyline.DataMiner.Analytics.GenericInterface;
using Skyline.DataMiner.Solutions.Relationships;

[GQIMetaData(Name = "My Relationship Data Source")]
public class MyDataSource : IGQIDataSource, IGQIOnInit, IGQIOnDestroy
{
    private IRelationshipManager _manager;

    public OnInitOutputArgs OnInit(OnInitInputArgs args)
    {
        _manager = args.GetRelationshipManager();
        return new OnInitOutputArgs();
    }

    public OnDestroyOutputArgs OnDestroy(OnDestroyInputArgs args)
    {
        _manager?.Dispose();
        return new OnDestroyOutputArgs();
    }
}
```

**Query directly connected entities:**

```csharp
var neighbors = _manager.From(elementA).GetNeighbors();
```

**Traverse the graph up to N hops:**

```csharp
var results = _manager.From(elementA).Traverse(maxDepth: 3);
foreach (var result in results)
{
    // result.Entity, result.Depth, result.Via
}
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
