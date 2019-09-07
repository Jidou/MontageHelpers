using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MontageJobExecutor {
    public class MontageFile {

        public string Name { get; set; }
        public byte[] Content { get; set; }


        public MontageFile(string name, byte[] content) {
            Name = name;
            Content = content;
        }
    }
}
