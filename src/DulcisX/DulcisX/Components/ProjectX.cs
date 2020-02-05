﻿using DulcisX.Core.Extensions;
using DulcisX.Core.Models;
using DulcisX.Core.Models.Enums;
using DulcisX.Helpers;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace DulcisX.Components
{
    public class ProjectX : HierarchyItemX
    {
        private Guid _underlyingGuid = Guid.Empty;

        public Guid UnderlyingGuid
        {
            get
            {
                if (_underlyingGuid == Guid.Empty)
                {
                    ThreadHelper.ThrowIfNotOnUIThread();

                    var result = ParentSolution.UnderlyingSolution.GetGuidOfProject(UnderlyingHierarchy, out _underlyingGuid);

                    VsHelper.ValidateSuccessStatusCode(result);
                }

                return _underlyingGuid;
            }
        }

        public __VSPROJOUTPUTTYPE OutputType
        {
            get => (__VSPROJOUTPUTTYPE)UnderlyingHierarchy.GetProperty(ItemId, (int)__VSHPROPID5.VSHPROPID_OutputType);
            set => SetProperty((int)__VSHPROPID5.VSHPROPID_OutputType, (uint)(int)value);
        }

        internal ProjectX(IVsHierarchy hierarchy, uint itemId, SolutionX solution) : base(hierarchy, itemId, HierarchyItemTypeX.Project, ConstructorInstance.FromValue(solution), ConstructorInstance.Empty<ProjectX>())
        {

        }
    }
}
