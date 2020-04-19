using ObligatorioDA2.Services.Contracts.Dtos;
using System;
using System.Collections.Generic;

namespace ObligatorioDA2.Services.Contracts
{
    public interface ISportTableService
    {
        ICollection<Tuple<TeamDto,int>> GetScoreTable(string sportName);
    }
}
