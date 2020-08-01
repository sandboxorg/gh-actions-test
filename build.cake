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

    var nuspecFile = MakeAbsolute(new FilePath("./src/gh-actions-test.nuspec"));

    context.Information($"Packing {nuspecFile}...");

    NuGetPack(nuspecFile, new NuGetPackSettings
    {
        BasePath = nuspecFile.GetDirectory(),
        Version = packageVersion,
        Owners = owners,
        ReleaseNotes = new [] { releaseNotes },
        OutputDirectory = "./build/packages",
    });
});

Task("publish")
    .IsDependentOn("pack")
    .Does(context =>
{
    var url =  context.EnvironmentVariable("NUGET_URL");
    if (string.IsNullOrWhiteSpace(url))
    {
        context.Information("No NuGet URL specified. Skipping publishing of NuGet package");
        return;
    }

    var apiKey =  context.EnvironmentVariable("NUGET_API_KEY");
    if (string.IsNullOrWhiteSpace(apiKey))
    {
        context.Information("No NuGet API key specified. Skipping publishing of NuGet package");
        return;
    }

    foreach (var nugetPackageFile in GetFiles("./build/packages/*.nupkg"))
    {
        context.Information($"Publishing {nugetPackageFile}...");
        context.NuGetPush(nugetPackageFile, new NuGetPushSettings
        {
            Source = url,
            ApiKey = apiKey,
        });
    }
});

RunTarget(target);
