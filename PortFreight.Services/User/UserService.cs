using System;
using System.Collections.Generic;
using System.Linq;
using PortFreight.Data;

namespace PortFreight.Services.User
{
    public class UserService : IUserService
    {

        private readonly UserDbContext _userContext;
        private readonly PortFreightContext _portFreightContext;
        private PortFreightUser PortFreightUser;

        public UserService(UserDbContext userDbContext, PortFreightContext portFreightContext)
        {
            _userContext = userDbContext;
            _portFreightContext = portFreightContext;
        }

        public bool IsUserRegistered(string email)
        {
            return _userContext.Users.Any(
                x => x.Email == email
                && x.EmailConfirmed == true
                && (x.LockoutEnd == null || x.LockoutEnd < DateTime.Now));
        }

        public List<PortFreightUser> GetUsersListByEmailOrSenderID(string SearchParam)
        {
            return _userContext.Users.Where(
                x => (x.Email.Contains(SearchParam) || x.SenderId.Contains(SearchParam))
                && x.EmailConfirmed == true
                && (x.LockoutEnd == null || x.LockoutEnd < DateTime.Now))
                .OrderBy(x => x.SenderId)
                .ToList();
        }

        public bool IsSenderIdRegistered(string senderId)
        {
            return _userContext.Users.Any(
                x => x.SenderId == senderId
                && x.EmailConfirmed == true
                && (x.LockoutEnd == null || x.LockoutEnd < DateTime.Now));
        }
        
        public bool IsSenderIdValidDataSupplier(string senderId)
        {
            return _portFreightContext.OrgList.Any(
                x => x.SubmitsMsd1 == true
                || x.SubmitsMsd2 == true
                || x.SubmitsMsd3 == true
                || x.SubmitsMsd4 == true
                || x.SubmitsMsd5 == true);
        }

        public List<PortFreightUser> GetDisabledUsers()
        {
            return _userContext.Users.Where(x => x.LockoutEnd != null && x.LockoutEnd > DateTime.Now).ToList();
        }

        public void ReinstateUser(string id)
        {
            PortFreightUser = _userContext.Users.FirstOrDefault(x => x.Id == id);

            PortFreightUser.LockoutEnd = null;

            UpdateAndSave();
        }

        public void DisableUser(string id)
        {
            PortFreightUser = _userContext.Users.FirstOrDefault(x => x.Id == id);

            PortFreightUser.LockoutEnd = DateTime.Now.AddMonths(18);

            UpdateAndSave();
        }

        private void UpdateAndSave()
        {
            _userContext.Update(PortFreightUser);
            _userContext.SaveChanges();
        }

        public List<string> GetUsersBySenderID(string senderId)
        {
            return _userContext.Users.Where(
                x => (x.SenderId.Equals(senderId))).Select(x => x.UserName).ToList();
                
        }
    }
}
