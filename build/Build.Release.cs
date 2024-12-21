using Nuke.Common.Tools.GitHub;
using Octokit;
using Serilog;

partial class Build
{
    /******************************************************************************************
     * FIELDS
     * ***************************************************************************************/
    [Parameter("The GitHub repo owner's user name for tokenless basic authentication")]
    readonly string? GithubLogin;

    [Parameter("The GitHub repo owner's password for tokenless basic authentication"), Secret]
    readonly string? GithubPassword;


    /******************************************************************************************
     * PROPERTIES
     * ***************************************************************************************/
    public Target CreateRelease => t => t
        //.DependsOn(PublishAll)
        //.DependsOn(Restore)
        .Executes(CreateNewRelease);

    /******************************************************************************************
     * METHODS
     * ***************************************************************************************/
    async Task CreateNewRelease()
    {
        var client = GetGitHubClient();


        var repoClient = client.Repository;
        var repo = await repoClient.Get("paralaxsd", "hetzerize");
        var tagName = ThisAssembly.AssemblyFileVersion;
        tagName = "v1.0.5-beta";

        //CreateArtifacts(tag);
        var newRelease = await CreateReleaseAsync(client, repo, tagName);
        await CreateRelaseAsync(newRelease, repoClient, repo);
    }

    GitHubClient GetGitHubClient()
    {
        var client = GitHubTasks.GitHubClient;
        if(client.Credentials == null || client.Credentials.AuthenticationType == AuthenticationType.Anonymous)
        {
            // Can we use basic auth?
            // As seen at https://octokitnet.readthedocs.io/en/documentation/getting-started/#authenticated-access
            if(GithubLogin is { } login && GithubPassword is { } password)
            {
                client.Credentials = new Credentials(login, password);
            }
            else
            {
                Assert.Fail("Client credentials are missing. Either provide a GITHUB_TOKEN or login+password for the repo owner.");
            }
        }

        return client;
    }

    void CreateArtifacts(string tag)
    {
        Information("Creating artifacts...");
        Platform.All.Apply(platform => CreateCompressedArtifactsFor(platform, tag));
    }

    async Task<NewRelease> CreateReleaseAsync(GitHubClient client, Repository repo, string tagName)
    {
        Information("Creating release...");
        var repoClient = client.Repository;
        var releaseCommits = await GetCommitsForReleaseAsync(repoClient, repo);
        if (releaseCommits.Count == 0)
        {
            throw new InvalidOperationException(
                "Cannot create a new release: there are no new commits since the last release.");
        }
        var body = CreateReleaseBodyFrom(releaseCommits);
        var latestCommit = releaseCommits.First();
        GitTag tag;
        var tagsClient = client.Git.Tag;
        try
        {
            tag = await tagsClient.Get(repo.Id, tagName);
        }
        catch (NotFoundException)
        {
            var commitAuthor = latestCommit.Commit.Author;
            Console.WriteLine($"Tag name: {tagName}");
            Console.WriteLine($"Commit SHA: {latestCommit.Sha}");
            Console.WriteLine($"Author name: {commitAuthor.Name}");
            Console.WriteLine($"Author email: {commitAuthor.Email}");

            var newTag = new NewTag()
            {
                Message = tagName,
                Tag = tagName,
                Object = latestCommit.Sha,
                Type = TaggedType.Commit,
                Tagger = new(commitAuthor.Name, commitAuthor.Email, DateTimeOffset.UtcNow)
            };
            tag = await tagsClient.Create("paralaxsd", "hetzerize", newTag);

            // Create the reference to make the tag visible
            var newRef = new NewReference($"refs/tags/{tagName}", tag.Sha);
            await client.Git.Reference.Create(repo.Id, newRef);
        }
    
        return new(tagName)
        {
            Body = body,
            Name = tagName,
            TargetCommitish = latestCommit.Sha,  // Changed this to use commit SHA instead of tag
        };
    }

    async Task CreateRelaseAsync(
        NewRelease newRelease, IRepositoriesClient repoClient, Repository repo)
    {
        var releaseClient = repoClient.Release;
        var release = await releaseClient.Create(repo.Id, newRelease);
        var artifactPaths = ArtifactsDirectory
            .GlobFiles("*.zip", "*.tar,gz").OrderBy(p => p);

        foreach(var path in artifactPaths)
        {
            Information($"Uploading {path.Name}...");

            await using var stream = File.OpenRead(path);
            var upload = new ReleaseAssetUpload(path, "application/zip", stream, null);
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
        var mainBranch = await repoClient.Branch.Get(repoId, repo.DefaultBranch);

        var allCommits = await repoClient.Commit.GetAll(repoId, new CommitRequest() { Sha = mainBranch.Commit.Sha });

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

    static string CreateReleaseBodyFrom(IReadOnlyList<GitHubCommit> releaseCommits)
    {
        var title = $"## Changelog:  {Environment.NewLine}";
        var commits = releaseCommits.Select(CreateChangelogLineFrom)
            .JoinedBy($"  {Environment.NewLine}");
        return title + commits;
    }

    static string CreateChangelogLineFrom(GitHubCommit comm) =>
        $"* {comm.Commit.Message.Split('\r', '\n')[0]} by @{comm.Committer.Login}";
}