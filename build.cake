#tool "nuget:?package=xunit.runner.console"
#tool "nuget:?package=OpenCover"
#tool nuget:?package=Codecov
#addin nuget:?package=Cake.Codecov

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

Task("SetAppVeyorVersion").WithCriteria(AppVeyor.IsRunningOnAppVeyor).Does(() => 
{
    version = AppVeyor.Environment.Build.Version;

    if (AppVeyor.Environment.Repository.Tag.IsTag)
    {
        var tagName = AppVeyor.Environment.Repository.Tag.Name;
        if(tagName.StartsWith("v"))
        {
            version = tagName.Substring(1);
        }

        AppVeyor.UpdateBuildVersion(version);
    }
});

Task("Build")
    .IsDependentOn("SetAppVeyorVersion")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(()=> 
{
    var buildSettings =  new DotNetCoreBuildSettings { Configuration = configuration };
    if(!string.IsNullOrEmpty(version)) buildSettings.ArgumentCustomization = args => args.Append("/p:Version=" + version);

    DotNetCoreBuild("src/Configuration/Configuration.csproj", buildSettings);
});

Task("Test").IsDependentOn("Build").Does(() =>
{
    DotNetCoreTest("./tests/Configuration.Tests/Configuration.Tests.csproj", new DotNetCoreTestSettings
    {
        Configuration = configuration,
        ArgumentCustomization = args => args.Append("/p:BuildProjectReferences=false")
    });
});

Task("TestCoverage").IsDependentOn("Test").Does(() => 
{
    OpenCover(
        tool => { tool.XUnit2("tests/Configuration.Tests/bin/" + configuration + "/**/Configuration.Tests.dll", new XUnit2Settings { ShadowCopy = false }); },
        new FilePath("coverage.xml"),
        new OpenCoverSettings()
            .WithFilter("+[Configuration]*")
            .WithFilter("-[Configuration.Tests]*"));
});

Task("Upload-Coverage")
    .WithCriteria(AppVeyor.IsRunningOnAppVeyor)
    .IsDependentOn("TestCoverage")
    .Does(() =>
{
    Codecov("coverage.xml");
});

Task("Pack").IsDependentOn("Upload-Coverage").Does(()=> 
{
    CreateDirectory("build");
    
    CopyFiles(GetFiles("./src/Configuration/bin/**/*.nupkg"), "build");
    Zip("./src/Configuration/bin/" + configuration, "build/Configuration-" + version +".zip");
});

RunTarget(target);