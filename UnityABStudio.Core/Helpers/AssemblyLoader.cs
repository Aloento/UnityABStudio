namespace SoarCraft.QYun.UnityABStudio.Core.Helpers {
    using Mono.Cecil;
    using System.Collections.Generic;
    using System.IO;

    public class AssemblyLoader {
        public bool Loaded;
        private readonly Dictionary<string, ModuleDefinition> moduleDic = new();

        public void Load(string path) {
            var files = Directory.GetFiles(path, "*.dll");
            var resolver = new MyAssemblyResolver();
            var readerParameters = new ReaderParameters {
                AssemblyResolver = resolver
            };
            try {
                foreach (var file in files) {
                    var assembly = AssemblyDefinition.ReadAssembly(file, readerParameters);
                    resolver.Register(assembly);
                    this.moduleDic.Add(assembly.MainModule.Name, assembly.MainModule);
                }
            } catch {
                // ignored
            }

            this.Loaded = true;
        }

        public TypeDefinition GetTypeDefinition(string assemblyName, string fullName) {
            if (this.moduleDic.TryGetValue(assemblyName, out var module)) {
                var typeDef = module.GetType(fullName);
                if (typeDef == null && assemblyName == "UnityEngine.dll") {
                    foreach (var pair in this.moduleDic) {
                        typeDef = pair.Value.GetType(fullName);
                        if (typeDef != null) {
                            break;
                        }
                    }
                }

                return typeDef;
            }

            return null;
        }

        public void Clear() {
            foreach (var pair in this.moduleDic) {
                pair.Value.Dispose();
            }

            this.moduleDic.Clear();
            this.Loaded = false;
        }
    }

    public class MyAssemblyResolver : DefaultAssemblyResolver {
        public void Register(AssemblyDefinition assembly) => RegisterAssembly(assembly);
    }
}
