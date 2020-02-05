using PortFreight.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PortFreight.Services.FileProcess
{
    public interface IFileProcessService
    {
        int Add(FlatFile file);

        string GetUserName(string senderId);

        void AddLogFileData(LogFileData logFileData);

        void UpdateLogFileData(LogFileData logFileData);

        List<LogFileData> GetLoggedFileData(string senderId);

        bool CheckFileExist(FlatFile file);

        int GetLastInvalidFileRefId(FlatFile file);

        IQueryable<string> GetAllUsers(string senderId);

        List<string> GetAllSenderIds();

        string GetUserByFileName(string filename);

        string GetSenderIdByFileName(string filename);
    }
}
