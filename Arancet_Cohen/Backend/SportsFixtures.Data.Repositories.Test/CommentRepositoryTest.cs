using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using RepositoryInterface;
using DataRepositories;
using Moq;
using BusinessLogic;
using DataRepositoryInterfaces;
using ObligatorioDA2.DataAccess.Entities;

namespace DataRepositoriesTest
{
    [TestClass]
    public class CommentRepositoryTest
    {
        ICommentRepository commentsRepo;
        Commentary comment;
        //Mock<BusinessLogic.Match> match;
        
        [TestInitialize]
        public void SetUp() {
            DbContextOptions<DatabaseConnection> options = new DbContextOptionsBuilder<DatabaseConnection>()
                .UseInMemoryDatabase(databaseName: "CommentRepository")
                .Options;
            DatabaseConnection db = new DatabaseConnection(options);
            IEntityRepository<CommentEntity> entityRepo = new GenericRepository<CommentEntity>(db);
            commentsRepo = new CommentRepository(entityRepo);
            User user = new Mock<User>("aName", "asurname", "aUsername", "aPassword", "anEmail@aDomain.com").Object;
            comment = new Commentary(4,"this is a comment",user);
            commentsRepo.Clear();
            //Mock<Team> home = new Mock<Team>(3,"River Plate", "aPath");
            //Mock<Team> away = new Mock<Team>(3, "Boca Juniors", "aPath");
            //match = new Mock<BusinessLogic.Match>(home.Object, away.Object);
        }

        [TestMethod]
        public void EmptyRepositoryTest() {
            Assert.IsTrue(commentsRepo.IsEmpty());
        }

        [TestMethod]
        public void AddCommentTest() {
            Mock<Team> home = new Mock<Team>();
            Mock<Team> away = new Mock<Team>();
            Mock<BusinessLogic.Match> match = new Mock<BusinessLogic.Match>(home.Object, away.Object, DateTime.Now);
            Assert.IsFalse(commentsRepo.IsEmpty());
        }

    }
}
