using ObligatorioDA2.Services.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ObligatorioDA2.Services
{
    public class ImageService : IImageService
    {
        private string imagesPath;

        public ImageService(string aPath)
        {
            imagesPath = aPath;
            Directory.CreateDirectory(imagesPath);
        }

        public byte[] ReadImage(string path)
        {
            byte[] data;
            try
            {
                data = TryRead(path);
            }
            catch (IOException) {
                data = new byte[0];
            }
            return data;
        }

        private byte[] TryRead(string path)
        {
            byte[] bytes = new byte[0];
            string fullPath = imagesPath + "/" + path;
            using (Stream source = File.OpenRead(fullPath))
            {
                bytes = new byte[source.Length];
                source.Read(bytes, 0, bytes.Length);
            }
            return bytes;
        }

        public string SaveImage(string imageName, string image)
        {
            string path = imagesPath+"/"+imageName;
            byte[] imageBytes = Convert.FromBase64String(image);
            using (FileStream fs = File.Create(path))
            {
                fs.Write(imageBytes, 0, imageBytes.Length);
            }
            return path;
        }
    }
}
