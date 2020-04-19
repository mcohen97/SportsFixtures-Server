using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.Data.Repositories.Contracts;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.Services.Contracts;
using ObligatorioDA2.Services.Contracts.Dtos;


namespace ObligatorioDA2.Services.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SportServiceTest
    {
        private ISportService serviceToTest;
        private Mock<ISportRepository> sportsStorage;
        private Mock<ITeamRepository> teamsStorage;
        private Mock<IAuthenticationService> authentication;
        private Sport testSport;
        private SportDto testDto;

        [TestInitialize]
        public void SetUp() {
            sportsStorage = new Mock<ISportRepository>();
            teamsStorage = new Mock<ITeamRepository>();
            authentication = new Mock<IAuthenticationService>();
            serviceToTest = new SportService(sportsStorage.Object, teamsStorage.Object, authentication.Object);
            testSport = new Sport("Sport", true);
            testDto = new SportDto() { name = testSport.Name, isTwoTeams = testSport.IsTwoTeams };
        }

        [TestMethod]
        public void GetAllTest() {
            GrantFollowerPermissions();
            sportsStorage.Setup(r => r.GetAll()).Returns(new List<Sport>() { testSport, testSport, testSport });
            ICollection<SportDto> result= serviceToTest.GetAllSports();
            authentication.Verify(r => r.Authenticate(), Times.Once);
            authentication.Verify(r => r.AuthenticateAdmin(), Times.Never);
            sportsStorage.Verify(r => r.GetAll(), Times.Once);
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(NotAuthenticatedException))]
        public void GetAllNoAuthTest() {
            LogOut();
            serviceToTest.GetAllSports();
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void GetAllNoDataAccessTest()
        {
            GrantFollowerPermissions();
            sportsStorage.Setup(r => r.GetAll()).Throws(new DataInaccessibleException());
            serviceToTest.GetAllSports();
        }

        [TestMethod]
        public void GetSportTest() {
            GrantFollowerPermissions();
            sportsStorage.Setup(r => r.Get(testSport.Name)).Returns(testSport);
            SportDto result = serviceToTest.GetSport(testSport.Name);
            authentication.Verify(r => r.Authenticate(), Times.Once);
            authentication.Verify(r => r.AuthenticateAdmin(), Times.Never);
            sportsStorage.Verify(r => r.Get(testSport.Name), Times.Once);
            Assert.AreEqual(testSport.Name, result.name);
            Assert.AreEqual(testSport.IsTwoTeams, result.isTwoTeams);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void GetSportNotFoundTest() {
            GrantFollowerPermissions();
            sportsStorage.Setup(r => r.Get(testSport.Name)).Throws(new SportNotFoundException());
            serviceToTest.GetSport(testSport.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void GetSportNoDataAccess() {
            GrantFollowerPermissions();
            sportsStorage.Setup(r => r.Get(testSport.Name)).Throws(new DataInaccessibleException());
            serviceToTest.GetSport(testSport.Name);
        }

        [TestMethod]
        public void AddSportTest() {
            GrantAdminPermissions();
            SportDto result = serviceToTest.AddSport(testDto);
            sportsStorage.Verify(r => r.Add(It.IsAny<Sport>()), Times.Once);
            Assert.AreEqual(testSport.Name,result.name);
            Assert.AreEqual(testSport.IsTwoTeams, result.isTwoTeams);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void AddInvalidFormatSportTest() {
            GrantAdminPermissions();
            testDto.name = null;
            serviceToTest.AddSport(testDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void AddAlreadyExistentSportTest() {
            GrantAdminPermissions();
            sportsStorage.Setup(r => r.Add(It.IsAny<Sport>())).Throws(new SportAlreadyExistsException());
            serviceToTest.AddSport(testDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void AddSportNoDataAccessTest()
        {
            GrantAdminPermissions();
            sportsStorage.Setup(r => r.Add(It.IsAny<Sport>())).Throws(new DataInaccessibleException());
            serviceToTest.AddSport(testDto);
        }

        [TestMethod]
        [ExpectedException(typeof(NotAuthenticatedException))]
        public void AddSportNoAuthenticationTest() {
            LogOut();
            serviceToTest.AddSport(testDto);
        }

        [TestMethod]
        [ExpectedException(typeof(NoPermissionsException))]
        public void AddSportNoPermissionsTest()
        {
            GrantFollowerPermissions();
            serviceToTest.AddSport(testDto);
        }

        [TestMethod]
        [ExpectedException(typeof(NotAuthenticatedException))]
        public void GetSportNoAuthTest()
        {
            LogOut();
            serviceToTest.GetSport(testSport.Name);
        }

        [TestMethod]
        public void DeleteTest() {
            GrantAdminPermissions();
            serviceToTest.DeleteSport(testSport.Name);
            sportsStorage.Verify(r => r.Delete(testSport.Name), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void DeleteNotExistentSportTest() {
            GrantAdminPermissions();
            sportsStorage.Setup(r => r.Delete(testSport.Name)).Throws(new SportNotFoundException());
            serviceToTest.DeleteSport(testSport.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void DeleteNoDataAccessTest() {
            GrantAdminPermissions();
            sportsStorage.Setup(r => r.Delete(testSport.Name)).Throws(new DataInaccessibleException());
            serviceToTest.DeleteSport(testSport.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(NoPermissionsException))]
        public void DeleteSportNoPermissionsTest()
        {
            GrantFollowerPermissions();
            serviceToTest.DeleteSport(testSport.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(NotAuthenticatedException))]
        public void DeleteSportNoAuthTest()
        {
            LogOut();
            serviceToTest.DeleteSport(testSport.Name);
        }

        private void GrantAdminPermissions()
        {
            //authentication.Setup(r => r.IsLoggedIn()).Returns(true);
            //authentication.Setup(r => r.HasAdminPermissions()).Returns(true);
        }
        private void GrantFollowerPermissions()
        {
            //authentication.Setup(r => r.IsLoggedIn()).Returns(true);
            authentication.Setup(r => r.AuthenticateAdmin()).Throws(new NoPermissionsException());
        }

        private void LogOut()
        {
            authentication.Setup(r => r.Authenticate()).Throws(new NotAuthenticatedException());
            authentication.Setup(r => r.AuthenticateAdmin()).Throws(new NotAuthenticatedException());
        }
    }
}
