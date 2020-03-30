using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace GrapeCity.DataVisualization.Chart.TestSite
{
    public class AssemblyLoader : AssemblyLoadContext
    {
        #region Fields
        private string _assemblyPath;
        #endregion

        #region Constructor
        public AssemblyLoader(string assemblyPath) : base()
        {
            this._assemblyPath = assemblyPath;
        }
        #endregion

        #region Methods
        public Assembly Load()
        {
            return this.LoadFromAssemblyPath(this._assemblyPath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string assemblyPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this._assemblyPath), $"{assemblyName.Name}.dll");
            if (File.Exists(assemblyPath))
            {
                return LoadFromAssemblyPath(assemblyPath);
            }
            return Default.LoadFromAssemblyName(assemblyName) ?? null;
        }
        #endregion

    }
}
