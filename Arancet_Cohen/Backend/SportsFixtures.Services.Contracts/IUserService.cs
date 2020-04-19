using System.Collections.Generic;
using ObligatorioDA2.Services.Contracts.Dtos;

namespace ObligatorioDA2.Services.Contracts
{
    public interface IUserService
    {
        UserDto GetUser(string username);
        UserDto AddUser(UserDto userData);
        UserDto ModifyUser(UserDto userData);
        void DeleteUser(string userName);
        void FollowTeam(string userName, int idTeam);
        ICollection<TeamDto> GetUserTeams(string userName);
        void UnFollowTeam(string userName, int id);
        ICollection<UserDto> GetAllUsers();
    }
}
