using System;
using System.Collections.Generic;
using ObligatorioDA2.Services.Contracts.Dtos;

namespace ObligatorioDA2.Services.Contracts
{
    public interface IEncounterService
    {
        EncounterDto AddEncounter(EncounterDto aMatch);

        EncounterDto AddEncounter(ICollection<int> teamsIds, string sportName, DateTime date);

        EncounterDto AddEncounter(int assignedId, ICollection<int> teamsIds, string sportName, DateTime date);

        ICollection<EncounterDto> GetAllEncounter();

        EncounterDto GetEncounter(int anId);

        void DeleteEncounter(int anId);

        EncounterDto ModifyEncounter(EncounterDto aMatch);

        EncounterDto ModifyEncounter(int idMatch, ICollection<int> teamsIds, DateTime date, string sportName);

        ICollection<EncounterDto> GetAllEncounterDtos(string sportName);

        ICollection<EncounterDto> GetAllEncounterDtos(int teamId);

        bool Exists(int id);

        CommentaryDto CommentOnEncounter(int matchId, string userName, string text);

        ICollection<CommentaryDto> GetEncounterCommentaries(int matchId);
        ICollection<CommentaryDto> GetAllCommentaries();
        CommentaryDto GetComment(int id);
        void SetResult(int id, ResultDto resultDto);
    }
}
