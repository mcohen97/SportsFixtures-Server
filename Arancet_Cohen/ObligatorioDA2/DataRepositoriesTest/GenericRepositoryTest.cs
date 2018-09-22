﻿using System;
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
    class GenericRepositoryTest
    {
        IEntityRepository<BaseEntity> testRepo;
        Mock<BaseEntity> testEntity;
        [TestInitialize]
        public void SetUp()
        {
            InitializeRepository();
            testEntity = new Mock<BaseEntity>();
            testEntity.SetupGet(u => u.Id).Returns(3);
        }

        private void InitializeRepository()
        {
            DbContextOptions<DatabaseConnection> options = new DbContextOptionsBuilder<DatabaseConnection>()
                .UseInMemoryDatabase(databaseName: "UserRepository")
                .Options;
            DatabaseConnection context = new DatabaseConnection(options);
            testRepo = new GenericRepository<BaseEntity>(context);
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
            Mock<BaseEntity> otherEntity = new Mock<BaseEntity>();
            otherEntity.SetupGet(e => e.Id).Returns(4);
            testRepo.Add(testEntity.Object);
            testRepo.Add(otherEntity.Object);
            bool any = testRepo.Any(u => u.Id == 4);
            Assert.IsTrue(any);
        }

        [TestMethod]
        public void AnyPredicateFalseTest()
        {
            Mock<BaseEntity> otherEntity = new Mock<BaseEntity>();
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
    }
}
