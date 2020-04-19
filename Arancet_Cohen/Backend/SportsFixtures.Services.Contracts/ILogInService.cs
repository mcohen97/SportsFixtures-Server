using ObligatorioDA2.Services.Contracts.Dtos;


namespace ObligatorioDA2.Services.Contracts
{
    public interface ILogInService
    {
        UserDto Login(string username, string password);
    }
}
