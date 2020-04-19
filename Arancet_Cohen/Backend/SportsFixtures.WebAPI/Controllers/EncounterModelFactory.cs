using ObligatorioDA2.Services.Contracts.Dtos;
using ObligatorioDA2.WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ObligatorioDA2.WebAPI.Controllers
{
    public class EncounterModelFactory
    {
        public EncounterModelOut CreateModelOut(EncounterDto encounter) {
            EncounterModelOut conversion;
            if (encounter.isSportTwoTeams)
            {
                conversion = CreateMatchModelOut(encounter);
            }
            else {
                conversion = CreateCompetitionModelOut(encounter);
            }
            return conversion;
        }

        private MatchModelOut CreateMatchModelOut(EncounterDto encounter)
        {
            MatchModelOut converted = new MatchModelOut()
            {
                Id = encounter.id,
                SportName = encounter.sportName,
                TeamIds = encounter.teamsIds,
                Date = encounter.date,
                CommentsIds = encounter.commentsIds,
                HasResult = encounter.hasResult
            };
            if (encounter.hasResult) {
                List<Tuple<int, int>> standings = encounter.result.teams_positions.ToList();
                converted.HasWinner = standings[0].Item2 != standings[1].Item2;
                if (converted.HasWinner)
                {
                    converted.WinnerId = standings.First(t => t.Item2 == 1).Item1;
                }
            }
            return converted;
        }

        private CompetitionModelOut CreateCompetitionModelOut(EncounterDto encounter)
        {
            CompetitionModelOut converted = new CompetitionModelOut()
            {
                Id = encounter.id,
                TeamIds = encounter.teamsIds,
                Date = encounter.date,
                SportName = encounter.sportName,
                CommentsIds = encounter.commentsIds,
                HasResult = encounter.hasResult
            };
            if (encounter.hasResult) {
                converted.Team_Position = encounter.result.teams_positions
                    .Select(tp => new StandingModelOut() {TeamId =tp.Item1, Points= tp.Item2 })
                    .ToList();
            }
            return converted;
        }


    }
}
