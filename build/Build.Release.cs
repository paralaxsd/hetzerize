using Extensions;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.Tools.GitHub;
using Octokit;
using Serilog;

using static Octokit.AuthenticationType;

partial class Build
{
    /******************************************************************************************
     * FIELDS
     * ***************************************************************************************/
    [Secret, Parameter("The GitHub repo owner's token for authentication without a GITHUB_TOKEN.")]
    readonly string? GithubToken;

    [GitRepository] readonly GitRepository GitRepository = null!;

    /******************************************************************************************
     * PROPERTIES
     * ***************************************************************************************/
    public Target CreateRelease => t => t
        .DependsOn(PublishAll)
        .Executes(CreateNewRelease);

    /******************************************************************************************
     * METHODS
     * ***************************************************************************************/
    async Task CreateNewRelease()
    {
        var client = GetGitHubClient();

        var thisRepo = await client.GetCurrentRepoFromAsync(GitRepository);
        const string tagName = ThisAssembly.AssemblyInformationalVersion;

        CreateArtifacts(tagName);
        await CreateReleaseFromAsync(client, thisRepo, tagName);
    }

    GitHubClient GetGitHubClient()
    {
        var client = GitHubTasks.GitHubClient;
        var credentials = client.Credentials;
        if (credentials == null || credentials.AuthenticationType == Anonymous)
        {
            var githubAction = GitHubActions.Instance;

            if (GithubToken is { } token)
            {
                Information("Using user provided Github token for authentication.");
                client.Credentials = new(token);
            }
            else if(!string.IsNullOrWhiteSpace(githubAction.Token))
            {
                Information("Using action provided GITHUB_TOKEN for authentication.");
                client.Credentials = new(githubAction.Token);
            }
            else
            {
                Assert.Fail("Client credentials are missing. " + 
                    "Either provide a GITHUB_TOKEN or an auth token with appropriate permissions.");
            }
        }

        return client;
    }

    void CreateArtifacts(string tag)
    {
        Information("Creating artifacts...");
        Platform.All.Apply(platform => CreateCompressedArtifactsFor(platform, tag));
    }

    async Task CreateReleaseFromAsync(GitHubClient client, Repository repo, string tagName)
    {
        var newRelease = await CreateNewReleaseObjAsync(client, repo, tagName);

        var releaseClient = client.Repository.Release;
        var release = await releaseClient.Create(repo.Id, newRelease);

        await AttachArtifactsToAsync(release, releaseClient);
    }

    async Task<NewRelease> CreateNewReleaseObjAsync(GitHubClient client, Repository repo, string tagName)
    {
        Information("Creating release...");
        var repoClient = client.Repository;
        var releaseCommits = await GetCommitsForReleaseAsync(repoClient, repo);
        var latestCommit = releaseCommits.First();
        if (releaseCommits.Count == 0)
        {
            Assert.Fail(
                "Cannot create a new release: there are no new commits since the last release.");
        }

        var body = CreateChangelogTextFrom(releaseCommits);
        var tag = await client.FindTagInAsync(repo, tagName) ??
                  await client.CreateNewTagAsync(repo, latestCommit, tagName);

        return new(tagName)
        {
            Body = body,
            Name = tagName,
            TargetCommitish = tag.Object.Sha
        };
    }

    async Task AttachArtifactsToAsync(Release release, IReleasesClient releaseClient)
    {
        var artifactPaths = ArtifactsDirectory
            .GlobFiles("*.zip", "*.tar.gz")
            .OrderBy(p => (string)p)
            .ToArray();

        foreach (var path in artifactPaths)
        {
            Information($"Uploading {path.Name}...");

            await using var stream = File.OpenRead(path);
            var upload = new ReleaseAssetUpload(path.Name, "application/zip", stream, null);
            await releaseClient.UploadAsset(release, upload);
        }
    }

    void Information(string text) => Log.Information(text);

    void CreateCompressedArtifactsFor(Platform platform, string tag)
    {
        var ext = platform == Platform.Windows ? "zip" : "tar.gz";

        (ArtifactsDirectory / platform)
            .CompressTo(ArtifactsDirectory / $"hetzerize-{tag}-{platform}-x64.{ext}");
    }

    async Task<IReadOnlyList<GitHubCommit>> GetCommitsForReleaseAsync(IRepositoriesClient repoClient, Repository repo)
    {
        var repoId = repo.Id;
        var defaultBranch = await repoClient.Branch.Get(repoId, repo.DefaultBranch);
        var defaultBranchOnly = new CommitRequest { Sha = defaultBranch.Commit.Sha };
        var allCommits = await repoClient.Commit.GetAll(repoId, defaultBranchOnly);

        try
        {
            var latestRelease = await repoClient.Release.GetLatest(repoId);
            return allCommits.TakeUntil(commit => commit.Sha == latestRelease.TargetCommitish).ToArray();
        }
        catch (NotFoundException)
        {
            return allCommits;
        }
    }

    static string CreateChangelogTextFrom(IReadOnlyList<GitHubCommit> releaseCommits)
    {
        var commits = releaseCommits.Select(CreateChangelogLineFrom)
            .JoinedBy($"  {Environment.NewLine}");
        return $"## Changelog:  {Environment.NewLine}{commits}";
    }

    static string CreateChangelogLineFrom(GitHubCommit comm) =>
        $"* {comm.Commit.Message.Split('\r', '\n')[0]} by @{comm.Committer.Login}";
}