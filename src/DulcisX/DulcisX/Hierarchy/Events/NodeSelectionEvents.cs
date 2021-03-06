﻿using DulcisX.Core.Enums;
using DulcisX.Core.Extensions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;

namespace DulcisX.Hierarchy.Events
{
    internal class NodeSelectionEvents : NodeEventSink, INodeSelectionEvents, IVsSelectionEvents
    {
        #region Events

        public event Action<IEnumerable<BaseNode>> OnSelected;

        #endregion

        private readonly IVsMonitorSelection _monitorSelection;

        private NodeSelectionEvents(SolutionNode solution, IVsMonitorSelection monitorSelection) : base(solution)
        {
            _monitorSelection = monitorSelection;
        }

        public int OnSelectionChanged(IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMISOld, ISelectionContainer pSCOld, IVsHierarchy pHierNew, uint itemidNew, IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew)
        {
            OnSelected?.Invoke(SelectedNodesCollection.GetSelection(pMISNew, pHierNew, itemidNew, Solution).ToCachingEnumerable());

            return CommonStatusCodes.Success;
        }

        public int OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
        {
            return CommonStatusCodes.NotImplemented;
        }

        public int OnCmdUIContextChanged(uint dwCmdUICookie, int fActive)
        {
            return CommonStatusCodes.NotImplemented;
        }

        internal static INodeSelectionEvents Create(SolutionNode solution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var monitorSelection = solution.ServiceContainer.GetCOMInstance<IVsMonitorSelection>();

            var nodeSelectionEvent = new NodeSelectionEvents(solution, monitorSelection);

            var result = monitorSelection.AdviseSelectionEvents(nodeSelectionEvent, out var cookieUID);

            ErrorHandler.ThrowOnFailure(result);

            nodeSelectionEvent.SetCookie(cookieUID);

            return nodeSelectionEvent;
        }

        public override void Dispose()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var result = _monitorSelection.UnadviseSelectionEvents(Cookie);

            ErrorHandler.ThrowOnFailure(result);
        }
    }
}
