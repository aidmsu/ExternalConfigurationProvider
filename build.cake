#tool "nuget:?package=xunit.runner.console"
#tool "nuget:?package=OpenCover"

var configuration = Argument("configuration", "Release");
var version = Argument<string>("buildVersion", null);
var target = Argument("target", "Default");

Task("Default").IsDependentOn("Pack");

Task("Clean").Does(()=> 
{
    CleanDirectory("./build");
    StartProcess("dotnet", "clean -c:" + configuration);
});

Task("Restore").Does(()=> 
{
    DotNetCoreRestore();
});

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(()=> 
{
    var buildSettings =  new DotNetCoreBuildSettings { Configuration = configuration };
    if(!string.IsNullOrEmpty(version)) buildSettings.ArgumentCustomization = args => args.Append("/p:Version=" + version);

    DotNetCoreBuild("src/Sdl.Configuration/Sdl.Configuration.csproj", buildSettings);
});

Task("Test").IsDependentOn("Build").Does(() =>
{
    DotNetCoreTest("./tests/Sdl.Configuration.Tests/Sdl.Configuration.Tests.csproj", new DotNetCoreTestSettings
    {
        Configuration = configuration,
        ArgumentCustomization = args => args.Append("/p:BuildProjectReferences=false")
    });
});

Task("TestCoverage").IsDependentOn("Test").Does(() => 
{
    OpenCover(
        tool => { tool.XUnit2("tests/Sdl.Configuration.Tests/bin/" + configuration + "/**/Sdl.Configuration.Tests.dll", new XUnit2Settings { ShadowCopy = false }); },
        new FilePath("coverage.xml"),
        new OpenCoverSettings()
            .WithFilter("+[Sdl.Configuration]*")
            .WithFilter("-[Sdl.Configuration.Tests]*"));
});

Task("Pack").IsDependentOn("TestCoverage").Does(()=> 
{
    CreateDirectory("build");
    
    CopyFiles(GetFiles("./src/Sdl.Configuration/bin/**/*.nupkg"), "build");
    Zip("./src/Sdl.Configuration/bin/" + configuration, "build/Sdl.Configuration-" + version +".zip");
});

RunTarget(target);