using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObligatorioDA2.BusinessLogic.Exceptions;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ObligatorioDA2.BusinessLogic.Test
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ResultTest
    {
        private Result testResult;
        private Team testTeam;

        [TestInitialize]
        public void StartUp() {
            testResult = new Result();
            Sport sportDummy = new Sport("aSport", true);
            testTeam = new Team("aTeam", "aPhoto", sportDummy);
        }

        [TestMethod]
        public void AddPositionTest() {
            testResult.Add(testTeam,1);
            Assert.IsTrue(testResult.GetPositions().Any());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidResultDataException))]
        public void AddInvalidPositionTest() {
            testResult.Add(testTeam, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidResultDataException))]
        public void AddRepeatedParticipantTest()
        {
            testResult.Add(testTeam, 1);
            testResult.Add(testTeam, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidResultDataException))]
        public void AddNullParticipantTest(){
            testResult.Add(null, 1);
        }

    }
}
