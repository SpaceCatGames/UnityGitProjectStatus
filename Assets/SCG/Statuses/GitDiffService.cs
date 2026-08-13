using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

namespace SCG.UnityGitStatus
{
    /// <summary>
    /// Loads file diffs and performs guarded index or working-tree mutations.
    /// </summary>
    internal static class GitDiffService
    {
        #region Constants

        private const string DiffArguments = "--no-optional-locks diff --no-ext-diff --no-color --unified=";
        private const string CachedArgument = " --cached";
        private const string PathSeparatorArgument = " -- ";
        private const string ApplyCheckArguments = "--no-optional-locks apply --check --recount --unidiff-zero";
        private const string ApplyArguments = "--no-optional-locks apply --recount --unidiff-zero";
        private const string CachedApplyArgument = " --cached";
        private const string ReverseApplyArgument = " --reverse";
        private const string StageFileArguments = "--no-optional-locks add -- ";
        private const string UnstageFileArguments = "--no-optional-locks reset -- ";
        private const string RevertFileArguments = "--no-optional-locks checkout -- ";
        private const string DeleteUntrackedFileArguments = "--no-optional-locks clean -f -- ";

        #endregion

        #region Diff API

        /// <summary>
        /// Loads one side of the diff for a project-relative path.
        /// </summary>
        /// <param name="path">The project-relative path.</param>
        /// <param name="repositoryPath">The path relative to the repository root.</param>
        /// <param name="side">The repository side to load.</param>
        /// <param name="kind">The status kind used for whole-file fallback metadata.</param>
        /// <returns>The parsed diff or an operation error.</returns>
        internal static (GitFileDiff Diff, string Error) Load(
            string path,
            string repositoryPath,
            GitDiffSide side,
            GitStatusKind kind)
        {
            var gitPath = string.IsNullOrEmpty(repositoryPath) ? path : repositoryPath;
            var arguments = DiffArguments + UnityGitStatusSettings.DiffContextLines +
                            (side == GitDiffSide.Staged ? CachedArgument : string.Empty) +
                            PathSeparatorArgument + Quote(gitPath);
            var result = Run(arguments, string.Empty);

            return !result.Success
                ? (null, result.Error)
                : kind == GitStatusKind.Conflicted
                    ? side == GitDiffSide.Staged
                        ? (null, string.Empty)
                        : (new GitFileDiff(path, side, null, null, false, false, false, false, true)
                        {
                            OriginalPatch = result.Output
                        }, string.Empty)
                    : !string.IsNullOrEmpty(result.Output)
                        ? (GitDiffParser.Parse(path, side, result.Output), string.Empty)
                        : side == GitDiffSide.Unstaged && kind == GitStatusKind.Untracked
                            ? (new GitFileDiff(path, side, null, null, false, true, false, false), string.Empty)
                            : (null, string.Empty);
        }

        #endregion

        #region Partial Operations

        /// <summary>
        /// Stages the selected lines from an unstaged diff.
        /// </summary>
        /// <param name="diff">The diff containing the selected lines.</param>
        /// <returns>The operation result.</returns>
        internal static GitOperationResult StageSelected(GitFileDiff diff) => ApplySelected(diff, true, false);

        /// <summary>
        /// Removes the selected lines from the index.
        /// </summary>
        /// <param name="diff">The staged diff containing the selected lines.</param>
        /// <returns>The operation result.</returns>
        internal static GitOperationResult UnstageSelected(GitFileDiff diff) => ApplySelected(diff, true, true);

        /// <summary>
        /// Reverts the selected working-tree lines.
        /// </summary>
        /// <param name="diff">The unstaged diff containing the selected lines.</param>
        /// <returns>The operation result.</returns>
        internal static GitOperationResult RevertSelected(GitFileDiff diff) => ApplySelected(diff, false, true);

        private static GitOperationResult ApplySelected(GitFileDiff diff, bool cached, bool reverse)
        {
            var patch = GitPatchBuilder.BuildSelectedPatch(diff);
            if (string.IsNullOrEmpty(patch)) return new GitOperationResult(false, string.Empty, "Select at least one changed line.");

            var options = (cached ? CachedApplyArgument : string.Empty) +
                          (reverse ? ReverseApplyArgument : string.Empty);
            var check = Run(ApplyCheckArguments + options, patch);
            return check.Success ? Run(ApplyArguments + options, patch) : check;
        }

        #endregion

        #region Whole-file Operations

        /// <summary>
        /// Stages an entire file.
        /// </summary>
        /// <param name="path">The project-relative path.</param>
        /// <returns>The operation result.</returns>
        internal static GitOperationResult StageFile(string path) =>
            Run(StageFileArguments + Quote(path), string.Empty);

        /// <summary>
        /// Removes an entire file from the index while retaining its working-tree state.
        /// </summary>
        /// <param name="path">The project-relative path.</param>
        /// <param name="originalPath">The original path of a rename or copy, or an empty string.</param>
        /// <returns>The operation result.</returns>
        internal static GitOperationResult UnstageFile(string path, string originalPath = "") =>
            Run(UnstageFileArguments + BuildPathArguments(path, originalPath), string.Empty);

        /// <summary>
        /// Restores an entire tracked file in the working tree.
        /// </summary>
        /// <param name="path">The project-relative path.</param>
        /// <returns>The operation result.</returns>
        internal static GitOperationResult RevertFile(string path) =>
            Run(RevertFileArguments + Quote(path), string.Empty);

        /// <summary>
        /// Deletes one untracked file through Git's path-scoped clean command.
        /// </summary>
        /// <param name="path">The project-relative path.</param>
        /// <returns>The operation result.</returns>
        internal static GitOperationResult DeleteUntrackedFile(string path) =>
            Run(DeleteUntrackedFileArguments + Quote(path), string.Empty);

        #endregion

        #region Process

        private static GitOperationResult Run(string arguments, string input)
        {
            try
            {
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = Constants.GitExecutableName,
                    Arguments = arguments,
                    WorkingDirectory = string.IsNullOrEmpty(GitStatusCache.RepositoryRoot)
                        ? Path.GetDirectoryName(Application.dataPath) ?? string.Empty
                        : GitStatusCache.RepositoryRoot,
                    UseShellExecute = false,
                    RedirectStandardInput = !string.IsNullOrEmpty(input),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                process.Start();

                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                if (!string.IsNullOrEmpty(input))
                {
                    process.StandardInput.Write(input);
                    process.StandardInput.Close();
                }

                if (!process.WaitForExit(Constants.GitCommandTimeoutMilliseconds))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (InvalidOperationException)
                    {
                    }

                    return new GitOperationResult(false, string.Empty, "Git operation timed out.");
                }

                process.WaitForExit();
                var output = outputTask.GetAwaiter().GetResult();
                var error = errorTask.GetAwaiter().GetResult();
                return process.ExitCode == 0
                    ? new GitOperationResult(true, output, string.Empty)
                    : new GitOperationResult(false, output, string.IsNullOrWhiteSpace(error) ? "Git operation failed." : error.Trim());
            }
            catch (InvalidOperationException exception)
            {
                return Failure(exception);
            }
            catch (IOException exception)
            {
                return Failure(exception);
            }
            catch (UnauthorizedAccessException exception)
            {
                return Failure(exception);
            }
            catch (Win32Exception exception)
            {
                return Failure(exception);
            }
        }

        private static GitOperationResult Failure(Exception exception) =>
            new(false, string.Empty, exception.Message);

        private static string BuildPathArguments(string path, string additionalPath)
        {
            var arguments = Quote(path);
            return string.IsNullOrEmpty(additionalPath) || GitPathComparer.AreEqual(path, additionalPath)
                ? arguments
                : arguments + " " + Quote(additionalPath);
        }

        private static string Quote(string value) => "\"" + (value ?? string.Empty).Replace("\"", "\\\"") + "\"";

        #endregion
    }
}
