using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WhitespaceStats
{
    class Program
    {
        private static Predicate<string> _directoryNameFilter = x => !x.StartsWith(".");
        private static Predicate<string> _fileNameFilter = x => true;

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Print whitespace statistics for the files in the given directory");
                Console.WriteLine();
                Console.WriteLine("Usage: {0} <dir>", Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location));
                return;
            }

            Console.WriteLine(String.Join(";", _columns.Select(x => x.Name)));

            foreach (var filePath in SearchFilesRecursively(args[0]))
            {
                AnalyzeFile(filePath);
            }
        }

        private static List<string> SearchFilesRecursively(string path)
        {
            var files = Directory.EnumerateFiles(path).Where(x => _fileNameFilter(Path.GetFileName(x))).OrderBy(x => Path.GetFileName(x)).ToList();
            var directories = Directory.EnumerateDirectories(path).Where(x => _directoryNameFilter(Path.GetFileName(x))).OrderBy(x => Path.GetFileName(x));
            files.AddRange(directories.SelectMany(x => SearchFilesRecursively(x)));

            return files;
        }

        private static void AnalyzeFile(string path)
        {
            try
            {
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var characteristics = FileCharacteristics.Analyze(fileStream);
                    Console.WriteLine(String.Join(";", _columns.Select(x => x.ValueFunc(path, characteristics))));
                }
            }
            catch (IOException)
            {
                Console.Error.WriteLine("Could not access file: {0}", path);
            }
        }

        private class Column
        {
            public string Name { get; set; }
            public Func<string, FileCharacteristics, string> ValueFunc { get; set; }
        }

        private static string ExtractProperty(FileCharacteristics fileCharacteristics, Func<TextStatistics, object> extractFunc)
        {
            if (fileCharacteristics.TextStatistics != null)
            {
                return extractFunc(fileCharacteristics.TextStatistics).ToString();
            }
            else
            {
                return "";
            }
        }

        private static Column[] _columns = new[]
        {
            new Column { Name = "Directory",                    ValueFunc = (path, characteristics) => Path.GetDirectoryName(path) },
            new Column { Name = "File",                         ValueFunc = (path, characteristics) => Path.GetFileName(path) },
            new Column { Name = "Type",                         ValueFunc = (path, characteristics) => characteristics.EncodingName },
            new Column { Name = "All lines",                    ValueFunc = (path, characteristics) => ExtractProperty(characteristics, x => x.AllLines) },
            new Column { Name = "LF ending lines",              ValueFunc = (path, characteristics) => ExtractProperty(characteristics, x => x.LfLines) },
            new Column { Name = "CR+LF ending lines",           ValueFunc = (path, characteristics) => ExtractProperty(characteristics, x => x.CrLfLines) },
            new Column { Name = "Leading spaces lines",         ValueFunc = (path, characteristics) => ExtractProperty(characteristics, x => x.LeadingSpacesLines) },
            new Column { Name = "Leading tabs lines",           ValueFunc = (path, characteristics) => ExtractProperty(characteristics, x => x.LeadingTabsLines) },
            new Column { Name = "Leading mixed lines",          ValueFunc = (path, characteristics) => ExtractProperty(characteristics, x => x.LeadingMixedLines) },
            new Column { Name = "Non-leading tabs lines",       ValueFunc = (path, characteristics) => ExtractProperty(characteristics, x => x.NonLeadingTabsLines) },
            new Column { Name = "Trailing whitespace lines",    ValueFunc = (path, characteristics) => ExtractProperty(characteristics, x => x.TrailingWhitespaceLines) },
            new Column { Name = "Any sole CR lines",            ValueFunc = (path, characteristics) => ExtractProperty(characteristics, x => x.AnySoleCrLines) },
        };
    }
}
