using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Recommendations.API.Model.Settings;

namespace Recommendations.API.Services
{
    public interface IRecommendationService
    {
        Task<IReadOnlyList<(double Score, int ProductID)>> Get(int userID);
    }

    public class RecommendationService : IRecommendationService
    {
        readonly SemaphoreSlim _lock = new SemaphoreSlim(1);

        readonly GlobalOptions _options;
        readonly ILogger<RecommendationService> _log;

        readonly Process _pythonProcess;

        public RecommendationService(IOptions<GlobalOptions> options, ILogger<RecommendationService> log)
        {
            _log = log;
            _options = options.Value;

            _pythonProcess = Process.Start(new ProcessStartInfo(_options.PythonPath, _options.PredictionScriptPath)
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.ASCII,
                WorkingDirectory = Path.GetDirectoryName(_options.PredictionScriptPath)
            });
        }

        class PythonRecommendation
        {
            public double Score { get; set; }
            public double ProductID { get; set; }
        }

        public async Task<IReadOnlyList<(double Score, int ProductID)>> Get(int userID)
        {
            await _lock.WaitAsync();
            string responseJson;
            try
            {
                _log.LogWarning($"Write to STDIN: {userID}");
                await _pythonProcess.StandardInput.WriteLineAsync(userID.ToString());
                responseJson = await _pythonProcess.StandardOutput.ReadLineAsync();
            }
            finally
            {
                _lock.Release();
            }

            _log.LogWarning($"From to STDOUT: {responseJson}");
            PythonRecommendation[] response;
            try
            {
                response = JsonConvert.DeserializeObject<PythonRecommendation[]>(responseJson);
            }
            catch (Exception exception)
            {
                throw new FormatException($"Invalid json line: {responseJson}", exception);
            }

            return response.Select(x => (x.Score, (int)x.ProductID)).ToArray();
        }
    }
}
