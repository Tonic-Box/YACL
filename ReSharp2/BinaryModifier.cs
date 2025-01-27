// ILUtilities/BinaryModifier.cs
using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ReSharp2
{
    public class BinaryModifier
    {
        private AssemblyDefinition _assembly;

        public BinaryModifier(AssemblyDefinition assembly)
        {
            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        }

        // Existing methods...

        /// <summary>
        /// Creates a new method within a specified type.
        /// </summary>
        /// <param name="typeFullName">Full name of the type to add the method to.</param>
        /// <param name="methodName">Name of the new method.</param>
        /// <param name="returnTypeFullName">Full name of the return type.</param>
        /// <param name="parameters">Optional array of parameter definitions.</param>
        /// <returns>The newly created MethodDefinition.</returns>
        public MethodDefinition CreateMethod(
            string typeFullName,
            string methodName,
            string returnTypeFullName,
            ParameterDefinition[] parameters = null)
        {
            var type = _assembly.MainModule.Types.FirstOrDefault(t => t.FullName == typeFullName);
            if (type == null)
                throw new InvalidOperationException($"Type '{typeFullName}' not found.");

            // Resolve return type
            TypeReference returnType = ResolveType(returnTypeFullName);

            // Create method attributes (public and hidebysig)
            var methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig;

            // Define the method
            var newMethod = new MethodDefinition(methodName, methodAttributes, returnType);

            // Add parameters if any
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    newMethod.Parameters.Add(param);
                }
            }

            // Initialize method body with an empty IL processor
            var ilProcessor = newMethod.Body.GetILProcessor();

            // Optional: Initialize with a default return (e.g., 'ret' for void)
            if (returnType.FullName == _assembly.MainModule.TypeSystem.Void.FullName)
            {
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
            }

            // Add the new method to the type
            type.Methods.Add(newMethod);

            return newMethod;
        }

        /// <summary>
        /// Writes IL instructions to a method's body.
        /// </summary>
        /// <param name="typeFullName">Full name of the type containing the method.</param>
        /// <param name="methodName">Name of the method to modify.</param>
        /// <param name="instructions">Array of IL instructions to add.</param>
        public void WriteMethodIL(string typeFullName, string methodName, OpCodeInstruction[] instructions)
        {
            var type = _assembly.MainModule.Types.FirstOrDefault(t => t.FullName == typeFullName);
            if (type == null)
                throw new InvalidOperationException($"Type '{typeFullName}' not found.");

            var method = type.Methods.FirstOrDefault(m => m.Name == methodName);
            if (method == null)
                throw new InvalidOperationException($"Method '{methodName}' not found in type '{typeFullName}'.");

            if (!method.HasBody)
                method.Body.InitLocals = true; // Initialize locals if not present

            var ilProcessor = method.Body.GetILProcessor();
            method.Body.Instructions.Clear(); // Clear existing instructions

            foreach (var opInstr in instructions)
            {
                Instruction instruction;

                // Handle different operand types
                if (opInstr.Operand == null)
                {
                    instruction = ilProcessor.Create(opInstr.OpCode);
                }
                else if (opInstr.Operand is string str)
                {
                    instruction = ilProcessor.Create(opInstr.OpCode, str);
                }
                else if (opInstr.Operand is int i)
                {
                    instruction = ilProcessor.Create(opInstr.OpCode, i);
                }
                else if (opInstr.Operand is MethodReference methodRef)
                {
                    instruction = ilProcessor.Create(opInstr.OpCode, methodRef);
                }
                else if (opInstr.Operand is TypeReference typeRef)
                {
                    instruction = ilProcessor.Create(opInstr.OpCode, typeRef);
                }
                else
                {
                    throw new NotSupportedException($"Operand type '{opInstr.Operand.GetType()}' is not supported.");
                }

                ilProcessor.Append(instruction);
            }
        }

        /// <summary>
        /// Resolves a type by its full name.
        /// </summary>
        /// <param name="typeFullName">Full name of the type.</param>
        /// <returns>TypeReference of the resolved type.</returns>
        private TypeReference ResolveType(string typeFullName)
        {
            // Attempt to get the type from the current module
            var type = _assembly.MainModule.GetType(typeFullName);
            if (type != null)
                return type;

            // Attempt to resolve from mscorlib or System assemblies
            var systemType = Type.GetType(typeFullName);
            if (systemType != null)
                return _assembly.MainModule.ImportReference(systemType);

            throw new InvalidOperationException($"Type '{typeFullName}' could not be resolved.");
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

    /// <summary>
    /// Represents an IL instruction with an OpCode and an optional operand.
    /// </summary>
    public class OpCodeInstruction
    {
        public OpCode OpCode { get; set; }
        public object Operand { get; set; }

        public OpCodeInstruction(OpCode opCode, object operand = null)
        {
            OpCode = opCode;
            Operand = operand;
        }
    }
}
