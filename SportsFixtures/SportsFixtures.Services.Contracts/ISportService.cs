using System.Collections.Generic;
using ObligatorioDA2.Services.Contracts.Dtos;

namespace ObligatorioDA2.Services.Contracts
{
    public interface ISportService
    {
        ICollection<SportDto> GetAllSports();
        SportDto GetSport(string name);
        SportDto AddSport(SportDto testDto);
        void DeleteSport(string name);
    }
}
