using CWI.Desafio2.Application.FileManager.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CWI.Desafio2.Application.FileManager
{
    public class FileManagerAppService
    {
        private readonly string HOMEPATH = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "/data");

        public FileManagerAppService()
        {
            if (!Directory.Exists(HOMEPATH))
                Directory.CreateDirectory(HOMEPATH);
        }

        public List<FileViewModel> ReadFiles()
        {
            try
            {
                var inPath = string.Concat(HOMEPATH, "/in");

                if (!Directory.Exists(inPath))
                    Directory.CreateDirectory(inPath);

                var filenames = Directory.GetFiles(inPath, @"*.csv", SearchOption.TopDirectoryOnly);

                var files = new List<FileViewModel>();

                foreach (var file in filenames)
                {
                    var content = new List<string>();

                    using var reader = new StreamReader(file);

                    while (reader.Peek() >= 0)
                        content.Add(reader.ReadLine());

                    if (content.Any())
                        files.Add(new FileViewModel()
                        {
                            Content = content.ToArray(),
                            Filename = file
                        });
                }

                return files;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void WriteFile(string[] data, string filename)
        {
            var sb = new StringBuilder();

            var outPath = string.Concat(HOMEPATH, "/out");

            if (!Directory.Exists(outPath))
                Directory.CreateDirectory(outPath);

            var fullPath = $"{outPath}/{DateTime.Now:yyMMdd_HHmmss}_{filename}.csv";

            for (var i = 0; i < data.Length; i++)
                sb.AppendLine(data[i]);

            using var sw = new StreamWriter(fullPath);
            sw.WriteLine(sb);
        }
    }
}