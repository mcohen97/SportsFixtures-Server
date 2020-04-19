using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.Data.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using ObligatorioDA2.Data.DomainMappers;
using ObligatorioDA2.Data.Entities;
using System.Collections.Generic;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.Data.Repositories.Contracts;
using System.Data.Common;
using System;

namespace ObligatorioDA2.Data.Repositories
{
    public class EncounterRepository : IEncounterRepository
    {
        private DatabaseConnection context;
        private EncounterMapper matchConverter;
        private TeamMapper teamConverter;
        private CommentMapper commentConverter;
        private EncounterFactory factory;
        public EncounterRepository(DatabaseConnection aContext)
        {
            context = aContext;
            matchConverter = new EncounterMapper();
            commentConverter = new CommentMapper();
            teamConverter = new TeamMapper();
            factory = new EncounterFactory();
        }


        public Encounter Add(Encounter aMatch)
        {
            Encounter added;
            if (!Exists(aMatch.Id))
            {
                added = AddNew(aMatch);
            }
            else
            {
                throw new EncounterAlreadyExistsException();
            }
            return added;
        }

        private Encounter AddNew(Encounter aMatch)
        {
            EncounterEntity toAdd = matchConverter.ToEntity(aMatch);
            context.Entry(toAdd).State = EntityState.Added;
            AddComments(toAdd, aMatch.GetAllCommentaries());

            //We also need to ask if it is an Sql database, so that we can execute the sql scripts.
            if (aMatch.Id > 0 && context.Database.IsSqlServer())
            {
                SaveWithIdentityInsert();
            }
            else
            {
                context.SaveChanges();
            }
            Encounter added = factory.CreateEncounter(toAdd.Id, aMatch.GetParticipants(), aMatch.Date, aMatch.Sport);
            ICollection<EncounterTeam> playingTeams = matchConverter.ConvertParticipants(added);
            foreach (EncounterTeam team in playingTeams)
            {
                context.Entry(team).State = EntityState.Added;
            }
            context.SaveChanges();
            context.Entry(toAdd).State = EntityState.Detached;
            return added;
        }

        private void AddComments(EncounterEntity entity, ICollection<Commentary> commentaries)
        {
            IEnumerable<CommentEntity> commentEntities = commentaries.Select(c => commentConverter.ToEntity(c));
            foreach (CommentEntity ce in commentEntities) {
                entity.Commentaries.Add(ce);
                context.Entry(ce.Maker).State = EntityState.Unchanged;
            }
        }

        private void SaveWithIdentityInsert()
        {
            context.Database.OpenConnection();
            try
            {
                context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.Matches ON");
                context.SaveChanges();
                context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.Matches OFF");
            }
            finally
            {
                context.Database.CloseConnection();
            }
        }

        public void Clear()
        {
            try
            {
                TryClear();
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }
        }

        private void TryClear()
        {
            foreach (EncounterEntity match in context.Encounters)
            {
                context.Encounters.Remove(match);
            }
            context.SaveChanges();
        }

        public void Delete(int anId)
        {
            try
            {
                TryDelete(anId);
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }
        }

        private void TryDelete(int anId)
        {
            if (AnyWithId(anId))
            {
                DeleteExistent(anId);
            }
            else
            {
                throw new EncounterNotFoundException();
            }
        }

        private void DeleteExistent(int anId)
        {
            EncounterEntity retrieved = context.Encounters.Include(m => m.Commentaries).First(m => m.Id == anId);
            context.Encounters.Remove(retrieved);
            context.Comments.RemoveRange(retrieved.Commentaries);
            context.SaveChanges();
        }

        public Encounter Get(int anId)
        {
            Encounter toReturn;
            try
            {
                toReturn = TryGet(anId);
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }
            return toReturn;
        }

        private Encounter TryGet(int anId)
        {
            Encounter toReturn;
            if (AnyWithId(anId))
            {
                toReturn = GetExistentMatch(anId);
            }
            else
            {
                throw new EncounterNotFoundException();
            }
            return toReturn;
        }

        private Encounter GetExistentMatch(int anId)
        {
            EncounterEntity entity = context.Encounters
                .Include(m => m.SportEntity)
                .Include(m => m.Commentaries).ThenInclude(c => c.Maker)
                .First(me => me.Id == anId);

            ICollection<EncounterTeam> match_teams = context.EncounterTeams.Include(mt=> mt.Team)
                                                                   .ThenInclude(t => t.Sport)                                                                             
                                                                   .Where(mt => mt.EncounterId == anId)
                                                                   .ToList();
            Encounter conversion = matchConverter.ToEncounter(entity,match_teams);

            context.Entry(entity).State = EntityState.Detached;
            foreach (EncounterTeam mt in match_teams) {
                context.Entry(mt).State = EntityState.Detached;
            }
            return conversion;
        }

        public ICollection<Encounter> GetAll()
        {
            ICollection<Encounter> allOfThem;
            try
            {
                allOfThem = TryGetAll();
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }
            return allOfThem;
        }

        private ICollection<Encounter> TryGetAll()
        {
            ICollection<Encounter> allOfThem = new List<Encounter>();
            IQueryable<EncounterEntity> entities = context.Encounters
                                            .Include(m => m.SportEntity)
                                            .Include(m => m.Commentaries).ThenInclude(c => c.Maker).AsNoTracking();

            foreach (EncounterEntity match in entities) {
                IQueryable<EncounterTeam> matchPlayers = context.EncounterTeams
                    .Include(mt => mt.Team).ThenInclude(t=>t.Sport)
                    .Include(mt => mt.Encounter).ThenInclude(m=> m.SportEntity)
                    .Where(mt => mt.EncounterId == match.Id).AsNoTracking();
                Encounter built = matchConverter.ToEncounter(match, matchPlayers.ToList());

                allOfThem.Add(built);
            }
            return allOfThem;
        }

        public bool IsEmpty()
        {
            bool isEmpty;
            try
            {
                isEmpty = AskIfEmpty();
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }
            return isEmpty;
        }

        private bool AskIfEmpty()
        {
            return !context.Encounters.Any();
        }

        public void Modify(Encounter aMatch)
        {
            if (Exists(aMatch.Id))
            {
                ModifyExistent(aMatch);
            }
            else
            {
                throw new EncounterNotFoundException();
            }
        }

        public void ModifyExistent(Encounter aMatch)
        {
            EncounterEntity converted = matchConverter.ToEntity(aMatch);
            if (context.Encounters.Any(m => m.Id == aMatch.Id))
            {
                EncounterEntity old = context.Encounters.First(m => m.Id == aMatch.Id);
                context.Entry(old).State = EntityState.Detached;
            }
            context.Entry(converted).State = EntityState.Modified;
            ICollection<EncounterTeam> playingTeams = matchConverter.ConvertParticipants(aMatch);
            RemoveMissingParticipants(aMatch);
            AddNewParticipants(playingTeams);
            context.SaveChanges();
            context.Entry(converted).State = EntityState.Detached;
        }

        private void RemoveMissingParticipants(Encounter aMatch)
        {
            ICollection<Team> participants = aMatch.GetParticipants();
            IQueryable<EncounterTeam> missing = context.EncounterTeams
                .Where(mt =>mt.EncounterId== aMatch.Id && !participants.Any(p => p.Id == mt.TeamNumber));
            context.EncounterTeams.RemoveRange(missing);
        }

        private void AddNewParticipants(ICollection<EncounterTeam> playingTeams)
        {
            foreach (EncounterTeam p in playingTeams) {
                bool alreadyPlays = context.EncounterTeams
                .Any(mt => mt.EncounterId == p.EncounterId && mt.TeamNumber == p.TeamNumber);
                context.Entry(p).State = EntityState.Detached;
                if (!alreadyPlays)
                {
                    context.Entry(p).State = EntityState.Added;
                }
                else {
                    context.Entry(p).State = EntityState.Modified;
                }
            }
        }

        private bool AnyWithId(int anId)
        {
            return context.Encounters.Any(m => m.Id == anId);
        }

        public Commentary CommentOnEncounter(int idMatch, Commentary aComment)
        {
            if (!Exists(idMatch))
            {
                throw new EncounterNotFoundException();
            }
            return CommentOnExistingMatch(idMatch, aComment);
        }

        private Commentary CommentOnExistingMatch(int idMatch, Commentary aComment)
        {
            CommentEntity comment = commentConverter.ToEntity(aComment);
            EncounterEntity commented = context.Encounters.Include(m => m.Commentaries).First(m => m.Id == idMatch);
            commented.Commentaries.Add(comment);
            context.Attach(comment).State = EntityState.Added;
            context.SaveChanges();
            return commentConverter.ToComment(comment);
        }

        public bool Exists(int id)
        {
            bool exists;
            try
            {
                exists = AskIfExists(id);
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }
            return exists;
        }

        private bool AskIfExists(int id)
        {
            return context.Encounters.Any(m => m.Id == id);
        }

        public ICollection<Commentary> GetComments()
        {
            ICollection<Commentary> allComments;
            try
            {
                allComments = TryGetComments();
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }
            return allComments;
        }

        private ICollection<Commentary> TryGetComments()
        {
            IQueryable<CommentEntity> allComments = context.Comments.Include(c => c.Maker);
            ICollection<Commentary> conversion = allComments.Select(c => commentConverter.ToComment(c)).ToList();
            return conversion;
        }

        public Commentary GetComment(int id)
        {
            Commentary toReturn;
            try
            {
                toReturn = TryGetComment(id);
            }
            catch (DbException)
            {
                throw new DataInaccessibleException();
            }
            return toReturn;
        }

        private Commentary TryGetComment(int id)
        {
            if (!context.Comments.Any(c => c.Id == id))
            {
                throw new CommentNotFoundException();
            }
            CommentEntity retrieved = context.Comments.Include(c => c.Maker).First(c => c.Id == id);
            return commentConverter.ToComment(retrieved);
        }
    }
}