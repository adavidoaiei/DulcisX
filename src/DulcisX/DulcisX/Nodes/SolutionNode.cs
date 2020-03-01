﻿using DulcisX.Core.Extensions;
using DulcisX.Core.Models.Enums;
using DulcisX.Core.Models.Enums.VisualStudio;
using DulcisX.Core.Models.PackageUserOptions;
using DulcisX.Helpers;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SimpleInjector;
using System;
using System.Collections.Generic;

namespace DulcisX.Nodes
{
    public class SolutionNode : BaseNode, IPhysicalNode
    {
        public IVsSolution UnderlyingSolution { get; }

        public override NodeTypes NodeType => NodeTypes.Solution;

        public override SolutionNode ParentSolution => this;

        public Container ServiceContainer { get; }

        public SolutionNode(IVsSolution solution, Container container) : base(null, (IVsHierarchy)solution, CommonNodeIds.Solution)
        {
            UnderlyingSolution = solution;
            ServiceContainer = container;
        }

        public string GetFullName()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var result = UnderlyingSolution.GetProperty((int)__VSPROPID.VSPROPID_SolutionFileName, out var fullName);

            ErrorHandler.ThrowOnFailure(result);

            return (string)fullName;
        }

        public override BaseNode GetParent()
            => null;

        public override BaseNode GetParent(NodeTypes nodeType)
            => null;

        public override IEnumerable<BaseNode> GetChildren()
        {
            var node = HierarchyUtilities.GetFirstChild(UnderlyingHierarchy, ItemId, true);

            do
            {
                if (VsHelper.IsItemIdNil(node))
                {
                    yield break;
                }

                if (UnderlyingHierarchy.TryGetNestedHierarchy(node, out var nestedHierarchy))
                {
                    yield return NodeFactory.GetSolutionItemNode(ParentSolution, nestedHierarchy, CommonNodeIds.Root);
                }

                node = HierarchyUtilities.GetNextSibling(UnderlyingHierarchy, node, true);
            }
            while (true);
        }

        public ProjectNode GetProject(string uniqueName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var result = UnderlyingSolution.GetProjectOfUniqueName(uniqueName, out var hierarchy);
            ErrorHandler.ThrowOnFailure(result);

            return new ProjectNode(this, hierarchy);
        }

        public ProjectNode GetProject(Guid projectGuid)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var result = UnderlyingSolution.GetProjectOfGuid(projectGuid, out var hierarchy);
            ErrorHandler.ThrowOnFailure(result);

            return new ProjectNode(this, hierarchy);
        }

        public ProjectNode GetProject(IVsHierarchy hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            return new ProjectNode(this, hierarchy);
        }

        public IEnumerable<(ProjectNode Project, StartupOptions Options)> GetStartupProjects()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var solutionConfiguration = new SolutionConfigurationOptions();

            GetUserConfiguration(solutionConfiguration, CommonStreamKeys.SolutionConfiguration);

            if (solutionConfiguration.IsMultiStartup)
            {
                foreach (var startupProject in solutionConfiguration.StartupProjects)
                {
                    var project = GetProject(startupProject.Key);

                    yield return (project, startupProject.Value);
                }
            }
            else
            {
                var solutionBuildManager = ServiceContainer.GetCOMInstance<IVsSolutionBuildManager>();

                var result = solutionBuildManager.get_StartupProject(out var hierarchy);

                yield return (GetProject(hierarchy), StartupOptions.Start);

                ErrorHandler.ThrowOnFailure(result);
            }
        }

        public void GetUserConfiguration(IVsPersistSolutionOpts persistanceSolutionOptions, string streamKey)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var solutionPersistance = ServiceContainer.GetCOMInstance<IVsSolutionPersistence>();

            var result = solutionPersistance.LoadPackageUserOpts(persistanceSolutionOptions, streamKey);

            ErrorHandler.ThrowOnFailure(result);
        }
    }
}
