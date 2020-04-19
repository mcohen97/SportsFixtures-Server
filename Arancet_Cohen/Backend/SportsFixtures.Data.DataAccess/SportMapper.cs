using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.Data.Entities;

namespace ObligatorioDA2.Data.DomainMappers
{
    public class SportMapper
    {

        public SportEntity ToEntity(Sport aSport)
        {
            SportEntity converted = new SportEntity()
            {
                Name = aSport.Name,
                IsTwoTeams =aSport.IsTwoTeams
            };
            return converted;
        }

        public Sport ToSport(SportEntity entity)
        {
            Sport conversion = new Sport(entity.Name, entity.IsTwoTeams);
            return conversion;
        }
    }
}
