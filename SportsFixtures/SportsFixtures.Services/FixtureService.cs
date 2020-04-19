using System;
using System.Collections.Generic;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.Data.Repositories.Contracts;
using System.Linq;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.Services.Contracts;
using ObligatorioDA2.BusinessLogic.Exceptions;
using System.Reflection;
using System.IO;
using ObligatorioDA2.Services.Mappers;
using ObligatorioDA2.Services.Contracts.Dtos;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;

namespace ObligatorioDA2.Services
{
    public class FixtureService : IFixtureService
    {
        private IInnerEncounterService matchService;
        private ITeamRepository teamStorage;
        private FixtureGenerator fixtureAlgorithm;
        private IAuthenticationService authenticator;
        private ILoggerService logger;
        private EncounterDtoMapper mapper;
        private const string DLL_EXTENSION = "*.dll";


        public FixtureService( ITeamRepository teamRepository,IInnerEncounterService matchAddition,
            IAuthenticationService authService, ILoggerService loggerService)
        {
            matchService = matchAddition;
            teamStorage = teamRepository;
            authenticator = authService;
            logger = loggerService;
            mapper = new EncounterDtoMapper();
        }

        public FixtureGenerator FixtureAlgorithm { get { return fixtureAlgorithm; } private set { SetFixtureAlgorithm(value); } }

        private void SetFixtureAlgorithm(FixtureGenerator algorithm)
        {
            fixtureAlgorithm = algorithm;
        }

        public void SetFixtureAlgorithm(FixtureDto aFixture, string algorithmsPath) {
            ValidateFixture(aFixture);
            FixtureAlgorithm=BuildFixtureAlgorithm(aFixture, algorithmsPath);
        }

        private void ValidateFixture(FixtureDto aFixture)
        {
            if (string.IsNullOrEmpty(aFixture.fixtureName)) {
                logger.Log(LogType.FIXTURE, LogMessage.FIXTURE_WRONG, GetConnectedUserName(), DateTime.Now);
                throw new ServiceException("Fixture name can't be empty", ErrorType.INVALID_DATA);
            }
            if (aFixture.initialDate.Equals(new DateTime())) {
                logger.Log(LogType.FIXTURE, LogMessage.FIXTURE_WRONG, GetConnectedUserName(), DateTime.Now);
                throw new ServiceException("Fixture date can't be empty", ErrorType.INVALID_DATA);
            }
        }

        private void RollBack(ICollection<Encounter> added)
        {
            foreach (Encounter match in added)
            {
                matchService.DeleteEncounter(match.Id);
            }
        }

        public ICollection<EncounterDto> AddFixture(ICollection<string> teamsNames, string sportName)
        {
            authenticator.AuthenticateAdmin();
            ICollection<Team> teamsCollection;
            try
            {
                teamsCollection = teamsNames.Select(name => teamStorage.Get(sportName, name)).ToList();
            }
            catch (TeamNotFoundException e) {
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }
            return AddFixture(teamsCollection);
        }

        public ICollection<EncounterDto> AddFixture(string sportName)
        {
            authenticator.AuthenticateAdmin();
            ICollection<Team> teamsCollection;
            try
            {
                teamsCollection = teamStorage.GetTeams(sportName);
            }
            catch (SportNotFoundException e) {
                logger.Log(LogType.FIXTURE, LogMessage.FIXTURE_SPORT_NOT_FOUND, GetConnectedUserName(), DateTime.Now);
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }
            return AddFixture(teamsCollection);
        }

        private ICollection<EncounterDto> AddFixture(ICollection<Team> teamsCollection)
        {
            ICollection<Encounter> added = new List<Encounter>();
            try
            {
                ICollection<Encounter> generatedMatches = fixtureAlgorithm.GenerateFixture(teamsCollection);
                AddMatches(ref added, generatedMatches);
                logger.Log(LogType.FIXTURE, LogMessage.FIXTURE_OK, GetConnectedUserName(), DateTime.Now);
            }
            catch (TeamAlreadyHasEncounterException e)
            {
                RollBack(added);
                logger.Log(LogType.FIXTURE, LogMessage.FIXTURE_WRONG + " " + e.Message, GetConnectedUserName(), DateTime.Now);
                throw new WrongFixtureException(e.Message);
            }
            catch (InvalidTeamCountException e)
            {
                RollBack(added);
                logger.Log(LogType.FIXTURE, LogMessage.FIXTURE_WRONG + " " + e.Message, GetConnectedUserName(), DateTime.Now);
                throw new WrongFixtureException(e.Message);
            }
            return added.Select(e => mapper.ToDto(e)).ToList();
        }

        private ICollection<Encounter> AddMatches(ref ICollection<Encounter> added, ICollection<Encounter> generated)
        {
            foreach (Encounter match in generated)
            {
                Encounter matchAdded = matchService.AddEncounter(match);
                added.Add(matchAdded);
            }
            return added;
        }

        public ICollection<Type> GetAlgorithms(string dllPath)
        {
            authenticator.AuthenticateAdmin();
            string[] files = GetFilesInPath(dllPath);
            IEnumerable<Type> interestingTypes = new List<Type>();
            foreach (var file in files)
            {
                Type[] types;
                try
                {
                    types = Assembly.LoadFrom(file).GetTypes();
                }
                catch
                {
                    continue;  // Can't load as .NET assembly, so ignore
                }
                interestingTypes= interestingTypes.Concat(
                                    types.Where(t => t.IsClass &&
                                    (t.IsSubclassOf(typeof(FixtureGenerator)) || t == (typeof(FixtureGenerator)))));
            }
            return interestingTypes.ToList();
        }

        private string[] GetFilesInPath(string dllPath)
        {
            try
            {
                return Directory.GetFiles(dllPath, DLL_EXTENSION);
            }
            catch (IOException e) {
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }
        }

        private string GetConnectedUserName() {
            return authenticator.GetConnectedUser().username;
        }

        private FixtureGenerator BuildFixtureAlgorithm(FixtureDto dto, string algorithmsPath)
        {
            int roundLength = dto.roundLength == 0 ? 1 : dto.roundLength;
            int daysBetweenRounds = dto.daysBetweenRounds == 0 ? 7 : dto.daysBetweenRounds;

            try
            {
                Type algortihmType = GetAlgorithmType(algorithmsPath, dto.fixtureName);
                object fromDll = Activator.CreateInstance(algortihmType, new object[] { dto.initialDate, roundLength, daysBetweenRounds });
                FixtureGenerator algorithm = fromDll as FixtureGenerator;
                return algorithm;
            }
            catch (IOException e) {
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }
            catch (MissingMemberException e) {
                throw new ServiceException(e.Message, ErrorType.INVALID_DATA);
            }
        }

        private Type GetAlgorithmType(string algorithmsPath, string fixtureName)
        {
            bool found = false;
            string[] files = Directory.GetFiles(algorithmsPath, DLL_EXTENSION);
            var x = Directory.GetCurrentDirectory();
            Type first2comply = null;

            for (int i = 0; i < files.Length && !found; i++)
            {
                Assembly actual = Assembly.LoadFrom(files[i]);
                first2comply = actual.GetType(fixtureName);
                if (first2comply != null && (first2comply.IsSubclassOf(typeof(FixtureGenerator)) || first2comply == typeof(FixtureGenerator)))
                {
                    found = true;
                }
            }

            if (first2comply == null)
            {
                throw new ServiceException("Fixture not found", ErrorType.ENTITY_NOT_FOUND);
            }
            return first2comply;
        }
    }
}