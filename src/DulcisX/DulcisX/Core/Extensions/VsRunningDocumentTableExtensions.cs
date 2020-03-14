﻿using DulcisX.Nodes;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace DulcisX.Core.Extensions
{
    public static class VsRunningDocumentTableExtensions
    {
        public static IPhysicalNode GetNode(this IVsRunningDocumentTable rdt, uint docCookie, SolutionNode solution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var result = rdt.GetDocumentInfo(docCookie, out _, out _, out _, out _, out var hierarchy, out uint itemId, out _);

            ErrorHandler.ThrowOnFailure(result);

            return (IPhysicalNode)NodeFactory.GetItemNode(solution, hierarchy, itemId);
        }
    }
}