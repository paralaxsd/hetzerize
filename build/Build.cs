using Nuke.Common.CI.GitHubActions;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
// ReSharper disable UnusedMember.Local

[GitHubActions("compile", GitHubActionsImage.UbuntuLatest,
    On = [GitHubActionsTrigger.Push],
    FetchDepth = 0,
    InvokedTargets = [nameof(Compile)])]
[GitHubActions("publish-all", GitHubActionsImage.UbuntuLatest,
    On = [GitHubActionsTrigger.WorkflowDispatch],
    FetchDepth = 0,
    PublishArtifacts = true,
    InvokedTargets = [nameof(PublishAll)])]
[GitHubActions("create-release", GitHubActionsImage.UbuntuLatest,
    On = [GitHubActionsTrigger.WorkflowDispatch],
    FetchDepth = 0,
    InvokedTargets = [nameof(CreateRelease)],
    EnableGitHubToken = true)]
partial class Build : NukeBuild
{
    /******************************************************************************************
     * FIELDS
     * ***************************************************************************************/
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("The publishing platform - Windows (default) or Linux")]
    readonly Platform PublishPlatform = Platform.Windows;

    [Solution(GenerateProjects = true)] readonly Solution Solution = null!;

    /******************************************************************************************
     * PROPERTIES
     * ***************************************************************************************/
    public Target Clean => t => t
        .Description("Cleans the solution and erases temp + the artifact directory.")
        .Before(Compile)
        .Before(Publish)
        .Before(Restore)
        .Executes(CleanSolution);

    public Target Compile => t => t
        .Description("Builds the application.")
        .DependsOn(Restore)
        .Executes(CompileApplication);

    public Target Recompile => t => t
        .Description("Performs clean + compile at once.")
        .DependsOn(Clean)
        .DependsOn(Compile);

    public Target Publish => t => t
        .Description("Publishes the application for the target platform.")
        .Produces(ArtifactsDirectory / "**.*")
        .Executes(() => PublishFor(PublishPlatform));

    public Target PublishAll => t => t
        .Description("Publishes to the artifacts directory for all supported platforms")
        .Produces(ArtifactsDirectory / "**.*")
        .DependsOn(Clean)
        .DependsOn(Restore)
        .Executes(PublishForAllPlatforms);

    public Target Restore => t => t
        .Unlisted()
        .Executes(RestoreSolution);

    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath SourceDirectory => RootDirectory / "src";

    /******************************************************************************************
     * METHODS
     * ***************************************************************************************/
    void CleanSolution()
    {
        DotNetClean(s => s.SetProject(Solution.hetzerize.Path));
        RootDirectory
            .GlobDirectories("bin", "tmp")
            .ForEach(p => p.DeleteDirectory());
        SourceDirectory
            .GlobDirectories("**/bin", "**/tmp", "**/obj")
            .ForEach(p => p.DeleteDirectory());
        ArtifactsDirectory.DeleteDirectory();
    }

    void CompileApplication() =>
        DotNetBuild(s => s
            .SetProjectFile(Solution.hetzerize.Path)
            .EnableDeterministic()
            .SetConfiguration(Configuration));

    void PublishForAllPlatforms()
    {
        ArtifactsDirectory.CreateOrCleanDirectory();
        Platform.All.Apply(PublishFor);
    }

    void PublishFor(Platform targetPlatform) =>
        DotNetPublish( s => s
            .SetProject(Solution.hetzerize)
            .EnableDeterministic()
            .DisableSelfContained()
            .SetOperatingSystem(targetPlatform)
            .EnablePublishSingleFile()
            .AddProperty("PublishDir", ArtifactsDirectory / targetPlatform));

    void RestoreSolution()
    {
        DotNetRestore(s => s.SetProjectFile(Solution.Path));
        DotNetToolRestore();
    }

    public static int Main () => Execute<Build>(x => x.Compile);
}
