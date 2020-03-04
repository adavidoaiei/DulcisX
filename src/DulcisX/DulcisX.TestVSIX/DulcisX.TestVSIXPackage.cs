﻿using System.Linq;
using System.Runtime.InteropServices;
using DulcisX.Core;
using DulcisX.Core.Models.Enums;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace DulcisX.TestVSIX
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class DulcisXTestVSIXPackage : PackageX
    {
        public const string PackageGuidString = "a7c50965-01fc-4668-9b93-c14bad2dbe25";

        #region Package Members

        public DulcisXTestVSIXPackage()
        {
            OnInitializeAsync += DulcisXTestVSIXPackage_OnInitializeAsync;
        }

        private async System.Threading.Tasks.Task DulcisXTestVSIXPackage_OnInitializeAsync(System.Threading.CancellationToken arg)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(arg);

            GetSolution().SolutionEvents.OnBeforeProjectUnload.Hook(NodeTypes.All, (projectOld, projectNew) =>
            {
                var test = projectOld.GetDisplayName();
                var test2 = projectNew.GetDisplayName();
                var test3 = projectNew.IsLoaded();
            });
        }

        #endregion
    }
}