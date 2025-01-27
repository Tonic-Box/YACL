using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ReSharp2
{
    class Program
    {
        static void Main(string[] args)
        {
            // Check if at least one argument is provided
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: ILUtilitiesDemo <path_to_assembly>");
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

            // Initialize BinaryModifier
            var modifier = new BinaryModifier(assembly);

            try
            {
                // Example Modifications:

                // 1. Rename a type (Replace with actual type name from your assembly)
                // Example: modifier.RenameType("MyApp.Program", "MyApp.MainProgram");

                // 2. Rename a method (Replace with actual type and method names)
                // Example: modifier.RenameMethod("MyApp.MainProgram", "Main", "Start");

                // 3. Insert a new field (Replace with actual type and desired field details)
                // Example: modifier.InsertField("MyApp.MainProgram", "NewField", "System.Int32");

                // Uncomment and modify the above examples as needed

                // 4. Create a new method
                CreateAndAddNewMethod(modifier, assembly);

                // Save the modified assembly
                string outputPath = Path.Combine(Path.GetDirectoryName(assemblyPath), "Modified_" + Path.GetFileName(assemblyPath));
                modifier.SaveAssembly(outputPath);
                Console.WriteLine($"\nModified assembly saved to: {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error modifying assembly: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new method and adds IL instructions to its body.
        /// </summary>
        /// <param name="modifier">Instance of BinaryModifier.</param>
        /// <param name="assembly">The AssemblyDefinition being modified.</param>
        private static void CreateAndAddNewMethod(BinaryModifier modifier, AssemblyDefinition assembly)
        {
            // Define the target type where the new method will be added
            string targetTypeFullName = "MyApp.Program"; // Replace with actual type name

            // Define the new method's signature
            string newMethodName = "Greet";
            string returnType = "System.Void";
            var parameters = new ParameterDefinition[]
            {
                new ParameterDefinition("name", ParameterAttributes.None, assembly.MainModule.TypeSystem.String)
            };

            // Create the new method
            var newMethod = modifier.CreateMethod(targetTypeFullName, newMethodName, returnType, parameters);
            Console.WriteLine($"Created new method: {newMethod.FullName}");

            // Define IL instructions for the new method
            var instructions = new List<OpCodeInstruction>
            {
                // Load the 'name' argument onto the stack
                new OpCodeInstruction(OpCodes.Ldarg_1),

                // Load the string "Hello, " onto the stack
                new OpCodeInstruction(OpCodes.Ldstr, "Hello, "),

                // Concatenate the two strings
                new OpCodeInstruction(OpCodes.Call, assembly.MainModule.ImportReference(typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) }))),

                // Call Console.WriteLine(string)
                new OpCodeInstruction(OpCodes.Call, assembly.MainModule.ImportReference(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }))),

                // Return
                new OpCodeInstruction(OpCodes.Ret)
            };

            // Write IL instructions to the new method
            modifier.WriteMethodIL(targetTypeFullName, newMethodName, instructions.ToArray());
            Console.WriteLine($"Added IL instructions to method: {newMethodName}");
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
