using ObligatorioDA2.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Text;

namespace ObligatorioDA2.Services
{
    public interface IInnerEncounterService
    {
        Encounter AddEncounter(Encounter anEncounter);
        ICollection<Encounter> GetAllEncounters(string sportName);
        void DeleteEncounter(int anId);
    }
}
