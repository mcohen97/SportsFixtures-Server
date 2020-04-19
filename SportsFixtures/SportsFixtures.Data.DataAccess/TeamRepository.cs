using System;
using System.Collections.Generic;
using System.Linq;
using ObligatorioDA2.Data.DataAccess;
using ObligatorioDA2.Data.Repositories.Contracts;
using ObligatorioDA2.BusinessLogic;
using Microsoft.EntityFrameworkCore;
using ObligatorioDA2.Data.Entities;
using ObligatorioDA2.Data.DomainMappers;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using System.Data.Common;

namespace ObligatorioDA2.Data.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private DatabaseConnection context;
        private readonly TeamMapper mapper;

        public TeamRepository(DatabaseConnection context)
        {
            this.context = context;
            this.mapper = new TeamMapper();
        }

        public void Clear()
        {
            try
            {
                TryClear();
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }
        }

        private void TryClear()
        {
            foreach (TeamEntity team in context.Teams)
            {
                context.Teams.Remove(team);
            }
            context.SaveChanges();
        }

        public Team Get(string sportName, string teamName)
        {
            try
            {
                return TryGet(sportName, teamName);
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }

        }

        private Team TryGet(string sportName, string teamName)
        {
            Team fetched;
            if (Exists(sportName, teamName))
            {
                fetched = GetExistent(sportName, teamName);
            }
            else
            {
                throw new TeamNotFoundException();
            }

            return fetched;
        }

        private Team GetExistent(string sportName, string teamName)
        {
            TeamEntity entity = context.Teams
                .FirstOrDefault(e => e.Name.Equals(teamName) && e.Sport.Name.Equals(sportName));
            Team converted = mapper.ToTeam(entity);
            return converted;
        }

        public void Delete(string sportName, string teamName)
        {
            try
            {
                TryDelete(sportName, teamName);
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }
        }

        private void TryDelete(string sportName, string teamName)
        {
            if (Exists(sportName, teamName))
            {
                DeleteValid(sportName, teamName);
            }
            else
            {
                throw new TeamNotFoundException();
            }
        }

        private void DeleteValid(string sportName, string teamName)
        {
            TeamEntity toDelete = context.Teams
                .First(t => t.Sport.Name.Equals(sportName) && t.Name.Equals(teamName));
            context.Teams.Remove(toDelete);
            DeleteMatches(toDelete);
            context.SaveChanges();
        }

        private void DeleteMatches(TeamEntity toDelete)
        {
            IQueryable<EncounterTeam> matchTeams = context.EncounterTeams
                .Include(mt=> mt.Encounter).ThenInclude(m=>m.Commentaries)
                .Where(mt=> mt.TeamNumber == toDelete.TeamNumber);
            context.EncounterTeams.RemoveRange(matchTeams);
            IQueryable<EncounterEntity> matches = matchTeams.Select(mt => mt.Encounter);
            context.Encounters.RemoveRange(matches);
            foreach (EncounterEntity match in matches)
            {
                context.Comments.RemoveRange(match.Commentaries);
            }
        }

        public Team Add(Team aTeam)
        {
            try
            {
                return TryAdd(aTeam);
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }
        }

        private Team TryAdd(Team aTeam)
        {
            TeamEntity entity = mapper.ToEntity(aTeam);
            Team teamAdded;
            if (!Exists(entity.Sport.Name, entity.Name))
            {
                teamAdded = AddNew(aTeam);
            }
            else
            {
                throw new TeamAlreadyExistsException();
            }
            return teamAdded;
        }

        private Team AddNew(Team aTeam)
        {
            TeamEntity toStore = mapper.ToEntity(aTeam);
            context.Teams.Add(toStore);
            if (context.Sports.Contains(toStore.Sport))
            {
                context.Entry(toStore.Sport).State = EntityState.Unchanged;
            }
            //We also need to ask if it is an Sql database, so that we can execute the sql scripts.
            if (aTeam.Id > 0 && context.Database.IsSqlServer())
            {
                SaveWithIdentityInsert();
            }
            else
            {
                context.SaveChanges();
            }

            return mapper.ToTeam(toStore);

        }

        private void SaveWithIdentityInsert()
        {
            context.Database.OpenConnection();
            try
            {
                context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.Teams ON");
                context.SaveChanges();
                context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.Teams OFF");
            }
            finally
            {
                context.Database.CloseConnection();
            }
        }

        public void Modify(Team aTeam)
        {
            TeamEntity entity = mapper.ToEntity(aTeam);
            try
            {
                TryModify(entity);
            }
            catch (DbUpdateException)
            {
                context.Entry(entity).State = EntityState.Detached;
                throw new TeamNotFoundException();
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }
        }

        private void TryModify(TeamEntity entity)
        {
            context.Teams.Update(entity);
            context.SaveChanges();
        }

        public bool IsEmpty()
        {
            try
            {
                return !context.Teams.Any();
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }
        }

        public bool Exists(string sportName, string teamName)
        {
            bool exists;
            try
            {
                exists = context.Teams.Any(t => t.Sport.Name.Equals(sportName) && t.Name.Equals(teamName));
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }
            return exists;
        }

        public ICollection<Team> GetAll()
        {
            ICollection<Team> all;
            try
            {
                all = TryGetAll();
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }
            return all;
        }

        private ICollection<Team> TryGetAll()
        {
            IQueryable<TeamEntity> teams = context.Teams.Include(t=>t.Sport);
            ICollection<Team> result = teams.Select(t => mapper.ToTeam(t)).ToList();
            return result;
        }

        public ICollection<Team> GetFollowedTeams(string username)
        {
            ICollection<Team> followed;
            try
            {
                followed = TryGetFollowedTeams(username);
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }
            return followed;
        }

        private ICollection<Team> TryGetFollowedTeams(string username)
        {
            IQueryable<UserTeam> relationships = context.UserTeams
                .Where(ut => ut.UserEntityUserName.Equals(username));
            IQueryable<TeamEntity> teams = relationships.Select(ut => ut.Team);
            return teams.Select(t => mapper.ToTeam(t)).ToList();
        }

        public ICollection<Team> GetTeams(string sportName)
        {
            ICollection<Team> teams;
            try
            {
                teams = TryGetTeams(sportName);
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }
            return teams;
        }

        private ICollection<Team> TryGetTeams(string sportName)
        {
            if (!context.Sports.Any(s => s.Name.Equals(sportName)))
            {
                throw new SportNotFoundException();
            }
            return GetAll().Where(t => t.Sport.Name == sportName).ToList();
        }

        public bool Exists(int id)
        {
            bool exists;
            try
            {
                exists = AskIfExists(id);
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }
            return exists;
        }

        private bool AskIfExists(int id)
        {
            return context.Teams.Any(t => t.TeamNumber == id);
        }

        public void Delete(int id)
        {
            try
            {
                TryDelete(id);
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }

        }

        private void TryDelete(int id)
        {
            if (!Exists(id))
                throw new TeamNotFoundException();

            TeamEntity toDelete = context.Teams
                .First(t => t.TeamNumber == id);
            context.Teams.Remove(toDelete);
            DeleteMatches(toDelete);
            DeleteUserTeams(toDelete);
            context.SaveChanges();
        }

        public Team Get(int id)
        {
            Team fetched;
            try
            {
                fetched = TryGet(id);
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }
            return fetched;
        }

        private void DeleteUserTeams(TeamEntity toDelete)
        {
            IQueryable<UserTeam> userTeams = context.UserTeams.Where(t => t.Team.TeamNumber==toDelete.TeamNumber);
            context.UserTeams.RemoveRange(userTeams);
        }


        private Team TryGet(int id)
        {
            if (!Exists(id))
                throw new TeamNotFoundException();

            TeamEntity entity = context.Teams.AsNoTracking()
                .Include(t=> t.Sport)
                .FirstOrDefault(e => e.TeamNumber == id);
            Team converted = mapper.ToTeam(entity);
            return converted;
        }
    }
}