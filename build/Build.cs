using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
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
class Build : NukeBuild
{
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("The publishing platform - Windows (default) or Linux")]
    readonly Platform PublishPlatform = Platform.Windows;

    [Solution(GenerateProjects = true)] readonly Solution Solution = null!;

    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath SourceDirectory => RootDirectory / "src";

    Target Clean => t => t
        .Before(Compile)
        .Before(Publish)
        .Before(Restore)
        .Executes(() =>
        {
            DotNetClean(s => s.SetProject(Solution.FileName));
            RootDirectory.GlobDirectories("bin", "tmp").ForEach(p => p.DeleteDirectory());
            SourceDirectory.GlobDirectories("**/bin", "**/tmp", "**/obj").ForEach(p => p.DeleteDirectory());
            ArtifactsDirectory.DeleteDirectory();
        });

    Target Compile => t => t
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution.FileName)
                .EnableDeterministic()
                .SetConfiguration(Configuration));
        });

    Target Recompile => t => t
        .DependsOn(Clean)
        .DependsOn(Compile);

    Target Publish => t => t
        .Produces(ArtifactsDirectory / "**.*")
        .Executes(() => PublishFor(PublishPlatform));

    Target PublishAll => t => t
        .Description("Publishes to the artifacts directory for all supported platforms")
        .Produces(ArtifactsDirectory / "**.*")
        .DependsOn(Clean)
        .DependsOn(Restore)
        .Executes(() =>
        {
            PublishFor(Platform.Windows);
            PublishFor(Platform.Linux);
        });

    Target Restore => t => t
        .Executes(() =>
        {
            DotNetRestore(s => s.SetProjectFile(Solution.FileName));
            DotNetToolRestore();
        });

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
