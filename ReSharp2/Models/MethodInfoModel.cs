using Mono.Cecil.Cil;

namespace ReSharp2.Models
{
    public class MethodInfoModel
    {
        public string Name { get; set; }
        public bool HasBody { get; set; }
        public List<Instruction> ILInstructions { get; set; } = new List<Instruction>();
    }
}
