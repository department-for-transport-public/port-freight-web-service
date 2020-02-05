using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.FileProcess.ASCII;
using PortFreight.FileProcess.GESMES;
using PortFreight.FileProcessor.Common;
using PortFreight.Services.EmailSender;
using PortFreight.Services.FileProcess;
using PortFreight.Services.MSD1;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace PortFreight.FileProcess.Common
{
    public class FileProcessing
    {
        private readonly ILogger _logger;
        private ASCIIFileProcess _asciiFileProcess;
        private GESMESFileProcess _gesmesFileProcess;
        private EmailNotification _emailNotification;
        private string senderId = string.Empty;
        private FileOptions _settings;

        public FileProcessing(FlatFile flatFile,
                              Msd1Data msd1Data,
                              EmailNotification emailNotification,
                              IFileProcessService fileProcessService,
                              IMsd1DataService msd1DataService,
                              ILogger<FileProcessing> logger,
                              ASCIIFileProcess asciiFileProcess,
                              IOptions<FileOptions> settings,
                              GESMESFileProcess gesmesFileProcess

                              )
        {
            _logger = logger;
            _asciiFileProcess = asciiFileProcess;
            _settings = settings.Value;
            _gesmesFileProcess = gesmesFileProcess;
            _emailNotification = emailNotification;
        }

        public void ProcessFile(string fileType)
        {
            string filebucket = fileType.Equals("ASCII") ? _settings.AsciiBucket : _settings.GesmesBucket;
            string archivedBucket = fileType.Equals("ASCII") ? _settings.ArchivedAsciiBucket : _settings.ArchivedGesmesBucket;
            var files = DownloadFilesFromBucket(filebucket);
            foreach (var filename in files)
            {
                try
                {
                    var file = Path.GetFileName(filename);
                    if (fileType.Equals("ASCII"))
                    {
                        _asciiFileProcess.InitiateAsciiFileProcess(file);
                    }
                    else
                    {
                        _gesmesFileProcess.InitiateGesmesFileProcess(file);
                    }
                }

                catch (Exception err)
                {
                    _logger.LogError("File processing error:", err.Message, err.InnerException != null ? err.InnerException.Message : string.Empty, err.StackTrace);
                }
            }
        }
        public List<string> DownloadFilesFromBucket(string fileBucket)
        {
            StorageClient storageClient = StorageClient.Create();
            List<string> files = new List<string>();

            foreach (var storageItem in storageClient.ListObjects(fileBucket).OrderBy(x => x.TimeCreated))
            {
                try
                {
                    var fileStream = File.OpenWrite(Path.Combine(Environment.CurrentDirectory, storageItem.Name));
                    storageClient.DownloadObject(fileBucket, storageItem.Name, fileStream);
                    files.Add(fileStream.Name);
                    fileStream.Close();
                }
                catch (Exception err)
                {
                    _logger.LogError("Invalid filename error: " + storageItem.Name + " : " + err.Message, err.InnerException != null ? err.InnerException.Message : string.Empty, err.StackTrace);
                }
            }
            return files;
        }
    }
}
