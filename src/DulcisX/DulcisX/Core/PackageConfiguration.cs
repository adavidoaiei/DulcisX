﻿using DulcisX.Core.Extensions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SimpleInjector;

namespace DulcisX.Core
{
    internal sealed class PackageConfiguration : IContainerConfiguration
    {
        public void ConfigureServices(PackageX package, Container container)
        {
            container.RegisterSingleton(() => package.GetService<SComponentModel, IComponentModel>());

            container.RegisterSingleton(() =>
            {
                var componentModel = container.GetInstance<IComponentModel>();

                return componentModel.GetService<IVsHierarchyItemManager>();
            });

            container.RegisterSingleton(() =>
            {
                var componentModel = container.GetInstance<IComponentModel>();

                return componentModel.GetService<IVsHierarchyItemCollectionProvider>();
            });

            container.RegisterSingleton(() =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                var shell = container.GetCOMInstance<IVsShell>();

                var result = shell.GetProperty((int)__VSSPROPID7.VSSPROPID_MainWindowInfoBarHost, out var infoBarHost);

                ErrorHandler.ThrowOnFailure(result);

                return new ComContainer<IVsInfoBarHost>((IVsInfoBarHost)infoBarHost);
            });

            container.RegisterCOMInstance<SVsSolution, IVsSolution>(package);
            container.RegisterCOMInstance<SVsSolutionPersistence, IVsSolutionPersistence>(package);
            container.RegisterCOMInstance<SVsSolutionBuildManager, IVsSolutionBuildManager>(package);
            container.RegisterCOMInstance<SVsRunningDocumentTable, IVsRunningDocumentTable>(package);
            container.RegisterCOMInstance<SVsTrackProjectDocuments, IVsTrackProjectDocuments2>(package);
            container.RegisterCOMInstance<SVsShellMonitorSelection, IVsMonitorSelection>(package);
            container.RegisterCOMInstance<SVsStatusbar, IVsStatusbar>(package);
            container.RegisterCOMInstance<SVsInfoBarUIFactory, IVsInfoBarUIFactory>(package);
            container.RegisterCOMInstance<SVsShell, IVsShell>(package);
            container.RegisterCOMInstance<SVsWebBrowsingService, IVsWebBrowsingService>(package);
            container.RegisterCOMInstance<SVsUIShell, IVsUIShell>(package);

            container.RegisterSingleton(() => (IServiceProviders)package);
        }
    }
}
