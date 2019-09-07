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


        public ExecutionResult(string status, string module, string executionTime) {
            Status = status;
            Module = module;
            ExecutionTime = executionTime;
            Message = string.Empty;
        }


        public ExecutionResult(string status, string message) {
            Status = status;
            Module = string.Empty;
            ExecutionTime = string.Empty;
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
