using System;
using DataAccess;
using DataRepositories;
using DataRepositories.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ObligatorioDA2.DataAccess.Entities;
using RepositoryInterface;

namespace DataRepositoriesTest
{
    [TestClass]
    public class GenericRepositoryTest
    {
        IEntityRepository<UserEntity> testRepo;
        Mock<UserEntity> testEntity;
        [TestInitialize]
        public void SetUp()
        {
            InitializeRepository();
            testEntity = new Mock<UserEntity>();
            testEntity.Setup(u => u.Id).Returns(3);
            testRepo.Clear();
        }

        private void InitializeRepository()
        {
            DbContextOptions<DatabaseConnection> options = new DbContextOptionsBuilder<DatabaseConnection>()
                .UseInMemoryDatabase(databaseName: "UserRepository")
                .Options;
            DatabaseConnection context = new DatabaseConnection(options);
            testRepo = new GenericRepository<UserEntity>(context);
        }

        [TestMethod]
        public void AddTest() {
            testRepo.Add(testEntity.Object);
            Assert.IsFalse(testRepo.IsEmpty());
        }

        [TestMethod]
        [ExpectedException(typeof(EntityAlreadyExistsException))]
        public void AddAlreadyexistingtest() {
            testRepo.Add(testEntity.Object);
            testRepo.Add(testEntity.Object);
        }

        [TestMethod]
        public void GetTest() {
            testRepo.Add(testEntity.Object);
            BaseEntity fromRepo = testRepo.Get(3);
            Assert.AreEqual(fromRepo.Id, 3);
        }

        [TestMethod]
        [ExpectedException(typeof(EntityNotFoundException))]
        public void GetNotExistentTest() {
            BaseEntity fromRepo = testRepo.Get(3);
        }

        [TestMethod]
        public void ClearTest() {
            testRepo.Add(testEntity.Object);
            testRepo.Clear();
            Assert.IsTrue(testRepo.IsEmpty());
        }

        [TestMethod]
        public void AnyPredicateTrueTest() {
            Mock<UserEntity> otherEntity = new Mock<UserEntity>();
            otherEntity.SetupGet(e => e.Id).Returns(4);
            testRepo.Add(testEntity.Object);
            testRepo.Add(otherEntity.Object);
            bool any = testRepo.Any(u => u.Id == 4);
            Assert.IsTrue(any);
        }

        [TestMethod]
        public void AnyPredicateFalseTest()
        {
            Mock<UserEntity> otherEntity = new Mock<UserEntity>();
            otherEntity.SetupGet(e => e.Id).Returns(4);
            testRepo.Add(testEntity.Object);
            testRepo.Add(otherEntity.Object);
            bool any = testRepo.Any(u => u.Id == 5);
            Assert.IsFalse(any);
        }

        [TestMethod]
        public void DeleteTest() {
            testRepo.Add(testEntity.Object);
            testRepo.Delete(testEntity.Object.Id);
            Assert.IsTrue(testRepo.IsEmpty());
        }

        [TestMethod]
        [ExpectedException(typeof(EntityNotFoundException))]
        public void DeleteNonExistent() {
            testRepo.Delete(5);
        }

        [TestMethod]
        public void FirstTest() {
            Mock<UserEntity> otherEntity = new Mock<UserEntity>();
            otherEntity.SetupGet(e => e.Id).Returns(4);
            testRepo.Add(testEntity.Object);
            testRepo.Add(otherEntity.Object);
            BaseEntity result = testRepo.First(e => e.Id == 4);
            Assert.AreEqual(result.Id, 4);
        }

        [TestMethod]
        [ExpectedException(typeof(EntityNotFoundException))]
        public void FirstNotFoundTest() {
            Mock<UserEntity> otherEntity = new Mock<UserEntity>();
            otherEntity.SetupGet(e => e.Id).Returns(4);
            testRepo.Add(testEntity.Object);
            testRepo.Add(otherEntity.Object);
            BaseEntity result = testRepo.First(e => e.Id == 5);
        }

        [TestMethod]
        public void GetAlltest() {
            Mock<UserEntity> otherEntity = new Mock<UserEntity>();
            otherEntity.SetupGet(e => e.Id).Returns(4);
            testRepo.Add(testEntity.Object);
            testRepo.Add(otherEntity.Object);
            int actualResult = testRepo.GetAll().Count;
            Assert.AreEqual(actualResult,2);
        }

        [TestMethod]
        public void GetFilteredTest() {
            Mock<UserEntity> otherEntity2 = new Mock<UserEntity>();
            otherEntity2.SetupGet(e => e.Id).Returns(5);
            Mock<UserEntity> otherEntity1 = new Mock<UserEntity>();
            otherEntity1.SetupGet(e => e.Id).Returns(4);
            testRepo.Add(testEntity.Object);
            testRepo.Add(otherEntity1.Object);
            testRepo.Add(otherEntity2.Object);
            int expectedResult = testRepo.Get(e => (e.Id >= 4)).Count;
            Assert.AreEqual(expectedResult, 2);
        }
    }
}
