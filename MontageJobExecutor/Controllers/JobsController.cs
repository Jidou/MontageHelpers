using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MontageJobExecutor.Controllers {

    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase {

        private readonly ILogger _logger;


        public JobsController(ILogger<JobsController> logger, IConfiguration config) {
            _logger = logger;
        }


        [HttpPost]
        [DisableRequestSizeLimit]
        public ActionResult<ExecutionResult> Post([FromBody] Arguments arguments) {
            try {
#if DEBUG
                _logger.LogInformation($"Creating new process object with arguments: {arguments}");

                var fileNameAndArguments = arguments.AppNameWithParameters.Split(' ');

                var allArguments = new StringBuilder();

                for (var i = 1; i < fileNameAndArguments.Length; i++) {
                    if (fileNameAndArguments[i].EndsWith(".txt") || fileNameAndArguments[i].EndsWith(".fits") || fileNameAndArguments[i].EndsWith(".hdr") || fileNameAndArguments[i].EndsWith(".tbl") || fileNameAndArguments[i].EndsWith(".jpg")) {
                        allArguments.Append($"{GetDirectory()}/{fileNameAndArguments[i]} ");
                    } else {
                        allArguments.Append($"{fileNameAndArguments[i]} ");
                    }
                }

                _logger.LogInformation($"FileName: {fileNameAndArguments[0]}");
                _logger.LogInformation($"Arguments: {allArguments}");

                var response = ExecuteAndReadResult(fileNameAndArguments[0], allArguments);

#else
                var result = "[struct stat=\"OK\", module=\"mProject\", time=223.0]";
                var response = ParseResult(result);
#endif

                _logger.LogInformation(response.ToString());

                return response;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error during job execution");
                return StatusCode(500);
            }
        }


        private ExecutionResult ExecuteAndReadResult(string fileNameAndArgument, StringBuilder allArguments) {
            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = $"{fileNameAndArgument}",
                    Arguments = allArguments.ToString(),
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            _logger.LogInformation($"Starting process");

            var stopwatch = new Stopwatch();
            process.Start();

            _logger.LogInformation($"Reading stdout");

            var result = process.StandardOutput.ReadToEnd();

            _logger.LogInformation($"Result: {result}");
            _logger.LogInformation($"Waiting for process to exit");

            process.WaitForExit();

            var finishTime = stopwatch.ElapsedMilliseconds;
            ExecutionResult response = null;

            if (fileNameAndArgument.StartsWith("mAdd")) {

            } else if (fileNameAndArgument.StartsWith("mBackground")) {
                // TODO
            } else if (fileNameAndArgument.StartsWith("mBgModel")) {
                response = ParseResult(result);
                response.ExecutionTime = finishTime.ToString();
            } else if (fileNameAndArgument.StartsWith("mConcatFit")) {
                response = ParseResult(result);
                response.ExecutionTime = finishTime.ToString();
            } else if (fileNameAndArgument.StartsWith("mDiffFit")) {
                response = new ExecutionResult("OK", finishTime.ToString());
            } else if (fileNameAndArgument.StartsWith("mImgtbl")) {
                response = ParseResult(result);
                response.ExecutionTime = finishTime.ToString();
            } else if (fileNameAndArgument.StartsWith("mJPEG")) {
                // TODO
            } else if (fileNameAndArgument.StartsWith("mProject")) {
                response = ParseResult(result);
            } else {
                // TODO
            }

            return response;
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
                var messageIndex = result.IndexOf("msg=");
                var message = GetValueFromResult(result, messageIndex, 5);

                response = new ExecutionResult(message);
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


        private static string GetDirectory() {
            var directory = $"{Environment.CurrentDirectory}/{Program.BasePath}";
            return directory;
        }
    }
}
