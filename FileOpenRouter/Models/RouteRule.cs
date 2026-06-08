using System.Runtime.Serialization;

namespace FileOpenRouter.Models
{
    [DataContract]
    public class RouteRule
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "folder")]
        public string Folder { get; set; }

        [DataMember(Name = "program")]
        public string Program { get; set; }

        [DataMember(Name = "enabled")]
        public bool Enabled { get; set; }

        public RouteRule()
        {
            Name = string.Empty;
            Folder = string.Empty;
            Program = string.Empty;
            Enabled = true;
        }
    }
}
