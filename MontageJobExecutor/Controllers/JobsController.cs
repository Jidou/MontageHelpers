using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters.Internal;
using Microsoft.Extensions.Logging;

namespace MontageJobExecutor.Controllers {

    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase {

        private readonly ILogger _logger;
        private const string BasePath = "ExecutionResults";


        public JobsController(ILogger<JobsController> logger) {
            _logger = logger;
        }


        [HttpPost]
        public ActionResult<ExecutionResult> Post([FromBody] Arguments arguments) {
            try {
#if !DEBUG
                _logger.LogDebug($"Creating new process object with arguments: {arguments.ToString()}");

                var process = new Process {
                    StartInfo = new ProcessStartInfo {
                        FileName = $"{arguments.AppName}",
                        Arguments =
                            $"{arguments.Flags} {arguments.FlattenInputFitFilesArrayWithPath(GetDirectory(arguments.JobId))} {arguments.FlattenOutputFitFilesArrayWithPath(GetDirectory(arguments.JobId))} {arguments.FlattenInputHdrFilesArrayWithPath(GetDirectory(arguments.JobId))}",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };

                _logger.LogDebug($"Starting process");

                process.Start();

                _logger.LogDebug($"Reading stdout");

                var result = process.StandardOutput.ReadToEnd();

                _logger.LogDebug($"Waiting for process to exit");

                process.WaitForExit();
#else
                var result = "[struct stat=\"OK\", module=\"mProject\", time=223.0]";
#endif
                _logger.LogDebug($"Result: {result}");

                var response = ParseResult(result);

                _logger.LogDebug(response.ToString());

                return response;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error during job execution");
                return StatusCode(500);
            }
        }


        private ExecutionResult ParseResult(string result) {
            var statusIndex = result.IndexOf("stat=");
            var status = GetValueFromResult(result, statusIndex, 6);

            ExecutionResult response;
            if (status == "OK") {
                var moduleIndex = result.IndexOf("module=");
                var module = GetValueFromResult(result, moduleIndex, 8);

                var timeIndex = result.IndexOf("time=");
                var time = GetValueFromResult(result, timeIndex, 5);

                response = new ExecutionResult(status, module, time);
            } else if (status == "ERROR") {
                var messageIndex = result.IndexOf("module=");
                var message = GetValueFromResult(result, messageIndex, 5);

                response = new ExecutionResult(status, message);
            } else {
                var lengthOfStatus = statusIndex + 6 + status.Length + 1;
                var restOfResult = result.Substring(lengthOfStatus, result.Length - lengthOfStatus);

                response = new ExecutionResult(status, restOfResult);
            }


            return response;
        }


        private string GetValueFromResult(string result, int startIndex, int keyNameLength) {
            var indexOfStartOfValue = startIndex + keyNameLength;
            var indexOfEndOfValue = result.IndexOf('"', indexOfStartOfValue);
            indexOfEndOfValue = indexOfEndOfValue == -1 
                ? result.IndexOf(']', indexOfStartOfValue)
                : indexOfEndOfValue;

            return result.Substring(indexOfStartOfValue, indexOfEndOfValue - indexOfStartOfValue);
        }


        private double ConvertTimeStringToDouble(string time) {
            var styles = NumberStyles.Float;
            var provider = CultureInfo.CreateSpecificCulture("en");
            return double.Parse(time, styles, provider);
        }


        private static string GetDirectory(string jobId) {
            var directory = $"{Environment.CurrentDirectory}/{BasePath}/{jobId}";
            return directory;
        }
    }
}
