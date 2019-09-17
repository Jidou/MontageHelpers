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
        private readonly string _montageBinaryFilesPath;


        public JobsController(ILogger<JobsController> logger, IConfiguration config) {
            _logger = logger;
            _montageBinaryFilesPath = config["MontageBinaryFilesPath"];
        }


        [HttpPost]
        public ActionResult<ExecutionResult> Post([FromBody] Arguments arguments) {
            try {
#if !DEBUG
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

                _logger.LogInformation($"FileName: {_montageBinaryFilesPath}{fileNameAndArguments[0]}");
                _logger.LogInformation($"Arguments: {allArguments}");

                var process = new Process {
                    StartInfo = new ProcessStartInfo {
                        FileName = $"{_montageBinaryFilesPath}{fileNameAndArguments[0]}",
                        Arguments = allArguments.ToString(),
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };

                _logger.LogInformation($"Starting process");

                process.Start();

                _logger.LogInformation($"Reading stdout");

                var result = process.StandardOutput.ReadToEnd();

                _logger.LogInformation($"Waiting for process to exit");

                process.WaitForExit();
#else
                var result = "[struct stat=\"OK\", module=\"mProject\", time=223.0]";
#endif
                _logger.LogInformation($"Result: {result}");

                var response = ParseResult(result);

                _logger.LogInformation(response.ToString());

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
                var messageIndex = result.IndexOf("msg=");
                var message = GetValueFromResult(result, messageIndex, 4);

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


        private static string GetDirectory() {
            var directory = $"{Environment.CurrentDirectory}/{Program.BasePath}";
            return directory;
        }
    }
}
