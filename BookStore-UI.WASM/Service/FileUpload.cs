﻿using BlazorInputFile;
using BookStore_UI.WASM.Contracts;
//using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BookStore_UI.WASM.Service
{
    /*
    public class FileUpload : IFileUpload
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileUpload(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public void RemoveFile(string picName)
        {
            if(picName != null)
            {
                var path = $"{_webHostEnvironment.WebRootPath}\\uploads\\{picName}";
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        public async Task UploadFile(IFileListEntry file, MemoryStream memoryStream, string picName)
        {
            try
            {
                await file.Data.CopyToAsync(memoryStream);

                var path = $"{_webHostEnvironment.WebRootPath}\\uploads\\{picName}";

                using FileStream fileStream = new FileStream(path, FileMode.Create);
                memoryStream.WriteTo(fileStream);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
    */
}
