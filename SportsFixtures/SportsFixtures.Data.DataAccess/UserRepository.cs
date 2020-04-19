using System.Collections.Generic;
using System.Linq;
using ObligatorioDA2.Data.Repositories.Contracts;
using ObligatorioDA2.Data.DataAccess;
using ObligatorioDA2.BusinessLogic;
using Microsoft.EntityFrameworkCore;
using ObligatorioDA2.Data.Entities;
using ObligatorioDA2.Data.DomainMappers;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using System.Data.Common;

namespace ObligatorioDA2.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseConnection context;
        private readonly UserMapper userMapper;
        private readonly TeamMapper teamMapper;

        public UserRepository(DatabaseConnection aContext)
        {
            context = aContext;
            userMapper = new UserMapper();
            teamMapper = new TeamMapper();
        }

        public User Add(User aUser)
        {
            User added;
            try
            {
                added = TryAdd(aUser);
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }
            return added;
        }

        private User TryAdd(User aUser)
        {
            User added;
            if (!AnyWithThisUserName(aUser.UserName))
            {
                UserEntity entity = userMapper.ToEntity(aUser);
                ICollection<UserTeam> follower_teams = userMapper.GetUserTeams(aUser);
                context.Users.Add(entity);
                context.UserTeams.AddRange(follower_teams);
                context.SaveChanges();
                context.Entry(entity).State = EntityState.Detached;
                added = aUser;
            }
            else
            {
                throw new UserAlreadyExistsException();
            }
            return added;
        }

        public User Get(string aUserName)
        {
            User stored;
            try
            {
                stored = TryGet(aUserName);

            }
            catch (DbException e)
            {
                throw new DataInaccessibleException();
            }
            return stored;
        }

        private User TryGet(string aUserName)
        {
            User toReturn;
            if (AnyWithThisUserName(aUserName))
            {
                UserEntity retrieved = GetEntityByUsername(aUserName);
                ICollection<TeamEntity> teams = GetFollowedTeams(aUserName);
                toReturn = userMapper.ToUser(retrieved, teams);
                context.Entry(retrieved).State = EntityState.Detached;
            }
            else
            {
                throw new UserNotFoundException();
            }
            return toReturn;
        }

        private ICollection<TeamEntity> GetFollowedTeams(string aUserName)
        {
            ICollection<UserTeam> result= context.UserTeams
                     .Where(ut => ut.UserEntityUserName.Equals(aUserName))
                     .Include(ut => ut.Team).ThenInclude(t => t.Sport)
                     .ToList();
            return result.Select(ut => ut.Team).ToList();
        }

        public void Delete(string username)
        {
            try
            {
                TryDelete(username);
            }
            catch (DbException e)
            {
                throw new DataInaccessibleException();
            }
        }

        private void TryDelete(string username)
        {
            if (AnyWithThisUserName(username))
            {
                UserEntity toDelete = GetEntityByUsername(username);
                context.Users.Remove(toDelete);
                DeleteComments(username);
                context.SaveChanges();
            }
            else
            {
                throw new UserNotFoundException();
            }
        }

        private void DeleteComments(string username)
        {
            IQueryable<CommentEntity> commentsToDelete = context.Comments.Where(c => c.Maker.UserName.Equals(username));
            context.Comments.RemoveRange(commentsToDelete);
        }

        private UserEntity GetEntityByUsername(string aUserName)
        {
            return context.Users.First(u => u.UserName.Equals(aUserName));
        }

        public ICollection<User> GetAll()
        {
            ICollection<User> allOfThem;
            try
            {
                allOfThem = TryGetAll();
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }
            return allOfThem;
        }

        private ICollection<User> TryGetAll()
        {
            IQueryable<UserEntity> query = context.Users;
            ICollection<User> users = new List<User>();

            foreach (UserEntity user in query)
            {
                IQueryable<TeamEntity> followed = context.UserTeams
                    .Where(ue => ue.UserEntityUserName.Equals(user.UserName))
                    .Select(ue => ue.Team)
                    .Include(te=>te.Sport);
                User built = userMapper.ToUser(user, followed.ToList());
                users.Add(built);
            };
            return users;
        }

        public bool IsEmpty()
        {
            bool empty;
            try
            {
                empty = !context.Users.Any();
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }
            return empty;
        }

        public bool Exists(string username)
        {
            bool doesExist;
            try
            {
                doesExist = AskIfExists(username);
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }
            return doesExist;
        }

        private bool AskIfExists(string username)
        {
            return context.Users.Any(u => u.UserName.Equals(username));

        }

        public void Modify(User aUser)
        {
            try
            {
                TryModify(aUser);
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }
        }

        private void TryModify(User aUser)
        {
            if (AnyWithThisUserName(aUser.UserName))
            {
                UserEntity entity = userMapper.ToEntity(aUser);
                ICollection<UserTeam> favourites = userMapper.GetUserTeams(aUser);
                RemoveMissing(aUser.UserName, favourites);
                AddNewFavourites(favourites);
                context.Update(entity);
                context.SaveChanges();
            }
            else
            {
                throw new UserNotFoundException();
            }
        }

        private void RemoveMissing(string userName, ICollection<UserTeam> favourites)
        {
            IQueryable<UserTeam> missing = context.UserTeams
                .Where(ut => ut.UserEntityUserName.Equals(userName) && !favourites.Any(f => f.TeamEntitySportEntityName.Equals(ut.TeamEntitySportEntityName)
                                                 && f.TeamEntityName.Equals(ut.TeamEntityName)));
            context.UserTeams.RemoveRange(missing);
        }

        private void AddNewFavourites(ICollection<UserTeam> favourites)
        {
            IEnumerable<UserTeam> newFavourites = favourites
                                                            .Where(f => !(context.UserTeams
                                                            .Any(ut => f.TeamEntitySportEntityName.Equals(ut.TeamEntitySportEntityName)
                                                            && f.TeamEntityName.Equals(ut.TeamEntityName)
                                                            && f.UserEntityUserName.Equals(ut.UserEntityUserName))));
            foreach (UserTeam ut in newFavourites)
            {
                context.Entry(ut).State = EntityState.Added;
            }
        }


        private bool AnyWithThisUserName(string userName)
        {
            return context.Users.Any(u => u.UserName.Equals(userName));
        }

        public User Get(User asked)
        {
            return Get(asked.UserName);
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
            IQueryable<UserEntity> allOfThem = context.Users;
            foreach (UserEntity existent in allOfThem)
            {
                context.Users.Remove(existent);
            }
            context.SaveChanges();
        }
    }
}

