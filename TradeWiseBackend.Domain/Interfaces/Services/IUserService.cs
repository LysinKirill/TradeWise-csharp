using TradeWiseBackend.Domain.ServiceModels;

namespace TradeWiseBackend.Domain.Interfaces.Services;

public interface IUserService
{
    Task RegisterUser(UserRegistrationPayload userRegistrationPayload);
}