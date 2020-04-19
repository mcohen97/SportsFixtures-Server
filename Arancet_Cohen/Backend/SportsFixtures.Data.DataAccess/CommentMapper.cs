using System.Collections.Generic;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.Data.Entities;

namespace ObligatorioDA2.Data.DomainMappers
{
    public class CommentMapper
    {
        private UserMapper usersMapper;

        public CommentMapper()
        {
            usersMapper = new UserMapper();
        }
        public CommentEntity ToEntity(Commentary comment)
        {

            CommentEntity converted = new CommentEntity()
            {
                Id = comment.Id,
                Text = comment.Text,
                Maker = usersMapper.ToEntity(comment.Maker)
            };
            return converted;
        }

        public Commentary ToComment(CommentEntity commentEntity)
        {
            User maker = usersMapper.ToUser(commentEntity.Maker, new List<TeamEntity>());
            Commentary conversion = new Commentary(commentEntity.Id, commentEntity.Text, maker);
            return conversion;
        }
    }
}
