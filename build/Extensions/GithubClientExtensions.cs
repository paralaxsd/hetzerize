using Nuke.Common.Git;
using Octokit;

namespace Extensions;

static class GithubClientExtensions
{
    public static async Task<GitTag?> FindTagInAsync(this GitHubClient githubClient, Repository repo, string tagName)
    {
        try
        {
            var gitClient = githubClient.Git;
            var refsClient = gitClient.Reference;
            var tagRef = await refsClient.Get(repo.Id, $"refs/tags/{tagName}");

            if(tagRef is { })
            {
                var tagsClient = gitClient.Tag;
                return await tagsClient.Get(repo.Id, tagRef.Object.Sha);
            }
        }
        catch (NotFoundException)
        {
            // intentionally left blank
        }

        return null;
    }

    public static async Task<GitTag> CreateNewTagAsync(
        this GitHubClient client, Repository repo, GitHubCommit targetCommit, string tagName)
    {
        var newTag = CreateNewTagAt(targetCommit, tagName);
        var tag = await client.Git.Tag.Create(repo.Id, newTag);

        // Create the reference to make the tag visible
        var newRef = new NewReference($"refs/tags/{tagName}", tag.Sha);
        await client.Git.Reference.Create(repo.Id, newRef);

        return tag;
    }

    public static Task<Repository> GetCurrentRepoFromAsync(this GitHubClient client, GitRepository gitRepo)
    {
        var (owner, repoName) = gitRepo.Identifier.Split('/') switch
        {
            var parts => (parts[0], parts[1])
        };
        return client.Repository.Get(owner, repoName);
    }

    static NewTag CreateNewTagAt(GitHubCommit targetCommit, string tagName)
    {
        var commitAuthor = targetCommit.Commit.Author;
        return new()
        {
            Message = tagName,
            Tag = tagName,
            Object = targetCommit.Sha,
            Type = TaggedType.Commit,
            Tagger = new(commitAuthor.Name, commitAuthor.Email, DateTimeOffset.UtcNow)
        };
    }
}