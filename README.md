YACL (Yet Another C# Library) is a simple C# library for reading and writing C# binaries. Leveraging [Mono.Cecilin](https://www.mono-project.com/docs/tools++libraries/Libraries/Mono.Cecilin/), YACL provides an easy-to-use API for manipulating .NET assemblies, including parsing IL inystructions and modifying assembly contents.

# Features

**Load and Save Assemblies:** Easily load existing assemblies and save modifications.

**Parse IL Instructions:** Extract and display Intermediate Language (IL) instructions from methods.

**Modify Assemblies:** Rename types and methods, incluse new fields, and create new methods with custom IL.

# Usage

## Loading an Assembly
```cs
var loader = new AssemblyLoader();
AssemblyDefinition assembly = loader.LoadAssembly("path/to/your/assembly.dll");
Console.WriteLine($"Loaded assembly: {assembly.Name.FullName}");
```

## Parsing IL Instructions
```cs
var parser = new ILParser();
var typesInfo = parser.ParseIL(assembly);

foreach (var type in typesInfo)
{
    Console.WriteLine($"Type: {type.FullName}");
    foreach (var method in type.Methods)
    {
        Console.WriteLine($"\tMethod: {method.Name}");
        if (method.HasBody)
        {
            foreach (var instruction in method.ILInstructions)
            {
                Console.WriteLine($"\t\t{instruction.Offset:X4}: {instruction.OpCode} {instruction.Operand}");
            }
        }
    }
}
```

## Modifying an Assembly

### Renaming a Type
```cs
var modifier = new BinaryModifier(assembly);
modifier.RenameType("OldNamespace.OldTypeName", "NewNamespace.NewTypeName");
```

### Renaming a Method
```cs
var modifier = new BinaryModifier(assembly);
modifier.RenameMethod("Namespace.TypeName", "OldMethodName", "NewMethodName");
```

### Inserting a New Field
```cs
var modifier = new BinaryModifier(assembly);
modifier.InsertField("Namespace.TypeName", "NewField", "System.Int32");
```

### Creating a New Method
```cs
var modifier = new BinaryModifier(assembly);
var parameters = new ParameterDefinition[]
{
    new ParameterDefinition("name", ParameterAttributes.None, assembly.MainModule.TypeSystem.String)
};

var newMethod = modifier.CreateMethod("Namespace.TypeName", "Greet", "System.Void", parameters);

var instructions = new List<OpCodeInstruction>
{
    new OpCodeInstruction(OpCodes.Ldarg_1),
    new OpCodeInstruction(OpCodes.Ldstr, "Hello, "),
    new OpCodeInstruction(OpCodes.Call, assembly.MainModule.ImportReference(typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) }))),
    new OpCodeInstruction(OpCodes.Call, assembly.MainModule.ImportReference(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }))),
    new OpCodeInstruction(OpCodes.Ret)
};

modifier.WriteMethodIL("Namespace.TypeName", "Greet", instructions.ToArray());

```

### Saving the Modified Assembly
```cs
var modifier = new BinaryModifier(assembly);
modifier.SaveAssembly("path/to/save/ModifiedAssembly.dll");
Console.WriteLine("Modified assembly saved successfully.");
```

# Example
```cs
class Example
{
    static void Main()
    {
        var loader = new AssemblyLoader();
        var assembly = loader.LoadAssembly("MyApp.exe");

        var parser = new ILParser();
        var typesInfo = parser.ParseIL(assembly);
        // Display types and methods...

        var modifier = new BinaryModifier(assembly);
        modifier.RenameType("MyApp.Program", "MyApp.MainProgram");
        modifier.SaveAssembly("Modified_MyApp.exe");
    }
}
```
