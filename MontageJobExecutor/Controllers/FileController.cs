using System;
using System.Collections.Generic;
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
        private const string BasePath = "ExecutionResults";


        public FileController(ILogger<FileController> logger) {
            _logger = logger;
        }


        [HttpGet]
        public ActionResult<MontageFile> Get(string fileName, string jobId) {
            try {
                var directory = GetDirectory(jobId);
                var content = System.IO.File.ReadAllBytes($"{directory}/{fileName}");
                var montageFile = new MontageFile(fileName, content);

                return montageFile;
                
            } catch (Exception ex) {
                _logger.LogError(ex, "Error during fetching file");
                return StatusCode(500);
            }
        }


        [HttpPost]
        public ActionResult Post([FromBody] MontageFile file, string jobId) {
            try {
                var directory = GetDirectory(jobId);
                Directory.CreateDirectory(directory);

                using (var fileHandle = System.IO.File.Create($"{directory}/{file.Name}")) {
                    fileHandle.Write(file.Content);
                }

                return Created($"api/file/{jobId}", null);
            } catch (Exception ex) {
                _logger.LogError(ex, "Error during creating file");
                return StatusCode(500);
            }
        }


        [HttpDelete]
        public ActionResult Delete(string fileName, string jobId) {
            try {
                var directory = GetDirectory(jobId);
                System.IO.File.Delete($"{directory}/{fileName}");
                return Ok();
            } catch (Exception ex) {
                _logger.LogError(ex, "Error during deleting file");
                return StatusCode(500);
            }
        }


        private static string GetDirectory(string jobId) {
            var directory = $"{Environment.CurrentDirectory}/{BasePath}/{jobId}";
            return directory;
        }
    }
}
