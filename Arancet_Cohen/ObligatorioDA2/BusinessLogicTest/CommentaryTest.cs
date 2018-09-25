using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BusinessLogic;
using System.Collections.Generic;
using Moq;
using BusinessLogic.Exceptions;

namespace BusinessLogicTest
{
    [TestClass]
    public class CommentaryTest
    {
        private Commentary commentary;
        private int id;
        private string text;
        Mock<User> user;

        [TestInitialize]
        public void TestInitialize(){
            id = 1;
            text = "The match was so boring";
            user = new Mock<User>("aName","aUsername","aUsername","aPassword"
                ,"anEmail@aDomain.com",true);
            commentary = new Commentary(id, text,user.Object);
        }

        [TestMethod]
        public void ConstructorTest(){
            Assert.IsNotNull(commentary);
        }

         [TestMethod]
        public void GetIdTest(){
            Assert.AreEqual(id, commentary.Id);
        }

        [TestMethod]
        public void SetIdTest(){
            int newId = 2;
            commentary.Id = newId;
            Assert.AreEqual(newId, commentary.Id);
        }

        [TestMethod]
        public void GetTextTest(){
            Assert.AreEqual(text, commentary.Text);
        }

        [TestMethod]
        public void SetTextTest(){
            string newText = "The match was incredible";
            commentary.Text = newText;
            Assert.AreEqual(newText, commentary.Text);
        }

        [TestMethod]
        public void EqualsTest(){
            Commentary equalCommentary = new Commentary(id, text,user.Object);
            Assert.AreEqual(commentary, equalCommentary);
        }

        [TestMethod]
        public void NotEqualsTest(){
            Commentary notEqualCommentary = new Commentary(id+1, text, user.Object);
            Assert.AreNotEqual(commentary, notEqualCommentary);
        }

        [TestMethod]
        public void EqualsNullTest(){
            Assert.AreNotEqual(commentary, null);
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
        public void GetUserTest() {
            Assert.AreEqual(commentary.Maker.UserName, "aUsername");
        }
    }
}