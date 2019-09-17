using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MontageJobExecutor {
    public class ExecutionResult {

        public string Status { get; set; }
        public string Module { get; set; }
        public string ExecutionTime { get; set; }
        public string Message { get; set; }
        public MontageFile File { get; set; }


        public ExecutionResult(string status, string module, string executionTime) {
            Status = status;
            Module = module;
            ExecutionTime = executionTime;
            Message = string.Empty;
        }


        public ExecutionResult(string status, MontageFile montageFile, string executionTime) {
            Status = status;
            File = montageFile;
            ExecutionTime = executionTime;
        }


        public ExecutionResult(string status, string executionTime) {
            Status = status;
            Module = string.Empty;
            ExecutionTime = executionTime;
            Message = string.Empty;
        }


        public ExecutionResult(string message) {
            Status = "ERROR";
            Message = message;
        }


        public override string ToString() {
            string toString;

            if (Status == "OK") {
                toString = $"Status: {Status}, Module: {Module}, Time: {ExecutionTime}";
            } else if (Status == "ERROR") {
                toString = $"Status: {Status}, Message: {Message}";
            } else {
                toString = $"Status: {Status}";
            }

            return toString;
        }

    }
}
