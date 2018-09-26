﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessLogic;
using ObligatorioDA2.DataAccess.Entities;

namespace ObligatorioDA2.DataAccess.Domain.Mappers
{
    public class MatchMapper
    {
        private TeamMapper teamConverter;
        private CommentMapper commentConverter;

        public MatchMapper()
        {
            teamConverter = new TeamMapper();
            commentConverter = new CommentMapper();
        }
        public MatchEntity ToEntity(Match aMatch)
        {
            MatchEntity conversion = new MatchEntity()
            {
                Id = aMatch.Id,
                HomeTeam = teamConverter.ToEntity(aMatch.HomeTeam),
                AwayTeam = teamConverter.ToEntity(aMatch.AwayTeam),
                Date = aMatch.Date,
                Commentaries = TransformCommentaries(aMatch.GetAllCommentaries())
            };
            return conversion;
        }

        private ICollection<CommentEntity> TransformCommentaries(ICollection<Commentary> commentaries)
        {
            return commentaries.Select(c => commentConverter.ToEntity(c)).ToList();
        }

        public Match ToMatch(MatchEntity entity)
        {
            Team home = teamConverter.ToTeam(entity.HomeTeam);
            Team away = teamConverter.ToTeam(entity.AwayTeam);
            ICollection<Commentary> comments = entity.Commentaries.Select(ce => commentConverter.ToComment(ce)).ToList();
            DateTime date = entity.Date;
            Match created = new Match(entity.Id, home, away, date, comments);
            return created;
        }
    }
}

