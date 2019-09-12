using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MontageJobExecutor {
    public class Arguments {

        public string JobId { get; set; }
        public string AppNameWithParameters { get; set; }


        public Arguments(string jobId, string appNameWithParameters) {
            JobId = jobId;
            AppNameWithParameters = appNameWithParameters;
        }


        public override string ToString() {
            return $"JobId: {JobId}, AppNameWithParameters: {AppNameWithParameters}";
        }
    }
}
