using System.Transactions;
using TradeWiseBackend.Domain.Interfaces.Repositories;
using TradeWiseBackend.Domain.Interfaces.Services;
using TradeWiseBackend.Domain.Models;
using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Bll.Services;

public class UserService(IAccountsRepository accountsRepository) : IUserService
{
    public async Task RegisterUser(UserRegistrationPayload userRegistrationPayload)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var account = new Account
        {
            // TODO: добавить хеширование пароля
            PasswordHash = userRegistrationPayload.Password,
            Email = userRegistrationPayload.Email
        };

        await accountsRepository.Create(account);
        scope.Complete();
    }
}