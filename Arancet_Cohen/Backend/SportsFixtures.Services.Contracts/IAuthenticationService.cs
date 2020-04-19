
using ObligatorioDA2.Services.Contracts.Dtos;

namespace ObligatorioDA2.Services.Contracts
{
    public interface IAuthenticationService
    {
        void SetSession(string userName);
        void AuthenticateAdmin();
        void Authenticate();
        UserDto GetConnectedUser();
    }
}
