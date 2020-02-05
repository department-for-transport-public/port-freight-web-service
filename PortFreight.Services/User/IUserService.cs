using System.Collections.Generic;
using PortFreight.Data;

namespace PortFreight.Services.User
{
    public interface IUserService
    {
        bool IsUserRegistered(string email);
        bool IsSenderIdRegistered(string senderId);
        bool IsSenderIdValidDataSupplier(string senderId);
        List<PortFreightUser> GetUsersListByEmailOrSenderID(string SearchParam);
        List<PortFreightUser> GetDisabledUsers();
        void ReinstateUser(string id);
        void DisableUser(string id);
        List<string> GetUsersBySenderID(string senderId);
    }
}
