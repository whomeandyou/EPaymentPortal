using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.Batch.Service
{
    public static class FileServices
    {
        public static List<string> GetFilesPath(string path)
        {
            return Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).ToList();
        }

        public static string GetFileName(string path)
        {
            return String.Format("{0}{1}", Path.GetFileNameWithoutExtension(path), Path.GetExtension(path));
        }

        public static string GetFileNameWithoutExt(string path)
        {
            return String.Format("{0}", Path.GetFileNameWithoutExtension(path));
        }

        public static void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public static void MoveFile(string oldPath, string newPath)
        {
            if (File.Exists(oldPath))
            {
                if (File.Exists(newPath))
                {
                    File.Delete(newPath);
                }
                File.Move(oldPath, newPath);
            }
        }

        public static void CopyFile(string oldPath, string newPath)
        {
            File.Copy(oldPath, newPath);
        }

        public static bool IsExist(string path)
        {
            return File.Exists(path);
        }

        public static string[] ReadStringFromFile(string path)
        {
            return File.ReadAllLines(path);
        }

        public static void WriteToFile(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        public static void WriteToExcel(string path, byte[] content)
        {
            File.WriteAllBytes(path, content);
        }

        public static bool DirectoryExist(string[] path)
        {
            var check = true;
            for(var i = 0; i < path.Count(); i++)
            {
                if (!Directory.Exists(path[i]))
                {
                    Directory.CreateDirectory(path[i]);
                    check = false;
                }
            }
            return check;
        }

        public static void WriteLineIntoFile(string path,string line)
        {
            if (!File.Exists(path))
                File.Create(path);
            var test = File.AppendText(path);
            test.WriteLine(line);
            test.Close();
        }

        public static void DeleteFolder(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            Directory.Delete(path);
        }
    }
}
