﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Roslynator.Mef
{
    internal class MefServices
    {
        private static MefServices _default;
        private static ImmutableArray<Assembly> _defaultAssemblies;

        private readonly CompositionContext _compositionContext;

        public MefServices(CompositionContext compositionContext)
        {
            _compositionContext = compositionContext;
        }

        public static MefServices Default
        {
            get
            {
                if (_default == null)
                {
                    MefServices services = Create(DefaultAssemblies);
                    Interlocked.CompareExchange(ref _default, services, null);
                }

                return _default;
            }
        }

        public static ImmutableArray<Assembly> DefaultAssemblies
        {
            get
            {
                if (_defaultAssemblies.IsDefault)
                    ImmutableInterlocked.InterlockedInitialize(ref _defaultAssemblies, LoadDefaultAssemblies());

                return _defaultAssemblies;
            }
        }

        public static MefServices Create(CompositionContext compositionContext)
        {
            if (compositionContext == null)
                throw new ArgumentNullException(nameof(compositionContext));

            return new MefServices(compositionContext);
        }

        public static MefServices Create(IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null)
                throw new ArgumentNullException(nameof(assemblies));

            ContainerConfiguration compositionConfiguration = new ContainerConfiguration().WithAssemblies(assemblies);

            CompositionHost container = compositionConfiguration.CreateContainer();

            return new MefServices(container);
        }

        private static ImmutableArray<Assembly> LoadDefaultAssemblies()
        {
            return GetAssemblyNames()
                .Select(f => TryLoadAssembly(f))
                .Where(f => f != null)
                .ToImmutableArray();

            IEnumerable<string> GetAssemblyNames()
            {
                Version assemblyVersion = typeof(MefServices).GetTypeInfo().Assembly.GetName().Version;

                yield return $"Roslynator.CSharp.Workspaces, Version={assemblyVersion}, Culture=neutral, PublicKeyToken=ec3f0c29a7973f23";
                yield return $"Roslynator.VisualBasic.Workspaces, Version={assemblyVersion}, Culture=neutral, PublicKeyToken=59e9c6ae3cea4cef";
            }
        }

        private static Assembly TryLoadAssembly(string assemblyName)
        {
            try
            {
                return Assembly.Load(new AssemblyName(assemblyName));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public IEnumerable<Lazy<TExtension>> GetExports<TExtension>()
        {
            return _compositionContext.GetExports<TExtension>().Select(e => new Lazy<TExtension>(() => e));
        }

        public IEnumerable<Lazy<TExtension, TMetadata>> GetExports<TExtension, TMetadata>()
        {
            var importer = new WithMetadataImporter<TExtension, TMetadata>();
            _compositionContext.SatisfyImports(importer);
            return importer.Exports;
        }

        private class WithMetadataImporter<TExtension, TMetadata>
        {
            [ImportMany]
            public IEnumerable<Lazy<TExtension, TMetadata>> Exports { get; set; }
        }
    }
}

