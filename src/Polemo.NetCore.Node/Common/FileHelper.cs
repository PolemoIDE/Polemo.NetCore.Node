using System.Collections.Generic;
using System.IO;

namespace Polemo.NetCore.Node.Common
{
    public class FileHelper
    {
        public static IEnumerable<string> SearchAllFiles(string path, string fileName)
        {
            return Directory.EnumerateFiles(path, fileName, SearchOption.AllDirectories);
        }

        public static string GetFileRelativeDirectory(string projectPath, string fileFullName)
        {
            var projectDirectory = new DirectoryInfo(projectPath);
            var file = new FileInfo(fileFullName);
            var fileDirectory = file.DirectoryName;
            return fileDirectory.Remove(0, projectDirectory.FullName.Length);
        }

        public static string GetFileDirectoryName(string fileFullName)
        {
            var file = new FileInfo(fileFullName);
            return file.Directory.Name;
        }
    }
}
