﻿using DulcisX.Core.Models.Interfaces;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using SimpleInjector;
using StringyEnums;
using System;
using System.Reflection;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace DulcisX.Core
{
    public abstract class PackageX : AsyncPackage, IServiceProviders
    {
        public event Func<CancellationToken, Task> OnInitializeAsync;

        public Container ServiceContainer { get; }

        private readonly Action<Container> _consumerServices;

        #region Constructors

        protected PackageX()
        {
            ServiceContainer = new Container();
        }

        protected PackageX(Action<Container> configuration) : this()
        {
            _consumerServices = configuration;
        }

        static PackageX()
        {
            EnumCore.Init(initializer => initializer.InitWith(Assembly.GetExecutingAssembly()), false);
        }

        #endregion

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            AddDefaultServices(ServiceContainer);

            _consumerServices?.Invoke(ServiceContainer);

            ServiceContainer.Verify();

            await OnInitializeAsync?.Invoke(cancellationToken);

            await base.InitializeAsync(cancellationToken, progress);
        }

        private void AddDefaultServices(Container container)
        {
            container.RegisterSingleton(() =>
            {
                return this.GetService<SComponentModel, IComponentModel>();
            });

            container.RegisterSingleton(() =>
            {
                var componentModel = container.GetInstance<IComponentModel>();

                return componentModel.GetService<IVsHierarchyItemManager>();
            });
        }

        #region Services

        public TService GetGlobalService<TService>() where TService : class
            => GetGlobalService<TService, TService>();

        public TService GetGlobalService<TBaseService, TService>() where TBaseService : class
                                                                   where TService : class
            => GetGlobalService(typeof(TBaseService)) as TService;

        private IServiceProviders GetServiceProviders()
            => (IServiceProviders)this;

        #endregion
    }
}
