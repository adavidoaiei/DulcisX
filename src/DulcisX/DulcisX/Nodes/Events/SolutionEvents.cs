﻿using DulcisX.Core.Models.Enums.VisualStudio;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace DulcisX.Nodes.Events
{
    internal class SolutionEvents : EventSink, ISolutionEvents, IVsSolutionEvents, IVsSolutionEvents5
    {
        #region Events

        private EventDistributor<Action<ProjectNode, bool>> _onAfterProjectOpen;
        public EventDistributor<Action<ProjectNode, bool>> OnAfterProjectOpen
            => _onAfterProjectOpen ?? (_onAfterProjectOpen = new EventDistributor<Action<ProjectNode, bool>>());

        private EventDistributor<QueryProjectClose> _onQueryProjectClose;
        public EventDistributor<QueryProjectClose> OnQueryProjectClose
             => _onQueryProjectClose ?? (_onQueryProjectClose = new EventDistributor<QueryProjectClose>());

        private EventDistributor<Action<ProjectNode, bool>> _onBeforeProjectClose;
        public EventDistributor<Action<ProjectNode, bool>> OnBeforeProjectClose
             => _onBeforeProjectClose ?? (_onBeforeProjectClose = new EventDistributor<Action<ProjectNode, bool>>());

        private EventDistributor<Action<ProjectNode, ProjectNode>> _onAfterProjectLoad;
        public EventDistributor<Action<ProjectNode, ProjectNode>> OnAfterProjectLoad
             => _onAfterProjectLoad ?? (_onAfterProjectLoad = new EventDistributor<Action<ProjectNode, ProjectNode>>());

        private EventDistributor<QueryProjectUnload> _onQueryProjectUnload;
        public EventDistributor<QueryProjectUnload> OnQueryProjectUnload
             => _onQueryProjectUnload ?? (_onQueryProjectUnload = new EventDistributor<QueryProjectUnload>());

        private EventDistributor<Action<ProjectNode, ProjectNode>> _onBeforeProjectUnload;
        public EventDistributor<Action<ProjectNode, ProjectNode>> OnBeforeProjectUnload
             => _onBeforeProjectUnload ?? (_onBeforeProjectUnload = new EventDistributor<Action<ProjectNode, ProjectNode>>());


        public event Action<ProjectNode> OnProjectAdd;

        public event Action<ProjectNode> OnProjectRemove;

        public event Action<bool> OnAfterSolutionOpen;

        public event QuerySolutionClose OnQuerySolutionClose;

        public event Action OnBeforeSolutionClose;

        public event Action OnAfterSolutionClose;

        #endregion

        private Guid _lastProjectUnloaded = Guid.Empty;
        private string _lastProjectOpened = null;

        private SolutionEvents(SolutionNode solution) : base(solution)
        {

        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            var project = Solution.GetProject(pHierarchy);

            _onAfterProjectOpen?.Invoke(project.NodeType, project, fAdded == 1);

            if (_lastProjectOpened is object &&
                _lastProjectOpened == project.GetFullName())
            {
                OnProjectAdd?.Invoke(project);
            }

            return CommonStatusCodes.Success;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            bool tempBool = false;

            var project = Solution.GetProject(pHierarchy);

            _onQueryProjectClose?.Invoke(project.NodeType, fRemoving == 1, ref tempBool);

            pfCancel = tempBool ? 1 : 0;

            return CommonStatusCodes.Success;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            var project = Solution.GetProject(pHierarchy);

            _onBeforeProjectClose?.Invoke(project.NodeType, project, fRemoved == 1);

            if (OnProjectRemove is object &&
                _lastProjectUnloaded == project.GetGuid())
            {
                OnProjectRemove.Invoke(project);
            }

            return CommonStatusCodes.Success;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            var oldProject = Solution.GetProject(pStubHierarchy);
            var newProject = Solution.GetProject(pRealHierarchy);

            _onAfterProjectLoad?.Invoke(newProject.NodeType, oldProject, newProject);

            return CommonStatusCodes.Success;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            bool tempBool = false;

            var project = Solution.GetProject(pRealHierarchy);

            _onQueryProjectUnload?.Invoke(project.NodeType, project, ref tempBool);

            pfCancel = tempBool ? 1 : 0;

            return CommonStatusCodes.Success;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            var oldProject = Solution.GetProject(pRealHierarchy);
            var newProject = Solution.GetProject(pStubHierarchy);

            _onBeforeProjectUnload?.Invoke(newProject.NodeType, oldProject, newProject);

            if (OnProjectRemove is object)
            {
                _lastProjectUnloaded = newProject.GetGuid();
            }

            return CommonStatusCodes.Success;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            OnAfterSolutionOpen?.Invoke(fNewSolution == 1);

            return CommonStatusCodes.Success;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            bool tempBool = false;

            OnQuerySolutionClose?.Invoke(ref tempBool);

            pfCancel = tempBool ? 1 : 0;

            return CommonStatusCodes.Success;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            OnBeforeSolutionClose?.Invoke();

            return CommonStatusCodes.Success;
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            OnAfterSolutionClose?.Invoke();
            return CommonStatusCodes.Success;
        }

        internal static ISolutionEvents Create(SolutionNode solution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var solutionEvents = new SolutionEvents(solution);

            var result = solution.UnderlyingSolution.AdviseSolutionEvents(solutionEvents, out var cookieUID);

            ErrorHandler.ThrowOnFailure(result);

            solutionEvents.SetCookie(cookieUID);

            return solutionEvents;
        }

        public override void Dispose()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var result = Solution.UnderlyingSolution.UnadviseSolutionEvents(Cookie);

            ErrorHandler.ThrowOnFailure(result);
        }

        public void OnBeforeOpenProject(ref Guid guidProjectID, ref Guid guidProjectType, string pszFileName)
        {
            if (guidProjectType == VSConstants.CLSID.MiscellaneousFilesProject_guid ||
                guidProjectID != Guid.Empty)
            {
                return;
            }

            _lastProjectOpened = pszFileName;
        }
    }
}
