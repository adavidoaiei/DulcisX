﻿using DulcisX.Core.Models;
using DulcisX.Core.Models.Enums;
using Microsoft.VisualStudio.Shell.Interop;
using StringyEnums;

namespace DulcisX.Components
{
    public class DocumentX : HierarchyItemX
    {
        public string BuildAction
        {
            get => GetProperty<string>((int)__VSHPROPID4.VSHPROPID_BuildAction);
            set => SetProperty((int)__VSHPROPID4.VSHPROPID_BuildAction, value);
        }

        public CopyToOutputDirectoryX CopyToOutputDirectory
        {
            get
            {
                var val = ParentProject.GetItemProperty(ItemId, DocumentPropertyX.CopyToOutputDirectory);

                return val.GetEnumFromRepresentation<CopyToOutputDirectoryX>();
            }
        }

        internal DocumentX(IVsHierarchy hierarchy, uint itemId, SolutionX solution, ProjectX project = default, HierarchyItemX parentItem = default) : base(hierarchy, itemId, HierarchyItemTypeX.Document, ConstructorInstance.FromValue(solution), ConstructorInstance.FromValue(project), parentItem)
        {

        }
    }
}
