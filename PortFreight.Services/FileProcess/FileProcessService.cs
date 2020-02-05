using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using PortFreight.Data;
using PortFreight.Data.Entities;

namespace PortFreight.Services.FileProcess
{
    public class FileProcessService : IFileProcessService
    {
        private readonly PortFreightContext _context;
        private readonly UserDbContext _userContext;       

        public FileProcessService(PortFreightContext context, UserDbContext userContext)
        {
            _context = context;
            _userContext = userContext;            
        }
        public int Add(FlatFile file)
        {
            int fileRefId = 0;
            if (file != null)
            {
                _context.FlatFile.Add(file);
                _context.SaveChanges();
                fileRefId = _context.FlatFile.OrderBy(x => x.FileRefId).LastOrDefault().FileRefId;
            }
            return fileRefId;
        }

        public void AddLogFileData(LogFileData logFileData)
        {
            _context.LogfileData.Add(logFileData);
            _context.SaveChanges();
            _context.Entry(logFileData).State = EntityState.Detached;
        }

        public List<LogFileData> GetLoggedFileData(string senderId)
        {
            var loggedDescription = _context.FlatFile.
                                      Join(_context.LogfileData, f => f.FileRefId, lfd => lfd.FileRefId,
                                      (f, lfd) => new { f, lfd })
                                      .Where(x =>x.f.SenderId == senderId && x.lfd.FileRefId == x.f.FileRefId && !x.lfd.IsEmailed)
                                      .Select(x => x.lfd).ToList();

            return loggedDescription;
        }

        public List<string> GetAllSenderIds()
        {
            var senderIds = _context.FlatFile.
                                      Join(_context.LogfileData, f => f.FileRefId, lfd => lfd.FileRefId,
                                      (f, lfd) => new { f, lfd })
                                      .Where(x => x.lfd.FileRefId == x.f.FileRefId && !x.lfd.IsEmailed)
                                      .Select(x => x.f.SenderId).Distinct().ToList();

            return senderIds;
        }

        public bool CheckFileExist(FlatFile file)
        {
            bool result = false;
            if (file != null)
            {
                result = _context.FlatFile.Any(x =>
                                         x.SenderId == file.SenderId                                                                                
                                        && x.TableRef == file.TableRef
                                        && x.SendersRef == file.SendersRef
                                        && file.IsAmendment.Value == 0);
                return result;
            }
            return result;
        }

        public string GetUserName(string senderId)
        {
            var user = _userContext.Users.Where(x => x.SenderId == senderId).FirstOrDefault();
            if (user == null) {
                return string.Empty;
            }
            return user.Email;
        }

        public IQueryable<string> GetAllUsers(string senderId)
        {
            var users= _userContext.Users.Where(x => x.SenderId == senderId).Select(x => x.UserName);
            return users;
        }

        public int GetLastInvalidFileRefId(FlatFile file)
        {
            var flatFile = _context.FlatFile.LastOrDefault(x => x.SenderId == file.SenderId   
                                        && x.TableRef == file.TableRef
                                        && x.SendersRef == file.SendersRef
                                        );

            var fileRefId = flatFile != null ? flatFile.FileRefId : -1;

            return fileRefId;
        }

        public void UpdateLogFileData(LogFileData logFileData)
        {
            _context.LogfileData.Update(logFileData);
            _context.SaveChanges();
            _context.Entry(logFileData).State = EntityState.Modified;
        }

        public string GetUserByFileName(string filename)
        {           
            var fileUploadInfo = _context.FileUploadInfo.LastOrDefault(x => x.FileName == filename);
            var userName = fileUploadInfo == null ? string.Empty : fileUploadInfo.UploadBy;
            return userName;
        }

        public string GetSenderIdByFileName(string filename)
        {
            var userName = GetUserByFileName(filename);
            var senderId = string.IsNullOrEmpty(userName)? string.Empty : _userContext.Users.FirstOrDefault(x => x.UserName == userName).SenderId;
            return senderId;
        }
    }
}
