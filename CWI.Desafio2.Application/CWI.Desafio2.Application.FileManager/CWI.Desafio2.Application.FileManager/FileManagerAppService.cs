using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CWI.Desafio2.Application.FileManager
{
    public class FileManagerAppService
    {
        private const string HOMEPATH = "%HOMEDRIVE%%HOMEPATH%";

        public async Task<Tuple<string[], string>> ReadFile()
        {
            try
            {
                var path = string.Concat(HOMEPATH, !HOMEPATH.Last().Equals('/') ? "/" : string.Empty, "data");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                var inPath = string.Concat(path, "/out");

                if (!Directory.Exists(inPath))
                    Directory.CreateDirectory(inPath);

                var result = new List<string>();

                using (var reader = new StreamReader(inPath))
                    while (reader.Peek() >= 0)
                        result.Add(await reader.ReadLineAsync());

                var @return = new Tuple<string[], string>(result, inPath);

                return result.ToArray();
            }
            catch (FileNotFoundException fnfe)
            {
                //File not found.
            }
        }

        public async Task WriteFile(string[] data)
        {
            var sb = new StringBuilder();

            var path = string.Concat(HOMEPATH, !HOMEPATH.Last().Equals('/') ? "/" : string.Empty, "data");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var outPath = string.Concat(path, "/out");

            if (!Directory.Exists(outPath))
                Directory.CreateDirectory(outPath);

            var fullPath = string.Concat(outPath, DateTime.Now.ToString("yyMMdd_HHmmss"), ".csv");

            for (var i = 0; i < data.Length; i++)
                sb.AppendLine(data[i]);

            await using var sw = new StreamWriter(fullPath);
            sw.WriteLine(sb);
        }
    }
}