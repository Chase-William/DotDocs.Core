## How to Use

Its simple, either provide a path to the location of the repository on your local machine or provide a url to the repository's github.

```cs
// var builder = Builder.FromPath("...");
// OR
var builder = Builder.FromUrl("...");
builder.Prepare();
builder.Build();
builder.Document();
```

#### Prepare();

Downloads the repository via the given url if nessessary and ensures the validity of the path to the new or existing repository.

#### Build();

Collects metadata, issues the MSBuild system to compile the root project and its dependencies, and instanciates models. 

#### Document();

Uses models to generate markdown files for each corresponding type and its members.