using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Domain.Interfaces.Interfaces.Services;

public interface IUserService
{
    Task RegisterUser(UserRegistrationPayload userRegistrationPayload);
}