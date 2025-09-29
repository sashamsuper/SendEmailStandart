using System;
using System.IO;
using System.IO.Compression;

namespace PostalService
{
    // <summary>
    // Класс для создания zip-архива директории
    // </summary>
    public class ZipDir
    {
        // <summary>
        // Путь к директории, которую нужно запаковать
        // </summary>
        public string PathToZip { set; get; }

        // <summary>
        // Путь к временной директории, куда будет создан zip-архив
        // </summary>
        public string DirToTmpZip { set; get; } = "TmpZip";

        private string extractFile = null;

        // <summary>
        // Путь к файлу, который будет создан
        // </summary>
        public string ExtractFile
        {
            get
            {
                extractFile ??= FileNameForExport("zip");
                return extractFile;
            }
        }

        // <summary>
        // Создает zip-архив
        // </summary>
        // <param name="pathToZip">Путь к директории, которую нужно запаковать</param>
        public ZipDir(string pathToZip)
        {
            PathToZip = pathToZip;
            FindExtractPath();
            if (String.IsNullOrWhiteSpace(PathToZip) || String.IsNullOrWhiteSpace(ExtractFile))
            {
                return;
            }

            ZipFile.CreateFromDirectory(PathToZip, ExtractFile);
            System.IO.DirectoryInfo di = new(PathToZip);
            foreach (FileInfo file in di.GetFiles())
            {
                if (Path.GetExtension(file.Name) == ".xlsx")
                    file.Delete();
            }
        }

        // <summary>
        // Создает директорию, если она не существует
        // </summary>
        private void FindExtractPath()
        {
            if (String.IsNullOrWhiteSpace(ExtractFile))
                return;
            string dir = Path.GetDirectoryName(ExtractFile);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        // <summary>
        // Создает имя файла для экспорта
        // </summary>
        // <param name="rashirenie">Расширение файла</param>
        // <returns>Имя файла</returns>
        private string FileNameForExport(string rashirenie)
        {
            string filename;
            string nowString = DateTime.Now.ToString("yyyy_MM_dd_HH_mm");
            int i = 0;
            //Console.WriteLine($"Curr{AppDomain.CurrentDomain.BaseDirectory}");
            //Console.WriteLine($"Syste.Io{System.IO.Directory.GetCurrentDirectory()}");
            string basepath = System.IO.Directory.GetCurrentDirectory();
            string dirPath = Path.Combine(basepath, DirToTmpZip);
            if (!File.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
            filename = Path.Combine(dirPath, "Pack" + nowString + "_" + i + "." + rashirenie);
            while (System.IO.File.Exists(filename))
            {
                filename = Path.Combine(dirPath, "Pack" + nowString + "_" + i + "." + rashirenie);
                i++;
            }
            return filename;
        }
    }
}
