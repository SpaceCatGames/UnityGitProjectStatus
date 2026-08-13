using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SCG.UnityGitStatus
{
    /// <summary>
    /// Runs Git commands needed by the package without relying on shell quoting.
    /// The runner resolves the repository root, reads porcelain status output, and resolves the current branch.
    /// All commands execute from an explicit working directory to support project paths with spaces.
    /// </summary>
    internal static class GitStatusRunner
    {
        #region Constants

        private const string ResolveRepositoryRootArguments = "rev-parse --show-toplevel";
        private const string ReadRepositoryStatusArguments = "--no-optional-locks status --porcelain=v1 -z --untracked-files=all";
        private const string ReadCurrentBranchArguments = "--no-optional-locks branch --show-current";

        #endregion

        #region Public API

        /// <summary>
        /// Runs the Git refresh workflow asynchronously for a Unity project root.
        /// The workflow resolves the repository root first and then executes the porcelain status command.
        /// When status succeeds, the runner also tries to resolve the current branch name.
        /// </summary>
        /// <param name="projectRoot">Absolute Unity project root path.</param>
        /// <param name="timeoutMilliseconds">Timeout to apply to Git commands.</param>
        /// <returns>A task producing the raw Git refresh result.</returns>
        internal static Task<GitStatusRunResult> RunAsync(string projectRoot, int timeoutMilliseconds)
        {
            var safeTimeout = timeoutMilliseconds > 0
                ? timeoutMilliseconds
                : Constants.GitCommandTimeoutMilliseconds;

            return Task.Run(() =>
            {
                var repositoryRoot = RunGitCommand(projectRoot, ResolveRepositoryRootArguments, safeTimeout);
                var result = new GitStatusRunResult
                {
                    RepositoryRoot = string.Empty,
                    ProjectPathInRepository = string.Empty,
                    StatusOutput = string.Empty,
                    StatusError = repositoryRoot.ErrorOutput,
                    StatusExitCode = repositoryRoot.ExitCode,
                    TimedOut = repositoryRoot.TimedOut,
                    Error = repositoryRoot.ExceptionMessage,
                    Branch = string.Empty
                };

                if (repositoryRoot.ExitCode != 0 || repositoryRoot.TimedOut || !string.IsNullOrEmpty(repositoryRoot.ExceptionMessage))
                    return result;

                var repositoryRootPath = FirstLine(repositoryRoot.Output);

                if (string.IsNullOrEmpty(repositoryRootPath))
                {
                    result.Error = "Git repository root could not be resolved.";
                    return result;
                }

                var projectPathInRepository = GetProjectPathInRepository(repositoryRootPath, projectRoot);
                var status = RunGitCommand(
                    repositoryRootPath,
                    ReadRepositoryStatusArguments,
                    safeTimeout);

                result.RepositoryRoot = repositoryRootPath;
                result.ProjectPathInRepository = projectPathInRepository;
                result.StatusOutput = status.Output;
                result.StatusError = status.ErrorOutput;
                result.StatusExitCode = status.ExitCode;
                result.TimedOut = status.TimedOut;
                result.Error = status.ExceptionMessage;

                if (status.ExitCode != 0 || status.TimedOut || !string.IsNullOrEmpty(status.ExceptionMessage))
                    return result;

                var branch = RunGitCommand(
                    repositoryRootPath,
                    ReadCurrentBranchArguments,
                    Math.Min(safeTimeout, Constants.BranchCommandTimeoutMilliseconds));

                if (branch.ExitCode == 0 && !branch.TimedOut && string.IsNullOrEmpty(branch.ExceptionMessage))
                {
                    result.Branch = (branch.Output ?? string.Empty).Trim();
                }

                return result;
            });
        }

        #endregion

        #region Helpers

        private static string FirstLine(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
            var lineBreak = normalized.IndexOf('\n');
            return (lineBreak >= 0 ? normalized[..lineBreak] : normalized).Trim();
        }

        private static string GetProjectPathInRepository(string repositoryRoot, string projectRoot)
        {
            if (string.IsNullOrEmpty(repositoryRoot) || string.IsNullOrEmpty(projectRoot))
                return string.Empty;

            try
            {
                var fullRepositoryRoot = EnsureTrailingSeparator(Path.GetFullPath(repositoryRoot));
                var fullProjectRoot = Path.GetFullPath(projectRoot)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var trimmedRepositoryRoot = fullRepositoryRoot
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                if (GitPathComparer.AreEqual(fullProjectRoot, trimmedRepositoryRoot))
                {
                    return string.Empty;
                }

                return GitPathComparer.StartsWith(fullProjectRoot, fullRepositoryRoot)
                    ? fullProjectRoot[fullRepositoryRoot.Length..]
                        .Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                        .Replace('\\', '/')
                    : string.Empty;
            }
            catch (ArgumentException)
            {
                return string.Empty;
            }
            catch (IOException)
            {
                return string.Empty;
            }
            catch (NotSupportedException)
            {
                return string.Empty;
            }
            catch (UnauthorizedAccessException)
            {
                return string.Empty;
            }
        }

        private static string EnsureTrailingSeparator(string path) =>
            string.IsNullOrEmpty(path)
                ? string.Empty
                : path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ||
                  path.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                    ? path
                    : path + Path.DirectorySeparatorChar;

        private static GitCommandResult RunGitCommand(string workingDirectory, string arguments, int timeoutMilliseconds)
        {
            var result = new GitCommandResult
            {
                ExitCode = -1,
                Output = string.Empty,
                ErrorOutput = string.Empty,
                ExceptionMessage = string.Empty
            };

            try
            {
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = Constants.GitExecutableName,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                process.Start();

                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                if (!process.WaitForExit(timeoutMilliseconds))
                {
                    result.TimedOut = true;

                    try
                    {
                        process.Kill();
                    }
                    catch (InvalidOperationException)
                    {
                    }

                    return result;
                }

                process.WaitForExit();
                result.Output = outputTask.GetAwaiter().GetResult();
                result.ErrorOutput = errorTask.GetAwaiter().GetResult();
                result.ExitCode = process.ExitCode;
            }
            catch (InvalidOperationException exception)
            {
                result.ExceptionMessage = exception.Message;
            }
            catch (IOException exception)
            {
                result.ExceptionMessage = exception.Message;
            }
            catch (UnauthorizedAccessException exception)
            {
                result.ExceptionMessage = exception.Message;
            }
            catch (Win32Exception exception)
            {
                result.ExceptionMessage = exception.Message;
            }

            return result;
        }

        #endregion

        #region Nested Types

        private sealed class GitCommandResult
        {
            /// <summary>Standard output captured from the Git process.</summary>
            public string Output { get; set; }

            /// <summary>Standard error captured from the Git process.</summary>
            public string ErrorOutput { get; set; }

            /// <summary>Exit code returned by the Git process.</summary>
            public int ExitCode { get; set; }

            /// <summary>Whether the process exceeded the configured timeout.</summary>
            public bool TimedOut { get; set; }

            /// <summary>Process-level exception message captured before Git could return normally.</summary>
            public string ExceptionMessage { get; set; }
        }

        #endregion
    }
}
