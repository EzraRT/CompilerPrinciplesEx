using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace ex2
{
    class FileManager
    {
        private StreamReader file;
        private ArrayList content;
        public FileManager(string fileName)
        {
            file = new StreamReader(fileName);
            content = new ArrayList();
            string line = null;
            while ((line = file.ReadLine()) != null)
            {
                content.Add(line);
            }
        }
        public string[] getFileContent()
        {
            return (string[])content.ToArray(typeof(string));
        }
    }
}
