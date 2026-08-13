using System.Linq;
using NUnit.Framework;

namespace SCG.UnityGitStatus.Tests
{
    /// <summary>
    /// Verifies unified diff parsing and partial patch generation.
    /// </summary>
    public sealed class GitDiffParserTests
    {
        /// <summary>
        /// Verifies that multiple hunks and Unicode content retain their line numbers.
        /// </summary>
        [Test]
        public void Parse_MultipleHunksWithUnicode_RetainsStructure()
        {
            const string patch = "diff --git a/Assets/a file.txt b/Assets/a file.txt\n--- a/Assets/a file.txt\n+++ b/Assets/a file.txt\n@@ -1,2 +1,2 @@\n-old\n+новый\n context\n@@ -10 +10 @@\n-before\n+after\n";

            var result = GitDiffParser.Parse("Assets/a file.txt", GitDiffSide.Unstaged, patch);

            Assert.That(result.Hunks, Has.Count.EqualTo(2));
            Assert.That(result.Hunks[0].Lines[1].Content, Is.EqualTo("новый"));
            Assert.That(result.Hunks[1].Lines[0].OldLineNumber, Is.EqualTo(10));
        }

        /// <summary>
        /// Verifies that binary and rename metadata select whole-file behavior.
        /// </summary>
        [Test]
        public void Parse_SpecialMetadata_RequiresWholeFileAction()
        {
            const string patch = "diff --git a/old.bin b/new.bin\nsimilarity index 100%\nrename from old.bin\nrename to new.bin\nBinary files a/old.bin and b/new.bin differ\n";

            var result = GitDiffParser.Parse("new.bin", GitDiffSide.Staged, patch);

            Assert.That(result.IsBinary, Is.True);
            Assert.That(result.IsRenameOrCopy, Is.True);
            Assert.That(result.RequiresWholeFileAction, Is.True);
        }

        /// <summary>
        /// Verifies that combined conflict diffs do not expose ordinary two-way line actions.
        /// </summary>
        [Test]
        public void Parse_CombinedConflictDiff_RequiresWholeFileAction()
        {
            const string patch = "diff --cc Assets/File.cs\nindex 1111111,2222222..3333333\n--- a/Assets/File.cs\n+++ b/Assets/File.cs\n@@@ -1,1 -1,1 +1,1 @@@\n- left\n -right\n++merged\n";

            var result = GitDiffParser.Parse("Assets/File.cs", GitDiffSide.Unstaged, patch);

            Assert.That(result.IsConflicted, Is.True);
            Assert.That(result.RequiresWholeFileAction, Is.True);
            Assert.That(result.Hunks, Is.Empty);
        }

        /// <summary>
        /// Verifies that only selected lines are emitted as changes.
        /// </summary>
        [Test]
        public void BuildSelectedPatch_PartialSelection_ExcludesUnselectedAddition()
        {
            const string patch = "diff --git a/a.txt b/a.txt\n--- a/a.txt\n+++ b/a.txt\n@@ -1,2 +1,4 @@\n one\n+two\n+three\n four\n";
            var diff = GitDiffParser.Parse("a.txt", GitDiffSide.Unstaged, patch);
            diff.Hunks[0].Lines.Single(line => line.Content == "three").IsSelected = true;

            var selectedPatch = GitPatchBuilder.BuildSelectedPatch(diff);

            StringAssert.Contains("+three", selectedPatch);
            StringAssert.DoesNotContain("+two", selectedPatch);
        }

        /// <summary>
        /// Verifies that selecting every changed line preserves Git's original patch exactly.
        /// </summary>
        [Test]
        public void BuildSelectedPatch_AllLinesSelected_ReturnsOriginalPatch()
        {
            const string patch = "diff --git a/a.txt b/a.txt\n--- a/a.txt\n+++ b/a.txt\n@@ -1 +1 @@\n-before\n+after\n";
            var diff = GitDiffParser.Parse("a.txt", GitDiffSide.Unstaged, patch);
            foreach (var line in diff.Hunks.SelectMany(hunk => hunk.Lines).Where(line => line.IsSelectable))
            {
                line.IsSelected = true;
            }

            var selectedPatch = GitPatchBuilder.BuildSelectedPatch(diff);

            Assert.That(selectedPatch, Is.EqualTo(patch));
        }

        /// <summary>
        /// Verifies that separated selections become independent zero-context hunks with stable coordinates.
        /// </summary>
        [Test]
        public void BuildSelectedPatch_SeparatedSelections_CreatesIndependentHunks()
        {
            const string patch = "diff --git a/a.txt b/a.txt\n--- a/a.txt\n+++ b/a.txt\n@@ -1,5 +1,5 @@\n-old one\n+new one\n context\n-old two\n+new two\n tail\n";
            var diff = GitDiffParser.Parse("a.txt", GitDiffSide.Unstaged, patch);
            var changedLines = diff.Hunks.SelectMany(hunk => hunk.Lines).Where(line => line.IsSelectable).ToList();
            changedLines[0].IsSelected = true;
            changedLines[1].IsSelected = true;
            changedLines[2].IsSelected = true;

            var selectedPatch = GitPatchBuilder.BuildSelectedPatch(diff);

            StringAssert.Contains("@@ -1,1 +1,1 @@", selectedPatch);
            StringAssert.Contains("@@ -3,1 +2,0 @@", selectedPatch);
            StringAssert.DoesNotContain("\r", selectedPatch);
        }
    }
}
