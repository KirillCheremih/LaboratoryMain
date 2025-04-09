using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows;

namespace LaboratoryMain.Classes
{
    class imageHelper
    {
        public static string ProjectPath
        {
            get
            {
                string imagePath = Environment.CurrentDirectory;
                return imagePath.Remove(imagePath.Length - 10, 10);
            }
        }

        public static string UNKNOWN_IMG = $"{ProjectPath}\\Res\\unknown.png";

        public static string getFilePath(string fileName, string folder = "Res")
        {
            return (fileName == null) ? UNKNOWN_IMG : $"{ProjectPath}\\{folder}\\{fileName}";
        }

        public static BitmapImage getImageByName(string fileName, string resourceFolder)
        {
            try
            {
                return new BitmapImage(new Uri(getFilePath(fileName, resourceFolder)));
            }
            catch
            {
                return new BitmapImage(new Uri(UNKNOWN_IMG));
            }
        }

        // FilePaths
        public static bool addOrReplaceFile(string selectedPath, string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);

                File.Copy(selectedPath, filePath);
                MessageBox.Show($"Изображение заменено на <{filePath}>");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}");
                return false;
            }
        }

        public static bool checkFileExists(string path)
        {
            return File.Exists(path);
        }

        public static string getNewFilename(string fileName, string filePath)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string fileExtension = Path.GetExtension(fileName);

            string newFileName;
            int i = 1;

            do
            {
                newFileName = $"{filePath}\\{fileNameWithoutExtension}_{i}.{fileExtension}";
                i++;
            }
            while (File.Exists(newFileName));

            return newFileName;
        }
    }
}
