using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.Services.Contracts.Dtos;

namespace ObligatorioDA2.Services.Mappers
{
    public class UserDtoMapper
    {
        public UserDto ToDto(User aUser) {
            UserDto data = new UserDto()
            {
                name = aUser.Name,
                surname = aUser.Surname,
                username = aUser.UserName,
                password = aUser.Password,
                email = aUser.Email,
                isAdmin = aUser.IsAdmin
            };
            return data;
        }

    }
}
