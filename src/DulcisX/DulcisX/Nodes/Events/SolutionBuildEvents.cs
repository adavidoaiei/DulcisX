﻿using DulcisX.Core.Extensions;
using DulcisX.Core.Models.Enums.VisualStudio;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace DulcisX.Nodes.Events
{
    internal class SolutionBuildEvents : EventSink, ISolutionBuildEvents, IVsUpdateSolutionEvents
    {
        #region Events

        private EventDistributor<Action<ProjectNode>> _onAfterProjectConfigurationChange;
        public EventDistributor<Action<ProjectNode>> OnAfterProjectConfigurationChange
            => _onAfterProjectConfigurationChange ?? (_onAfterProjectConfigurationChange = new EventDistributor<Action<ProjectNode>>());

        public event BeforeSolutionBuild OnBeforeSolutionBuild;
        public event Action<bool, bool, bool> OnAfterSolutionBuild;
        public event Action OnSolutionBuildCancel;
        public event BeforeProjectConfigurationBuild OnBeforeProjectConfigurationBuild;

        #endregion

        private readonly IVsSolutionBuildManager _buildManager;

        private SolutionBuildEvents(SolutionNode solution, IVsSolutionBuildManager buildManager) : base(solution)
        {
            _buildManager = buildManager;
        }

        public int UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            bool tempBool = false;

            OnBeforeSolutionBuild?.Invoke(ref tempBool);

            pfCancelUpdate = tempBool ? 1 : 0;

            return CommonStatusCodes.Success;
        }

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            OnAfterSolutionBuild?.Invoke(fSucceeded == 1, fModified == 1, fCancelCommand == 1);

            return CommonStatusCodes.Success;
        }

        public int UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {
            bool tempBool = false;

            OnBeforeProjectConfigurationBuild?.Invoke(ref tempBool);

            pfCancelUpdate = tempBool ? 1 : 0;

            return CommonStatusCodes.Success;
        }

        public int UpdateSolution_Cancel()
        {
            OnSolutionBuildCancel?.Invoke();

            return CommonStatusCodes.Success;
        }

        public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            var project = Solution.GetProject(pIVsHierarchy);

            _onAfterProjectConfigurationChange?.Invoke(project.NodeType, project);

            return CommonStatusCodes.Success;
        }

        internal static ISolutionBuildEvents Create(SolutionNode solution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var buildManager = solution.ServiceContainer.GetCOMInstance<IVsSolutionBuildManager>();

            var solutionBuildEvents = new SolutionBuildEvents(solution, buildManager);

            var result = buildManager.AdviseUpdateSolutionEvents(solutionBuildEvents, out var cookieUID);

            ErrorHandler.ThrowOnFailure(result);

            solutionBuildEvents.SetCookie(cookieUID);

            return solutionBuildEvents;
        }

        public override void Dispose()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var result = _buildManager.UnadviseUpdateSolutionEvents(Cookie);

            ErrorHandler.ThrowOnFailure(result);
        }

    }
}