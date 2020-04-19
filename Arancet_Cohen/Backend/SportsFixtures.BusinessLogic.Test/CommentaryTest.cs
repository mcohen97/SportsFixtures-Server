using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ObligatorioDA2.BusinessLogic.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace ObligatorioDA2.BusinessLogic.Test
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class CommentaryTest
    {
        private Commentary commentary;
        private int id;
        private string text;
        Mock<User> user;

        [TestInitialize]
        public void TestInitialize()
        {
            id = 1;
            text = "The match was so boring";
            UserId identity = GetIdentity();
            user = new Mock<User>(identity, true);
            commentary = new Commentary(id, text, user.Object);
        }

        private UserId GetIdentity()
        {
            return new UserId()
            {
                Name = "aName",
                Surname = "aSurname",
                UserName = "aUsername",
                Password = "aPassword",
                Email = "anEmail@aDomain.com"
            };
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(commentary);
        }

        [TestMethod]
        public void GetIdTest()
        {
            Assert.AreEqual(id, commentary.Id);
        }

        [TestMethod]
        public void SetIdTest()
        {
            int newId = 2;
            commentary.Id = newId;
            Assert.AreEqual(newId, commentary.Id);
        }

        [TestMethod]
        public void GetTextTest()
        {
            Assert.AreEqual(text, commentary.Text);
        }

        [TestMethod]
        public void SetTextTest()
        {
            string newText = "The match was incredible";
            commentary.Text = newText;
            Assert.AreEqual(newText, commentary.Text);
        }

        [TestMethod]
        public void EqualsTest()
        {
            Commentary equalCommentary = new Commentary(id, text, user.Object);
            Assert.AreEqual(commentary, equalCommentary);
        }

        [TestMethod]
        public void NotEqualsTest()
        {
            Commentary notEqualCommentary = new Commentary(id + 1, text, user.Object);
            Assert.AreNotEqual(commentary, notEqualCommentary);
        }

        [TestMethod]
        public void EqualsNullTest()
        {
            Assert.IsFalse(commentary.Equals(null));
        }

        [TestMethod]
        public void EqualsDifferenceTypeTest()
        {
            Assert.IsFalse(commentary.Equals(""));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCommentaryDataException))]
        public void SetNullTextTest()
        {
            commentary.Text = null;
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCommentaryDataException))]
        public void SetEmptyTextTest()
        {
            commentary.Text = "";
        }

        [TestMethod]
        public void GetUserTest()
        {
            Assert.AreEqual(commentary.Maker.UserName, "aUsername");
        }
    }
}