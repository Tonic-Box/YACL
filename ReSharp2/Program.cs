using Mono.Cecil;

namespace ReSharp2
{
    class Program
    {
        static void Main(string[] args)
        {
            // Check if at least one argument is provided
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: ReSharp <path_to_assembly>");
                return;
            }

            string assemblyPath = args[0];

            // Validate assembly path
            if (!File.Exists(assemblyPath))
            {
                Console.WriteLine($"Error: The file '{assemblyPath}' does not exist.");
                return;
            }

            var loader = new AssemblyLoader();
            AssemblyDefinition assembly;

            try
            {
                assembly = loader.LoadAssembly(assemblyPath);
                Console.WriteLine($"Successfully loaded assembly: {assembly.Name.FullName}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading assembly: {ex.Message}");
                return;
            }

            var parser = new ILParser();
            var typesInfo = parser.ParseIL(assembly);

            // Display parsed IL
            foreach (var type in typesInfo)
            {
                Console.WriteLine($"Type: {type.FullName}");
                foreach (var method in type.Methods)
                {
                    Console.WriteLine($"\tMethod: {method.Name}");
                    if (method.HasBody)
                    {
                        Console.WriteLine("\t\tIL Instructions:");
                        foreach (var instruction in method.ILInstructions)
                        {
                            Console.WriteLine($"\t\t\t{instruction.Offset.ToString("X4")}: {instruction.OpCode} {OperandToString(instruction)}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("\t\t(No IL body, possibly abstract or external method)");
                    }
                }
                Console.WriteLine();
            }

            // Example modifications
            var modifier = new BinaryModifier(assembly);

            try
            {
                // Rename a type (Replace with actual type name from your assembly)
                // Example: modifier.RenameType("MyApp.Program", "MyApp.MainProgram");

                // Rename a method (Replace with actual type and method names)
                // Example: modifier.RenameMethod("MyApp.MainProgram", "Main", "Start");

                // Insert a new field (Replace with actual type and desired field details)
                // Example: modifier.InsertField("MyApp.MainProgram", "NewField", "System.Int32");

                // Uncomment and modify the above examples as needed

                // Save the modified assembly
                string outputPath = Path.Combine(Path.GetDirectoryName(assemblyPath), "Modified_" + Path.GetFileName(assemblyPath));
                modifier.SaveAssembly(outputPath);
                Console.WriteLine($"Modified assembly saved to: {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error modifying assembly: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper method to convert the operand to a readable string.
        /// </summary>
        static string OperandToString(Mono.Cecil.Cil.Instruction instruction)
        {
            if (instruction.Operand == null)
                return string.Empty;

            switch (instruction.OpCode.OperandType)
            {
                case Mono.Cecil.Cil.OperandType.InlineBrTarget:
                case Mono.Cecil.Cil.OperandType.InlineField:
                case Mono.Cecil.Cil.OperandType.InlineMethod:
                case Mono.Cecil.Cil.OperandType.InlineSig:
                case Mono.Cecil.Cil.OperandType.InlineTok:
                case Mono.Cecil.Cil.OperandType.InlineType:
                    return instruction.Operand.ToString();
                case Mono.Cecil.Cil.OperandType.InlineString:
                    return $"\"{instruction.Operand}\"";
                case Mono.Cecil.Cil.OperandType.InlineI:
                case Mono.Cecil.Cil.OperandType.InlineI8:
                case Mono.Cecil.Cil.OperandType.InlineR:
                    return instruction.Operand.ToString();
                case Mono.Cecil.Cil.OperandType.InlineNone:
                    return string.Empty;
                case Mono.Cecil.Cil.OperandType.ShortInlineBrTarget:
                case Mono.Cecil.Cil.OperandType.ShortInlineI:
                case Mono.Cecil.Cil.OperandType.ShortInlineR:
                case Mono.Cecil.Cil.OperandType.ShortInlineVar:
                    return instruction.Operand.ToString();
                case Mono.Cecil.Cil.OperandType.InlineVar:
                    return instruction.Operand.ToString();
                default:
                    return instruction.Operand.ToString();
            }
        }
    }
}
