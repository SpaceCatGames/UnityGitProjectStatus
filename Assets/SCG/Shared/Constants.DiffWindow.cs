namespace SCG.UnityGitStatus
{
    internal static partial class Constants
    {
        #region Diff Window EditorPrefs

        /// <summary>EditorPrefs key storing the width of the diff panel.</summary>
        public const string DiffPanelWidthKey = "SCG.UnityGitStatus.DiffPanelWidth";

        #endregion

        #region Diff Window Text

        /// <summary>Title shown above staged changes.</summary>
        public const string StagedDiffLabel = "Staged (HEAD -> index)";

        /// <summary>Title shown above unstaged changes.</summary>
        public const string UnstagedDiffLabel = "Unstaged (index -> working tree)";

        /// <summary>Label of the button that reloads the selected diff.</summary>
        public const string ReloadDiffButtonText = "Reload";

        /// <summary>Label of the column containing line numbers before the change.</summary>
        public const string BeforeDiffColumnLabel = "Before";

        /// <summary>Label of the column containing line numbers after the change.</summary>
        public const string AfterDiffColumnLabel = "After";

        /// <summary>Prefix shown before an added diff line.</summary>
        public const string AddedDiffLinePrefix = "+ ";

        /// <summary>Prefix shown before a removed diff line.</summary>
        public const string RemovedDiffLinePrefix = "- ";

        /// <summary>Prefix shown before an unchanged or metadata diff line.</summary>
        public const string UnchangedDiffLinePrefix = "  ";

        /// <summary>Label of the button that unstages selected lines.</summary>
        public const string UnstageSelectedButtonText = "Unstage selected";

        /// <summary>Label of the button that stages selected lines.</summary>
        public const string StageSelectedButtonText = "Stage selected";

        /// <summary>Label of the button that reverts selected lines.</summary>
        public const string RevertSelectedButtonText = "Revert selected";

        /// <summary>Label of the button that unstages an entire file.</summary>
        public const string UnstageFileButtonText = "Unstage file";

        /// <summary>Label of the button that stages an entire file.</summary>
        public const string StageFileButtonText = "Stage file";

        /// <summary>Label of the button that reverts an entire file.</summary>
        public const string RevertFileButtonText = "Revert file";

        /// <summary>Message shown after a successful Git operation.</summary>
        public const string GitOperationCompletedMessage = "Git operation completed.";

        /// <summary>Title of the destructive revert confirmation dialog.</summary>
        public const string RevertConfirmationTitle = "Revert local changes?";

        /// <summary>Format of the destructive revert confirmation message.</summary>
        public const string RevertConfirmationMessageFormat =
            "This permanently discards {0} in '{1}'. This action cannot be undone.";

        /// <summary>Confirmation button label in the revert dialog.</summary>
        public const string RevertConfirmationButtonText = "Revert";

        /// <summary>Cancellation button label in the revert dialog.</summary>
        public const string RevertCancelButtonText = "Cancel";

        /// <summary>Description of the selected-lines revert target.</summary>
        public const string SelectedLinesRevertTarget = "selected lines";

        /// <summary>Description of the whole-file revert target.</summary>
        public const string EntireFileRevertTarget = "the entire file";

        /// <summary>Message shown when a binary diff supports only whole-file actions.</summary>
        public const string BinaryWholeFileDescription =
            "Binary diff. Only whole-file actions are available.";

        /// <summary>Message shown when a conflicted path cannot use two-way line actions.</summary>
        public const string ConflictedWholeFileDescription =
            "Conflicted file. Resolve it manually, then stage the whole file.";

        /// <summary>Message shown when a rename or copy diff supports only whole-file actions.</summary>
        public const string RenameOrCopyWholeFileDescription =
            "Rename or copy diff. Only whole-file actions are available.";

        /// <summary>Message shown when a new file supports only whole-file actions.</summary>
        public const string NewFileWholeFileDescription =
            "New file. Only whole-file actions are available.";

        /// <summary>Message shown when a deleted file supports only whole-file actions.</summary>
        public const string DeletedFileWholeFileDescription =
            "Deleted file. Only whole-file actions are available.";

        /// <summary>Fallback message shown when no line-level diff is available.</summary>
        public const string NoLineLevelDiffDescription =
            "No line-level diff is available. Whole-file actions are available.";

        #endregion
    }
}
