using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortFreight.FileProcess.Common;

namespace PortFreight.FileProcessor.Pages
{
    [BindProperties]
    public class IndexModel : PageModel
    {
        private FileProcessing _fileProcessing;
        public string FileStatus { get; set; }
        public IndexModel(FileProcessing fileProcessing)
        {
            _fileProcessing = fileProcessing;
        }
        public void OnGet()
        {
            FileStatus = "Start File Processing";
            _fileProcessing.ProcessFile("ASCII");
            _fileProcessing.ProcessFile("GESMES");
            FileStatus = "Complete";
        }

        public void OnPost()
        {              
            _fileProcessing.ProcessFile("ASCII");
            _fileProcessing.ProcessFile("GESMES");
            FileStatus = "Complete";                       
        }
    } 
}