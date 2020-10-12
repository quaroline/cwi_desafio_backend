using CWI.Desafio2.Application.FileManager.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace CWI.Desafio2.Application.FileManager
{
    public class FileManagerAppService
    {
        public readonly string HOMEPATH = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "\\data\\");

        public FileManagerAppService()
        {
            if (!Directory.Exists(HOMEPATH))
                Directory.CreateDirectory(HOMEPATH);
        }

        public FileViewModel ReadFile(string filename)
        {
            try
            {
                var content = new List<string>();

                StreamReader reader;

                // Program won't be able to open a copied file sometimes bc of Windows threads, so it'll try open the file 3 times.
                for (int i = 0; i <= 3; ++i)
                {
                    try
                    {
                        reader = new StreamReader(HOMEPATH + "in\\" + filename);

                        while (reader.Peek() >= 0)
                            content.Add(reader.ReadLine());

                        break;
                    }
                    catch (IOException) when (i <= 3)
                    {
                        Thread.Sleep(1000);
                    }
                }

                return content.Any() ? new FileViewModel()
                {
                    Content = content.ToArray(),
                    Filename = filename
                } : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void WriteFile(string[] data, string filename)
        {
            var sb = new StringBuilder();

            var outPath = string.Concat(HOMEPATH, "\\out\\");

            if (!Directory.Exists(outPath))
                Directory.CreateDirectory(outPath);

            var fullPath = $"{outPath}{DateTime.Now:yyMMddHHmmss}_{filename.Replace(".csv", string.Empty)}.txt";

            for (var i = 0; i < data.Length; i++)
                sb.AppendLine(data[i]);

            using var sw = new StreamWriter(fullPath);
            sw.WriteLine(sb);
        }
    }
}