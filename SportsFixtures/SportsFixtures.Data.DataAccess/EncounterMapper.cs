using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.Data.Entities;

namespace ObligatorioDA2.Data.DomainMappers
{
    public class EncounterMapper
    {
        private TeamMapper teamConverter;
        private CommentMapper commentConverter;
        private SportMapper sportConverter;
        private EncounterFactory factory;

        public EncounterMapper()
        {
            teamConverter = new TeamMapper();
            commentConverter = new CommentMapper();
            sportConverter = new SportMapper();
            factory = new EncounterFactory();
        }
        public EncounterEntity ToEntity(Encounter anEncounter)
        {
            SportEntity sportEntity = sportConverter.ToEntity(anEncounter.Sport);
            EncounterEntity conversion = new EncounterEntity()
            {
                Id = anEncounter.Id,
                Date = anEncounter.Date,
                Commentaries = TransformCommentaries(anEncounter.GetAllCommentaries()),
                SportEntity = sportEntity,
                HasResult = anEncounter.HasResult()
            };
            return conversion;
        }


        private ICollection<CommentEntity> TransformCommentaries(ICollection<Commentary> commentaries)
        {
            return commentaries.Select(c => commentConverter.ToEntity(c)).ToList();
        }

        public Encounter ToEncounter(EncounterEntity anEncounter, ICollection<EncounterTeam> playingTeams)
        {
            ICollection<Commentary> comments = anEncounter.Commentaries.Select(ce => commentConverter.ToComment(ce)).ToList();
            ICollection<Team> teams = playingTeams.Select(tm => teamConverter.ToTeam(tm.Team)).ToList();
            DateTime date = anEncounter.Date;
            Sport sport = sportConverter.ToSport(anEncounter.SportEntity);
            Encounter created = factory.CreateEncounter(anEncounter.Id,teams,date, sport, comments);
            if (anEncounter.HasResult) {
                Result matchResult = ToResults(playingTeams);
                created.Result=matchResult;
            }
            return created;
        }

        private Result ToResults(ICollection<EncounterTeam> playingTeams)
        {
            Result matchResult = new Result();
            foreach (EncounterTeam mt in playingTeams) {
                Team converted = teamConverter.ToTeam(mt.Team);
                matchResult.Add(converted, mt.Position);
            }
            return matchResult;
        }

        public ICollection<EncounterTeam> ConvertParticipants(Encounter anEncounter)
        {
            EncounterEntity matchEntity = ToEntity(anEncounter);
            ICollection<EncounterTeam> conversions = new List<EncounterTeam>();
            foreach (Team participant in anEncounter.GetParticipants()) {
                TeamEntity team = teamConverter.ToEntity(participant);
                EncounterTeam participantConversion = new EncounterTeam()
                {
                    Encounter = matchEntity,
                    EncounterId = matchEntity.Id,
                    Team = team,
                    TeamNumber = team.TeamNumber
                };
                conversions.Add(participantConversion);
            }
            if (anEncounter.HasResult()) {
                ResultsToEntity(conversions, anEncounter.Result);
            }
            return conversions;
        }

        private void ResultsToEntity(ICollection<EncounterTeam> conversions, Result result)
        {
            foreach (Tuple<Team, int> standing in result.GetPositions()) {
                conversions.First(mt => mt.TeamNumber == standing.Item1.Id).Position = standing.Item2;
            }
        }
    }
}

