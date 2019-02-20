namespace Recommendations.API.Model.Settings
{
    public class GlobalOptions
    {
        public string ConnectionString { get; set; }

        public string JWTSecret { get; set; }

        public string PythonPath { get; set; }
        public string PredictionScriptPath { get; set; }
    }
}
