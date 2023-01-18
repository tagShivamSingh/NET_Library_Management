﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Web.LibraryManager.Cache;
using Microsoft.Web.LibraryManager.Contracts;
using Microsoft.Web.LibraryManager.Providers.Cdnjs;
using Microsoft.Web.LibraryManager.Providers.FileSystem;
using Microsoft.Web.LibraryManager.Providers.jsDelivr;
using Microsoft.Web.LibraryManager.Providers.Unpkg;

namespace Microsoft.Web.LibraryManager.Build.Contracts
{
    internal class Dependencies : IDependencies
    {
        private readonly IHostInteraction _hostInteraction;
        private readonly List<IProvider> _providers = new List<IProvider>();
        private readonly IEnumerable<string> _assemblyPaths;

        private Dependencies(IHostInteraction hostInteraction, IEnumerable<string> assemblyPaths)
        {
            _hostInteraction = hostInteraction;
            _assemblyPaths = assemblyPaths;
            Initialize();
        }

        public IHostInteraction GetHostInteractions() => _hostInteraction;

        public static Dependencies FromTask(string workingDirectory, IEnumerable<string> assemblyPaths)
        {
            var hostInteraction = new HostInteraction(workingDirectory);
            return new Dependencies(hostInteraction, assemblyPaths);
        }

        public IReadOnlyList<IProvider> Providers => _providers;

        public IProvider GetProvider(string providerId)
        {
            return _providers?.FirstOrDefault(p => p.Id.Equals(providerId, StringComparison.OrdinalIgnoreCase));
        }

        private void Initialize()
        {
            if (_providers.Count > 0)
                return;

            var packageInfoFactory = new NpmPackageInfoFactory(WebRequestHandler.Instance);
            var packageSearch = new NpmPackageSearch(WebRequestHandler.Instance);

            IEnumerable<IProviderFactory> factories = new IProviderFactory[] {
                new FileSystemProviderFactory(),
                new CdnjsProviderFactory(),
                new UnpkgProviderFactory(packageSearch, packageInfoFactory),
                new JsDelivrProviderFactory(packageSearch, packageInfoFactory),
            };

            foreach (IProviderFactory factory in factories)
            {
                if (factory != null)
                {
                    _providers.Add(factory.CreateProvider(_hostInteraction));
                }
            }
        }
    }
}
