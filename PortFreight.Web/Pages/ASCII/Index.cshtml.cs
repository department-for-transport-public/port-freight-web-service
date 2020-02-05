using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Web.Utilities;
using Microsoft.Extensions.Logging;

namespace PortFreight.Web.Pages.ASCII
{
    public class IndexModel : PageModel
    {
        readonly PortFreightContext _context;
        readonly CloudStorageInit _options;
        readonly StorageClient _storage;
        readonly UserManager<PortFreightUser> _userManager;
        private IHostingEnvironment _hostingEnvironment;
        private readonly ILogger _logger;

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public FileDetails FileDetails { get; set; }

        public IndexModel(
            IOptions<CloudStorageInit> options,
            PortFreightContext context,
            IHostingEnvironment environment,
            UserManager<PortFreightUser> userManager,
            ILogger<IndexModel> logger
        )
        {
            _options = options.Value;
            _storage = StorageClient.Create();
            _context = context;
            _hostingEnvironment = environment;
            _userManager = userManager;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteBucketObject(string FileToDelete)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var TempDataList = TempData.GetKeep<InputModel>("FileList");
            var ToRemove = TempDataList.FileList.SingleOrDefault(a => a.FileName == FileToDelete);

            _options.ObjectName = MakeFileName(FileToDelete, user.SenderId);
            try
            {
                IOrderedEnumerable<Google.Apis.Storage.v1.Data.Object> bucketItems = _storage.ListObjects(_options.BucketName).OrderBy(x => x.TimeCreated);
                if (bucketItems.Any(x => x.Name == _options.ObjectName))
                {
                    await _storage.DeleteObjectAsync(_options.BucketName, _options.ObjectName);
                }
                else
                {
                    _logger.LogError("File does not exist: " + _options.BucketName, _options.ObjectName + " : " + "Please contact help desk for further advice.", string.Empty, "");
                }
 
                FileUploadInfo fileToRemove = _context.FileUploadInfo.Where(x => x.FileName == FileToDelete || x.FileName == _options.ObjectName).LastOrDefault();
                if (fileToRemove != null)
                {
                    TempDataList.FileList.Remove(ToRemove);
                    _context.FileUploadInfo.Remove(fileToRemove);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    string errorMessage = "An error occured while removing the selected file " + FileToDelete + " , please contact help desk for further help";
                    ModelState.AddModelError("fileToRemove.FileName", errorMessage.ToString());
                    _logger.LogError("Removing file error: " + _options.BucketName, _options.ObjectName + " : " + "An error while removing the file, either not found or null parameter passed", string.Empty, "");

                    TempDataList.FileList.ForEach(x => Input.FileList.Add(x));

                    return Page();
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError("CustomError", e.ToString());
                _logger.LogError("Removing file error: " + _options.BucketName, _options.ObjectName + " : " + e.Message, e.InnerException != null ? e.InnerException.Message : string.Empty, e.StackTrace);
                TempData.Put("FileList", TempDataList);

                return Page();
            }

            TempData.Put("FileList", TempDataList);

            return RedirectToPage();
        }

        public IActionResult OnPostSubmitMoreFiles()
        {
            TempData.Remove("FileList");
            return Page();
        }

        public async System.Threading.Tasks.Task<IActionResult> OnPostSaveFiles(List<IFormFile> Files)
        {
            if (Files.Count == 0)
            {
                ModelState.AddModelError("CustomError", "Select the files you wish to submit");
                return Page();
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);

            foreach (var file in Files)
            {
                if (file.ContentType == "text/plain")
                {
                    using (StreamReader streamReader = new StreamReader(file.OpenReadStream(), Encoding.UTF8))
                    {
                        string contents = streamReader.ReadToEnd();

                        _options.ObjectName = MakeFileName(Path.GetFileName(file.FileName), user.SenderId);

                        if (!string.IsNullOrEmpty(contents))
                        {
                            try
                            {
                                await _storage.UploadObjectAsync(
                                    _options.BucketName,
                                    _options.ObjectName,
                                    file.ContentType,
                                    new MemoryStream(Encoding.UTF8.GetBytes(contents))
                                );
                            }
                            catch (Google.GoogleApiException e)
                            {
                                ModelState.AddModelError("CustomError", "Error uploading file");
                                return Page();
                            }

                            FileDetails = new FileDetails
                            {
                                FileName = Path.GetFileName(file.FileName),
                                FileSize = file.Length
                            };

                            Input.FileList.Add(FileDetails);
                        }
                        else
                        {
                            ModelState.AddModelError("BlankFile", "The file \"" +
                                file.FileName + "\" is blank. Please upload another file.");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("NotTXT", "The file \"" +
                       file.FileName + "\" is not a flat file. Please upload in the correct format.");
                }
            }

            Input.FileList.ForEach(f =>
            {
                FileUploadInfo fileLog = new FileUploadInfo
                {
                    FileName = MakeFileName(Path.GetFileName(f.FileName), user.SenderId),
                    UploadBy = _userManager.GetUserName(HttpContext.User),
                    UploadDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                };

                _context.AddAsync(fileLog);
            });

            await _context.SaveChangesAsync();

            TempData.Put("FileList", Input);

            return Page();
        }

        public IActionResult Error()
        {
            return Page();
        }

        private string MakeFileName(string existingFileName, string senderID)
        {
            return string.Concat(existingFileName, "_", senderID, DateTime.Now.ToString("yyyyMMdd"));
        }
    }

    public class InputModel
    {
        public List<FileDetails> FileList { get; set; } = new List<FileDetails>();
    }

    public class FileDetails
    {
        public string FileName;
        public long FileSize;
    }
}