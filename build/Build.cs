using Nuke.Common.CI.GitHubActions;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
// ReSharper disable UnusedMember.Local

[GitHubActions("compile", GitHubActionsImage.UbuntuLatest,
    On = [GitHubActionsTrigger.Push],
    Progress = true,
    FetchDepth = 0,
    InvokedTargets = [nameof(Compile)])]
[GitHubActions("publish-all", GitHubActionsImage.UbuntuLatest,
    On = [GitHubActionsTrigger.WorkflowDispatch],
    Progress = true,
    FetchDepth = 0,
    PublishArtifacts = true,
    InvokedTargets = [nameof(PublishAll)])]
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
        .Before(Compile)
        .Before(Publish)
        .Before(Restore)
        .Executes(() =>
        {
            DotNetClean(s => s.SetProject(Solution.hetzerize.Path));
            RootDirectory.GlobDirectories("bin", "tmp").ForEach(p => p.DeleteDirectory());
            SourceDirectory.GlobDirectories("**/bin", "**/tmp", "**/obj").ForEach(p => p.DeleteDirectory());
            ArtifactsDirectory.DeleteDirectory();
        });

    public Target Compile => t => t
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution.Path)
                .EnableDeterministic()
                .SetConfiguration(Configuration));
        });

    public Target Recompile => t => t
        .DependsOn(Clean)
        .DependsOn(Compile);

    public Target Publish => t => t
        .Produces(ArtifactsDirectory / "**.*")
        .Executes(() => PublishFor(PublishPlatform));

    public Target PublishAll => t => t
        .Description("Publishes to the artifacts directory for all supported platforms")
        .Produces(ArtifactsDirectory / "**.*")
        .DependsOn(Clean)
        .DependsOn(Restore)
        .Executes(() =>
        {
            ArtifactsDirectory.CreateOrCleanDirectory();
            Platform.All.Apply(PublishFor);
        });

    public Target Restore => t => t
        .Executes(() =>
        {
            DotNetRestore(s => s.SetProjectFile(Solution.Path));
            DotNetToolRestore();
        });
    
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath SourceDirectory => RootDirectory / "src";

    static Tool NerdBankGitVersioning => ToolResolver.GetPathTool("nbgv");

    /******************************************************************************************
     * METHODS
     * ***************************************************************************************/
    void PublishFor(Platform targetPlatform)
    {
        DotNetPublish( s => s
            .SetProject(Solution.hetzerize)
            .EnableDeterministic()
            .DisableSelfContained()
            .SetOperatingSystem(targetPlatform)
            .EnablePublishSingleFile()
            .AddProperty("PublishDir", ArtifactsDirectory / targetPlatform));
    }

    public static int Main () => Execute<Build>(x => x.Compile);
}
