﻿using System;

namespace DulcisX.Nodes.Events
{
    public delegate void QueryProjectClose(ProjectNode project, bool isRemoving, ref bool shouldCancel);
    public delegate void QueryProjectUnload(ProjectNode project, ref bool shouldCancel);
    public delegate void QuerySolutionClose(ref bool shouldCancel);

    public interface ISolutionEvents
    {
        EventDistributor<Action<ProjectNode, bool>> OnAfterProjectOpen { get; }

        EventDistributor<QueryProjectClose> OnQueryProjectClose { get; }

        EventDistributor<Action<ProjectNode, bool>> OnBeforeProjectClose { get; }

        EventDistributor<Action<ProjectNode, ProjectNode>> OnAfterProjectLoad { get; }

        EventDistributor<QueryProjectUnload> OnQueryProjectUnload { get; }

        EventDistributor<Action<ProjectNode, ProjectNode>> OnBeforeProjectUnload { get; }

        event Action<ProjectNode> OnProjectAdd;

        event Action<ProjectNode> OnProjectRemove;

        event Action<bool> OnAfterSolutionOpen;

        event QuerySolutionClose OnQuerySolutionClose;

        event Action OnBeforeSolutionClose;

        event Action OnAfterSolutionClose;
    }
}
