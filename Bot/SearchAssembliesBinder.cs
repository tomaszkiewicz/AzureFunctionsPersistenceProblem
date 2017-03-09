using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Bot
{
    sealed class SearchAssembliesBinder : SerializationBinder
    {
        private readonly bool _searchInDlls;
        private readonly Assembly _currentAssembly;

        public SearchAssembliesBinder(Assembly currentAssembly, bool searchInDlls)
        {
            _currentAssembly = currentAssembly;
            _searchInDlls = searchInDlls;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            var assemblyNames = new List<AssemblyName>();

            assemblyNames.Add(_currentAssembly.GetName());
            assemblyNames.Add(Assembly.GetCallingAssembly().GetName());

            if (_searchInDlls)
            {
                assemblyNames.AddRange(_currentAssembly.GetReferencedAssemblies());
                assemblyNames.AddRange(AppDomain.CurrentDomain.GetAssemblies().Select(s => s.GetName()));
            }

            assemblyNames = assemblyNames.OrderBy(s => s.Name).ToList();

            foreach (AssemblyName an in assemblyNames)
            {
                var typeToDeserialize = GetTypeToDeserialize(typeName, an);

                if (typeToDeserialize != null)
                    return typeToDeserialize; // found
            }

            return null; // not found
        }

        private static Type GetTypeToDeserialize(string typeName, AssemblyName an)
        {
            string fullTypeName = $"{typeName}, {an.FullName}";

            var typeToDeserialize = Type.GetType(fullTypeName);

            return typeToDeserialize;
        }
    }
}