﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Moq;
using DataAccess;
using DataRepositoryInterfaces;
using DataRepositories;
using BusinessLogic;
using BusinessLogic.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using RepositoryInterface;
using ObligatorioDA2.DataAccess.Entities;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;

namespace DataRepositoriesTest
{
    [TestClass]
    public class UserRepositoryTest
    {
        IUserRepository usersStorage;
        UserId userId;
        UserFactory factory;
        User user;

        [TestInitialize]
        public void SetUp()
        {
            factory = new UserFactory();
            userId = new UserId()
            {
                Name = "name",
                Surname = "surname",
                UserName = "username",
                Password = "password",
                Email = "mail@domain.com"
            };
            user = factory.CreateAdmin(userId);
            CreateRepository();
            usersStorage.Clear();
        }

        private void CreateRepository() {

            DbContextOptions<DatabaseConnection> options = new DbContextOptionsBuilder<DatabaseConnection>()
                .UseInMemoryDatabase(databaseName: "UserRepository")
                .Options;
            DatabaseConnection context = new DatabaseConnection(options);
            usersStorage = new UserRepository(context);
        }

        [TestMethod]
        public void NoUsersTest()
        {
            bool noUsers = usersStorage.IsEmpty();
            Assert.IsTrue(noUsers);
        }

        [TestMethod]
        public void AddUserTest()
        {
            usersStorage.Add(user);
            int expectedResult = 1;
            int actualResult = usersStorage.GetAll().Count;
            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestMethod]
        [ExpectedException(typeof(UserAlreadyExistsException))]
        public void AddAlreadyExistentUserTest()
        {
            usersStorage.Add(user);
            usersStorage.Add(user);
        }

        [TestMethod]
        public void GetUserTest()
        {
 
            User user = factory.CreateAdmin(userId);
            usersStorage.Add(user);
            User fetched = usersStorage.Get("username");
            Assert.AreEqual("name", fetched.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(UserNotFoundException))]
        public void GetNotExistentUserTest()
        {
            IUserRepository specific = (IUserRepository)usersStorage;
            User fetched = specific.Get("username3");
        }

        [TestMethod]
        public void ExistsUserTest()
        {
            usersStorage.Add(user);
            bool result = usersStorage.Exists(user.UserName);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DoesNotExistTest()
        {
            UserId userId1 = new UserId { Name = "name1", Surname = "surname1", UserName = "username1", Password = "password1", Email = "mail1@domain.com" };
            UserId userId2 = new UserId { Name = "name2", Surname = "surname2", UserName = "username2", Password = "password2", Email = "mail2@domain.com" };
            User user1 = factory.CreateAdmin(userId1);
            User user2 = factory.CreateAdmin(userId2);
            usersStorage.Add(user1);
            bool result = usersStorage.Exists(user2.UserName);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void DeleteTest()
        {
            usersStorage.Add(user);
            usersStorage.Delete(user.UserName);
            Assert.IsTrue(usersStorage.IsEmpty());
        }

        [TestMethod]
        [ExpectedException(typeof(UserNotFoundException))]
        public void DeleteNotExistetTest()
        {
            UserId userId1 = new UserId { Name = "name1", Surname = "surname1", UserName = "username1", Password = "password1", Email = "mail1@domain.com" };
            UserId userId2 = new UserId { Name = "name2", Surname = "surname2", UserName = "username2", Password = "password2", Email = "mail2@domain.com" };
            User user1 = factory.CreateAdmin(userId1);
            User user2 = factory.CreateAdmin(userId2);
            usersStorage.Add(user1);
            usersStorage.Delete(user2.UserName);
        }

        [TestMethod]
        public void ModifyTest()
        {
            usersStorage.Clear();
            UserId userId1 = new UserId { Name = "name1", Surname = "surname1", UserName = "username", Password = "password1", Email = "mail1@domain.com" };
            UserId userId2 = new UserId { Name = "name2", Surname = "surname2", UserName = "username", Password = "password2", Email = "mail2@domain.com" };
            User user1 = factory.CreateAdmin(userId1);
            User user2 = factory.CreateAdmin(userId2);
            usersStorage.Add(user1);
            CreateRepository();
            usersStorage.Modify(user2);
            User toVerify = usersStorage.Get(user2);
            Assert.AreEqual(toVerify.Name, user2.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(UserNotFoundException))]
        public void ModifyUserNotExistsTest()
        {
            usersStorage.Modify(user);
        }


        [TestMethod]
        public void GetTest() {
            User user = factory.CreateAdmin(userId);
            usersStorage.Add(user);
            User fetched = usersStorage.Get(user);
            Assert.AreEqual(user.UserName, fetched.UserName);
        }

        [TestMethod]
        [ExpectedException(typeof(UserNotFoundException))]
        public void GetNotExistentTest() {

            UserId userId1 = new UserId { Name = "name1", Surname = "surname1", UserName = "username2", Password = "password1", Email = "mail1@domain.com" };
            UserId userId2 = new UserId { Name = "name2", Surname = "surname2", UserName = "username1", Password = "password2", Email = "mail2@domain.com" };
            User user1 = factory.CreateAdmin(userId1);
            User user2 = factory.CreateAdmin(userId2);
            usersStorage.Add(user1);
            usersStorage.Get(user2);
        }

        [TestMethod]
        public void ClearUsersTest() {
            usersStorage.Add(user);
            usersStorage.Clear();
            Assert.IsTrue(usersStorage.IsEmpty());
        }

        [TestMethod]
        public void DeleteByUsername() {
            usersStorage.Add(user);
            User fetched = usersStorage.Get(user);
            usersStorage.Delete(fetched.UserName);
            Assert.IsTrue(usersStorage.IsEmpty());
        }

        [TestMethod]
        public void GetUserFollowedTeamsTest() {
            Team toFollow = GetFakeTeam();
            user.AddFavourite(toFollow);
            usersStorage.Add(user);
            CreateRepository();
            User fromDB =usersStorage.Get(user.UserName);
            Assert.AreEqual(fromDB.GetFavouriteTeams().Count, 1);
        }

        private Team GetFakeTeam()
        {
            Sport played = new Sport("Soccer");
            Team fake = new Team(1, "RealMadrid", "aPath", played);
            return fake;
        }
    }
}
