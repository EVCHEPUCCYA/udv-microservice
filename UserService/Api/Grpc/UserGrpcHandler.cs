using Grpc.Core;
using UserService.Core.Contracts;
using UserService.Protos;

namespace UserService.Api.Grpc;

public sealed class UserGrpcHandler : Protos.UserService.UserServiceBase
{
    private readonly IUserStore _store;

    public UserGrpcHandler(IUserStore store)
    {
        _store = store;
    }

    public override async Task<UserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        var entity = await _store.FindByIdAsync(request.Id);
        
        if (entity == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"User with id {request.Id} not found"));
        }

        return new UserResponse
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.EmailAddress
        };
    }

    public override async Task<GetAllUsersResponse> GetAllUsers(GetAllUsersRequest request, ServerCallContext context)
    {
        var entities = await _store.ListAllAsync();
        
        var response = new GetAllUsersResponse();
        response.Users.AddRange(entities.Select(e => new UserResponse
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.EmailAddress
        }));

        return response;
    }
}

