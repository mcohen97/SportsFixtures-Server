using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace ObligatorioDA2.BusinessLogic.Test
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class UserFactoryTest
    {
        UserId identity;
        User anAdmin;
        User factoryAdmin;
        User aFollower;
        User factoryFollower;
        UserFactory factory;


        [TestInitialize]
        public void SetUp()
        {
            identity = new UserId()
            {
                Name = "name",
                Surname = "surname",
                UserName = "username",
                Password = "password",
                Email = "mail@domain.com"
            };

            factory = new UserFactory();
            anAdmin = new User(identity, true);
            factoryAdmin = factory.CreateAdmin(identity);
            aFollower = new User(identity, false);
            factoryFollower = factory.CreateFollower(identity);
        }

        [TestMethod]
        public void CreateAdminNameTest()
        {
            Assert.AreEqual(factoryAdmin.Name, anAdmin.Name);
        }

        [TestMethod]
        public void CreateAdminSurnameTest()
        {
            Assert.AreEqual(factoryAdmin.Surname, anAdmin.Surname);
        }

        [TestMethod]
        public void CreateAdminUserNameTest()
        {
            Assert.AreEqual(factoryAdmin.UserName, anAdmin.UserName);
        }

        [TestMethod]
        public void CreateAdminPasswordTest()
        {
            Assert.AreEqual(factoryAdmin.Password, anAdmin.Password);
        }

        [TestMethod]
        public void CreateAdminEmailTest()
        {
            Assert.AreEqual(factoryAdmin.Email, anAdmin.Email);
        }

        [TestMethod]
        public void IsAdminTest()
        {
            Assert.IsTrue(factoryAdmin.IsAdmin);
        }

        [TestMethod]
        public void IsNotAdmin()
        {
            Assert.IsFalse(factoryFollower.IsAdmin);
        }
    }
}
