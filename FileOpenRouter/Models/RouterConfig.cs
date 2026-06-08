using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FileOpenRouter.Models
{
    [DataContract]
    public class RouterConfig
    {
        [DataMember(Name = "rules")]
        public List<RouteRule> Rules { get; set; }

        [DataMember(Name = "fallbackProgram")]
        public string FallbackProgram { get; set; }

        public RouterConfig()
        {
            Rules = new List<RouteRule>();
            FallbackProgram = string.Empty;
        }
    }
}
