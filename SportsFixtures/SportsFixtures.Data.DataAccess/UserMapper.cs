using System;
using System.Collections.Generic;
using System.Text;
using ObligatorioDA2.Data.Entities;
using ObligatorioDA2.BusinessLogic;
using System.Linq;

namespace ObligatorioDA2.Data.DomainMappers
{
    public class UserMapper
    {
        private UserFactory factory;
        private TeamMapper teamConverter;

        public UserMapper()
        {
            factory = new UserFactory();
            teamConverter = new TeamMapper();
        }

        public UserEntity ToEntity(User toConvert)
        {
            UserEntity converted = new UserEntity
            {
                Name = toConvert.Name,
                Surname = toConvert.Surname,
                UserName = toConvert.UserName,
                Password = toConvert.Password,
                Email = toConvert.Email,
                IsAdmin = toConvert.IsAdmin
            };
            return converted;
        }

        public ICollection<UserTeam> GetUserTeams(User aUser)
        {
            UserEntity userEntity = ToEntity(aUser);
            ICollection<UserTeam> relationships = aUser.GetFavouriteTeams()
                .Select(t => BuildRelationship(userEntity, t))
                .ToList();
            return relationships;
        }

        private UserTeam BuildRelationship(UserEntity entity, Team aTeam)
        {
            TeamEntity teamEntity = teamConverter.ToEntity(aTeam);
            UserTeam relationship = new UserTeam
            {
                Team = teamEntity,
                Follower = entity,
                TeamEntitySportEntityName = teamEntity.SportEntityName,
                TeamEntityName = teamEntity.Name,
                UserEntityUserName = entity.UserName
            };
            return relationship;
        }

        public User ToUser(UserEntity toConvert, ICollection<TeamEntity> teamEntities)
        {
            UserId identity = new UserId()
            {
                Name = toConvert.Name,
                Surname = toConvert.Surname,
                UserName = toConvert.UserName,
                Password = toConvert.Password,
                Email = toConvert.Email
            };
            ICollection<Team> teams = teamEntities.Select(t => teamConverter.ToTeam(t)).ToList();
            User converted;
            converted = toConvert.IsAdmin ? factory.CreateAdmin(identity, teams) : factory.CreateFollower(identity, teams);
            return converted;
        }
    }
}
