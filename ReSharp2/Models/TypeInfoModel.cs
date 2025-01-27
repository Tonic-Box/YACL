namespace ReSharp2.Models
{
    public class TypeInfoModel
    {
        public string FullName { get; set; }
        public List<MethodInfoModel> Methods { get; set; } = new List<MethodInfoModel>();
    }
}
