using ObligatorioDA2.BusinessLogic;
using System.Collections.Generic;

namespace ObligatorioDA2.Data.Repositories.Contracts
{
    public interface ITeamRepository : IRepository<Team, int>
    {
        Team Get(string sportName, string teamName);

        bool Exists(string sportName, string teamName);

        void Delete(string sportName, string teamName);

        ICollection<Team> GetFollowedTeams(string username);

        ICollection<Team> GetTeams(string sportName);
    }

}