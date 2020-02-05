using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace PortFreight.Data.Tests.UserAccount
{
    [TestClass]
    public class UserRegistrationTest
    {

        [TestMethod]
        public async Task OnRegister_WhenPassValidUser_AddValidUser()
        {                      
            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryUserDbContext")
                .Options;

            var user = new PortFreightUser() { UserName = "TestUser3",
                                               SenderId = "Test1234",
                                               Email = "user3@dft.gov.uk",
                                                PasswordHash = "TestTest1!" };

            using (var context = new UserDbContext(options))
            {
                await context.Users.AddAsync(user);
                context.SaveChanges();                
            }
            
            using (var context = new UserDbContext(options))
            {
                Assert.AreEqual(1, context.Users.Count());
                Assert.AreEqual(user.UserName, context.Users.FirstOrDefault(x => x.UserName == user.UserName).UserName);
            }
        }
    }
}
