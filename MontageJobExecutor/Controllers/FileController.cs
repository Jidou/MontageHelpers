using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MontageJobExecutor.Controllers {

    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase {

        private readonly ILogger _logger;


        public FileController(ILogger<FileController> logger) {
            _logger = logger;
        }


        [HttpGet]
        [DisableRequestSizeLimit]
        public ActionResult<ExecutionResult> Get(string fileName, string jobId) {
            try {
                var stopwatch = Stopwatch.StartNew();
                var directory = Program.GetDirectory(jobId);
                var content = System.IO.File.ReadAllBytes($"{directory}/{fileName}");
                var montageFile = new MontageFile(fileName, content);
                var response = new ExecutionResult("OK", montageFile, stopwatch.ElapsedMilliseconds.ToString());

                return response;
                
            } catch (Exception ex) {
                _logger.LogError(ex, "Error during fetching file");
                return StatusCode(500);
            }
        }


        [HttpPost]
        [DisableRequestSizeLimit]
        public ActionResult<ExecutionResult> Post([FromBody] MontageFile file, string jobId) {
            try {
                var stopwatch = Stopwatch.StartNew();
                var directory = Program.GetDirectory(jobId);
                Directory.CreateDirectory(directory);

                using (var fileHandle = System.IO.File.Create($"{directory}/{file.Name}")) {
                    fileHandle.Write(file.Content);
                }

                var elapsedTime = stopwatch.ElapsedMilliseconds.ToString();

                _logger.LogInformation($"copied file in {elapsedTime}ms");

                return Created($"api/file/{jobId}", new ExecutionResult("OK", "File successfully copied to server", elapsedTime));
            } catch (Exception ex) {
                _logger.LogError(ex, "Error during creating file");
                return StatusCode(500);
            }
        }


        [HttpDelete]
        [DisableRequestSizeLimit]
        public ActionResult<ExecutionResult> Delete(string jobId) {
            try {
                var stopwatch = Stopwatch.StartNew();
                var directory = Program.GetDirectory(jobId);
                Directory.Delete(directory, true);
                return Ok(new ExecutionResult("OK", stopwatch.ElapsedMilliseconds.ToString()));
            } catch (Exception ex) {
                _logger.LogError(ex, "Error during deleting file");
                return StatusCode(500);
            }
        }
    }
}
