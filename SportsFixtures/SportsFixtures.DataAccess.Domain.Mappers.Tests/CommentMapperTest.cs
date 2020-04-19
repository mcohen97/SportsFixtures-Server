using ObligatorioDA2.BusinessLogic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ObligatorioDA2.Data.Entities;
using System.Diagnostics.CodeAnalysis;

namespace ObligatorioDA2.Data.DomainMappers.Mappers.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class CommentMapperTest
    {
        private CommentMapper testMapper;
        private Commentary comment;
        private CommentEntity commentEntity;
        [TestInitialize]
        public void StartUp()
        {
            testMapper = new CommentMapper();
            UserId identity = new UserId
            {
                Name = "aName",
                Surname = "aSurname",
                UserName = "aUsername",
                Password = "aPassword",
                Email = "anEmail@aDomain.com"
            };
            Mock<User> stub = new Mock<User>(identity, true);
            comment = new Commentary("this is a comment", stub.Object);
            commentEntity = new CommentEntity()
            {
                Id = 3,
                Text = "another comment",
                Maker = new UserEntity()
                {
                    Name = "aName",
                    Surname = "aSurname",
                    UserName = "aUsername",
                    Password = "aPassword",
                    Email = "anEmail@aDomain.com"
                }
            };
        }

        [TestMethod]
        public void CommentToEntityTextTest()
        {
            CommentEntity entity = testMapper.ToEntity(comment);
            Assert.AreEqual(entity.Text, comment.Text);
        }

        [TestMethod]
        public void CommentToEntityUserTest()
        {
            CommentEntity entity = testMapper.ToEntity(comment);
            Assert.AreEqual(entity.Maker.UserName, comment.Maker.UserName);
        }

        [TestMethod]
        public void CommentToEntityIdTest()
        {
            CommentEntity entity = testMapper.ToEntity(comment);
            Assert.AreEqual(entity.Id, comment.Id);
        }

        [TestMethod]
        public void EntityToCommentTextTest()
        {
            Commentary converted = testMapper.ToComment(commentEntity);
            Assert.AreEqual(converted.Text, commentEntity.Text);
        }

        [TestMethod]
        public void EntityToCommentUserTest()
        {
            Commentary converted = testMapper.ToComment(commentEntity);
            Assert.AreEqual(converted.Maker.UserName, commentEntity.Maker.UserName);

        }

        [TestMethod]
        public void EntityToCommentIdTest()
        {
            Commentary converted = testMapper.ToComment(commentEntity);
            Assert.AreEqual(converted.Id, commentEntity.Id);
        }
    }
}
