using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace SCG.UnityGitStatus.Tests
{
    /// <summary>
    /// Verifies changed-path ordering used by the Git Status window.
    /// </summary>
    public sealed class GitStatusEntrySorterTests
    {
        /// <summary>
        /// Verifies that file-name ordering ignores parent folders and remains deterministic.
        /// </summary>
        [Test]
        public void Sort_FileNameAscending_OrdersByFileNameThenPath()
        {
            var entries = new[]
            {
                Create("Assets/Z/Beta.cs", GitStatusKind.Modified),
                Create("Assets/B/Alpha.cs", GitStatusKind.Added),
                Create("Assets/A/Alpha.cs", GitStatusKind.Deleted)
            };

            var result = GitStatusEntrySorter.Sort(entries, GitStatusSortMode.FileNameAscending, false);

            Assert.That(result.Select(entry => entry.Path), Is.EqualTo(new[]
            {
                "Assets/A/Alpha.cs",
                "Assets/B/Alpha.cs",
                "Assets/Z/Beta.cs"
            }));
        }

        /// <summary>
        /// Verifies that status ordering follows the window's documented status priority.
        /// </summary>
        [Test]
        public void Sort_FileStatus_GroupsByStatusPriority()
        {
            var entries = new List<GitStatusEntry>
            {
                Create("Assets/Untracked.cs", GitStatusKind.Untracked),
                Create("Assets/Modified.cs", GitStatusKind.Modified),
                Create("Assets/Deleted.cs", GitStatusKind.Deleted),
                Create("Assets/Conflicted.cs", GitStatusKind.Conflicted),
                Create("Assets/Added.cs", GitStatusKind.Added)
            };

            var result = GitStatusEntrySorter.Sort(entries, GitStatusSortMode.FileStatus, false);

            Assert.That(result.Select(entry => entry.Kind), Is.EqualTo(new[]
            {
                GitStatusKind.Conflicted,
                GitStatusKind.Deleted,
                GitStatusKind.Added,
                GitStatusKind.Modified,
                GitStatusKind.Untracked
            }));
        }

        /// <summary>
        /// Verifies that Unity display remapping does not discard repository-relative rename paths.
        /// </summary>
        [Test]
        public void WithDisplayPath_RenamedEntry_PreservesRepositoryPaths()
        {
            var entry = new GitStatusEntry(
                "Assets/New.cs",
                "Sibling/Old.cs",
                GitStatusKind.Renamed,
                false,
                false,
                "Assets/New.cs",
                "UnityProject/Assets/New.cs",
                "Sibling/Old.cs");

            var result = entry.WithDisplayPath("Assets/New.cs", false);

            Assert.That(result.RepositoryPath, Is.EqualTo("UnityProject/Assets/New.cs"));
            Assert.That(result.OriginalRepositoryPath, Is.EqualTo("Sibling/Old.cs"));
        }

        private static GitStatusEntry Create(string path, GitStatusKind kind) =>
            new(path, string.Empty, kind, false, false, path);
    }
}
