using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MontageJobExecutor {
    public class Arguments {

        public string JobId { get; set; }
        public string AppName { get; set; }
        public string Flags { get; set; }
        public string[] InputFitFiles { get; set; }
        public string[] InputHdrFiles { get; set; }
        public string[] OutputFitFiles { get; set; }


        public Arguments(string flags, string[] inputFitFiles, string[] outputFitFiles, string[] inputHdrFiles, string jobId, string appName) {
            Flags = flags;
            InputFitFiles = inputFitFiles;
            OutputFitFiles = outputFitFiles;
            InputHdrFiles = inputHdrFiles;
            JobId = jobId;
            AppName = appName;
        }


        public string FlattenInputFitFilesArray() {
            return InputFitFiles.Aggregate(string.Empty, (current, inputFile) => current + $"{inputFile} ");
        }


        public string FlattenInputHdrFilesArray() {
            return InputHdrFiles.Aggregate(string.Empty, (current, inputFile) => current + $"{inputFile} ");
        }


        public string FlattenOutputFitFilesArray() {
            return OutputFitFiles.Aggregate(string.Empty, (current, inputFile) => current + $"{inputFile} ");
        }


        public string FlattenInputFitFilesArrayWithPath(string path) {
            return InputFitFiles.Aggregate(string.Empty, (current, inputFile) => current + $"{path}/{inputFile} ");
        }


        public string FlattenInputHdrFilesArrayWithPath(string path) {
            return InputHdrFiles.Aggregate(string.Empty, (current, inputFile) => current + $"{path}/{inputFile} ");
        }


        public string FlattenOutputFitFilesArrayWithPath(string path) {
            return OutputFitFiles.Aggregate(string.Empty, (current, inputFile) => current + $"{path}/{inputFile} ");
        }


        public override string ToString() {
            return $"JobId: {JobId}, AppName: {AppName}, Flags: {Flags}, InputFitFiles: {FlattenInputFitFilesArray()}, OutputFitFiles: {FlattenOutputFitFilesArray()}, InputHdrFiles: {FlattenInputHdrFilesArray()}";
        }
    }
}
