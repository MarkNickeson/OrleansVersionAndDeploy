# Objective: Demonstrate Microsoft Orleans Grain Versioning Behavior

Microsoft Orleans has opt-in support for grain versioning using a combination of attribute-decoration and silo configuration
1) [VersionAttribute](https://learn.microsoft.com/en-us/dotnet/api/orleans.codegeneration.versionattribute?view=orleans-7.0)
2) [Version selector strategy](https://learn.microsoft.com/en-us/dotnet/orleans/grains/grain-versioning/version-selector-strategy); and
3) [Version compatibility](https://learn.microsoft.com/en-us/dotnet/orleans/grains/grain-versioning/compatible-grains)

I found it difficult to intuitively understand all of the potential implications so I built this repo to enable systematic testing.

## Appologies

Realistic testing requires isolated processes because the grain interfaces and implementations type names all collide. As a result the
test setup is somewhat complex as I resorted to interprocess communication to coordinate silo start/stop.

## Key Dependencies

Orleans 7.0
