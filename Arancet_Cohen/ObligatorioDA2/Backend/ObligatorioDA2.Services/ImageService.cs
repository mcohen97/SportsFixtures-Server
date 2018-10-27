﻿using ObligatorioDA2.Services.Interfaces;
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

        public void SaveImage(string imageName, string image)
        {
            string path = imagesPath+"/"+imageName + ".jpg";
            byte[] imageBytes = Convert.FromBase64String(image);
            using (FileStream fs = System.IO.File.Create(path))
            {
                fs.Write(imageBytes, 0, imageBytes.Length);
            }
        }
    }
}
