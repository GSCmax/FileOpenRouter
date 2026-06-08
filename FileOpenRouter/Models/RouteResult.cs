namespace FileOpenRouter.Models
{
    public class RouteResult
    {
        public bool Success { get; set; }
        public string MatchedRuleName { get; set; }
        public string ProgramPath { get; set; }
        public string ErrorMessage { get; set; }
    }
}
