﻿namespace DulcisX.Core.Enums.VisualStudio
{
    /// <summary>
    /// Common stream keys in the .suo file. Passed to <see cref="Nodes.SolutionNode.GetUserConfiguration(Microsoft.VisualStudio.Shell.Interop.IVsPersistSolutionOpts, string)"/>.
    /// </summary>
    public static class CommonStreamKeys
    {
        /// <summary>
        /// Specifies the solution configuration strean inside the .suo file.
        /// </summary>
        public const string SolutionConfiguration = "SolutionConfiguration";
    }
}
