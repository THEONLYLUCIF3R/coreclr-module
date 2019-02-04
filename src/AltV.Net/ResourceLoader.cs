using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AltV.Net
{
    internal class ResourceLoader
    {
        private readonly Module module;
        private readonly string basePath;

        private readonly Dictionary<string, Assembly> loadedAssemblies;
        private List<ResourceHandler> resources;

        internal ResourceLoader(Module module, string resourceName)
        {
            this.module = module;
            basePath = $"resources/{resourceName}";
            loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToDictionary(x => x.GetName().FullName, x => x);
        }

        public void Prepare()
        {
            resources = new List<ResourceHandler>(FindResources());

            foreach (var resource in resources)
            {
                resource.Prepare();
            }
        }

        public void Start()
        {
            foreach (var resource in resources)
            {
                resource.Start();
            }
        }

        public void Stop()
        {
            foreach (var resource in resources)
            {
                resource.Stop();
            }
        }

        //TODO: add recursively directory scan
        private IEnumerable<ResourceHandler> FindResources()
        {   
            var directoryFolder = new DirectoryInfo(basePath);
            yield return new ResourceHandler(module, directoryFolder, this);
            foreach (var directory in directoryFolder.GetDirectories())
            {
                yield return new ResourceHandler(module, directory, this);
            }
        }

        internal Assembly LoadAssembly(string path)
        {
            var reflectionAssembly = AssemblyName.GetAssemblyName(path);

            if (loadedAssemblies.TryGetValue(reflectionAssembly.FullName, out var element))
            {
                return element;
            }

            try
            {
                var loadedAssembly = Assembly.LoadFrom(path);

                loadedAssemblies[loadedAssembly.GetName().FullName] = loadedAssembly;

                return loadedAssembly;
            }
            catch (FileLoadException e)
            {
                module.Server.LogInfo($"An error occured while loading assembly \"{path}\": {e}");

                return null;
            }
        }
    }
}