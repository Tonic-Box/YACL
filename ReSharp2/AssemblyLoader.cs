using Mono.Cecil;

namespace ReSharp2
{
    public class AssemblyLoader
    {
        public AssemblyDefinition LoadAssembly(string assemblyPath)
        {
            if (string.IsNullOrWhiteSpace(assemblyPath))
                throw new ArgumentException("Assembly path cannot be null or empty.", nameof(assemblyPath));

            if (!System.IO.File.Exists(assemblyPath))
                throw new System.IO.FileNotFoundException($"The file '{assemblyPath}' does not exist.", assemblyPath);

            try
            {
                var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
                return assembly;
            }
            catch (BadImageFormatException ex)
            {
                throw new InvalidOperationException("The file is not a valid .NET assembly.", ex);
            }
        }
    }
}
