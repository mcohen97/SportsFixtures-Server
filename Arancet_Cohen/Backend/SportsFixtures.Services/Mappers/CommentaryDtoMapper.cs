using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.Data.Repositories.Contracts;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.Services.Contracts.Dtos;

namespace ObligatorioDA2.Services.Mappers
{
    public class CommentaryDtoMapper
    {
        private IUserRepository usersStorage;
        public CommentaryDtoMapper(IUserRepository usersRepo) {
            usersStorage = usersRepo;
        }
        public CommentaryDto ToDto(Commentary aCommentary) {
            return new CommentaryDto()
            {
                commentId = aCommentary.Id,
                makerUsername = aCommentary.Maker.UserName,
                text = aCommentary.Text
            };
        }

        public Commentary ToCommentary(CommentaryDto dto) {
            User maker = TryGetMaker(dto.makerUsername);
            return new Commentary(dto.commentId, dto.text, maker);
        }

        private User TryGetMaker(string username)
        {
            try
            {
                return usersStorage.Get(username);
            }
            catch (UserNotFoundException e)
            {
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
        }
    }
}
