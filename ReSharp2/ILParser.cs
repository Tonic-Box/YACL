using ReSharp2.Models;
using Mono.Cecil;

namespace ReSharp2
{
    public class ILParser
    {
        public List<TypeInfoModel> ParseIL(AssemblyDefinition assembly)
        {
            var typesInfo = new List<TypeInfoModel>();

            foreach (var module in assembly.Modules)
            {
                foreach (var type in module.Types)
                {
                    // Skip compiler-generated types
                    if (type.Name.StartsWith("<"))
                        continue;

                    var typeInfo = new TypeInfoModel
                    {
                        FullName = type.FullName
                    };

                    foreach (var method in type.Methods)
                    {
                        var methodInfo = new MethodInfoModel
                        {
                            Name = method.Name,
                            HasBody = method.HasBody
                        };

                        if (method.HasBody)
                        {
                            foreach (var instruction in method.Body.Instructions)
                            {
                                methodInfo.ILInstructions.Add(instruction);
                            }
                        }

                        typeInfo.Methods.Add(methodInfo);
                    }

                    typesInfo.Add(typeInfo);
                }
            }

            return typesInfo;
        }
    }
}
