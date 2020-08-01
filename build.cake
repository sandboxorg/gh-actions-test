var target          = Argument<string>("target", "pack");
var packageVersion  = Argument<string>("packageVersion", "0.0.1-local");

#tool "nuget:?package=NuGet.CommandLine&version=5.6.0"

Task("clean")
    .Does(() =>
{
    CleanDirectory("./build/packages");
});

Task("pack")
    .IsDependentOn("clean")
    .Does(context =>
{
    var owners = new[] { "augustoproiete" };
    var releaseNotes = $"https://github.com/augustoproiete/EventLogMessages/releases/tag/v{packageVersion}";

    var nuspecFile = MakeAbsolute(new FilePath("./src/EventLogMessages.nuspec"));

    context.Information($"Packing {nuspecFile}...");

    NuGetPack(nuspecFile, new NuGetPackSettings
    {
        BasePath = nuspecFile.GetDirectory(),
        Version = packageVersion,
        Owners = owners,
        ReleaseNotes = new [] { releaseNotes },
        OutputDirectory = "./build/packages",
    });

    var superSecret =  context.EnvironmentVariable("SUPERSECRETVALUE");
    if (string.IsNullOrWhiteSpace(superSecret))
    {
        Console.WriteLine("No SUPERSECRETVALUE specified");
    }
    else
    {
        Console.WriteLine($"SUPERSECRETVALUE: {superSecret}");
    }

    Console.WriteLine("Writing SuperSecret value to text file");
    System.IO.File.WriteAllText(@".\build\packages\SuperSecret.txt", $"SUPERSECRETVALUE: {superSecret}");
});

Task("publish")
    .IsDependentOn("pack")
    .Does(context =>
{
    var apiKey =  context.EnvironmentVariable("NUGET_API_KEY");
    if (string.IsNullOrWhiteSpace(apiKey))
    {
        throw new CakeException("No NuGet API key specified.");
    }

    foreach (var nugetPackageFile in GetFiles("./build/packages/*.nupkg"))
    {
        context.Information($"Publishing {nugetPackageFile}...");
        context.NuGetPush(nugetPackageFile, new NuGetPushSettings
        {
            ApiKey = apiKey,
            Source = "https://www.myget.org/F/augustoproiete/api/v2/package",
        });
    }
});

RunTarget(target);
