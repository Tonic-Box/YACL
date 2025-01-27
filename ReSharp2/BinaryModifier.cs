using Mono.Cecil;

namespace ReSharp2
{
    public class BinaryModifier
    {
        private AssemblyDefinition _assembly;

        public BinaryModifier(AssemblyDefinition assembly)
        {
            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        }

        /// <summary>
        /// Renames a type in the assembly.
        /// </summary>
        /// <param name="originalFullName">The original full name of the type.</param>
        /// <param name="newName">The new name for the type.</param>
        public void RenameType(string originalFullName, string newName)
        {
            var type = _assembly.MainModule.Types.FirstOrDefault(t => t.FullName == originalFullName);
            if (type == null)
                throw new InvalidOperationException($"Type '{originalFullName}' not found.");

            type.Name = newName;
        }

        /// <summary>
        /// Renames a method within a specified type.
        /// </summary>
        /// <param name="typeFullName">Full name of the type containing the method.</param>
        /// <param name="originalMethodName">Original method name.</param>
        /// <param name="newMethodName">New method name.</param>
        public void RenameMethod(string typeFullName, string originalMethodName, string newMethodName)
        {
            var type = _assembly.MainModule.Types.FirstOrDefault(t => t.FullName == typeFullName);
            if (type == null)
                throw new InvalidOperationException($"Type '{typeFullName}' not found.");

            var method = type.Methods.FirstOrDefault(m => m.Name == originalMethodName);
            if (method == null)
                throw new InvalidOperationException($"Method '{originalMethodName}' not found in type '{typeFullName}'.");

            method.Name = newMethodName;
        }

        /// <summary>
        /// Inserts a new field into a specified type.
        /// </summary>
        /// <param name="typeFullName">Full name of the type.</param>
        /// <param name="fieldName">Name of the new field.</param>
        /// <param name="fieldTypeFullName">Full name of the field's type.</param>
        public void InsertField(string typeFullName, string fieldName, string fieldTypeFullName)
        {
            var type = _assembly.MainModule.Types.FirstOrDefault(t => t.FullName == typeFullName);
            if (type == null)
                throw new InvalidOperationException($"Type '{typeFullName}' not found.");

            var fieldType = _assembly.MainModule.ImportReference(Type.GetType(fieldTypeFullName) ?? throw new InvalidOperationException($"Type '{fieldTypeFullName}' not found."));

            var newField = new FieldDefinition(fieldName, FieldAttributes.Public, fieldType);
            type.Fields.Add(newField);
        }

        /// <summary>
        /// Saves the modified assembly to a specified path.
        /// </summary>
        /// <param name="outputPath">Path to save the modified assembly.</param>
        public void SaveAssembly(string outputPath)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Output path cannot be null or empty.", nameof(outputPath));

            _assembly.Write(outputPath);
        }
    }
}
