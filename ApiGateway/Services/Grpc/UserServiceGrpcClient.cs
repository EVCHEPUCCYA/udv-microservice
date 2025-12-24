using ApiGateway.Models;
using Grpc.Net.Client;
using Serilog;
using UserService.Protos;

namespace ApiGateway.Services;

public sealed class UserServiceGrpcClient : IUserServiceClient
{
    private readonly UserService.Protos.UserService.UserServiceClient _grpcClient;
    private readonly Serilog.ILogger _logger;

    public UserServiceGrpcClient(IConfiguration configuration, Serilog.ILogger logger)
    {
        var serviceEndpoint = configuration["Services:UserServiceGrpc"] ?? "http://localhost:5001";
        var grpcChannel = GrpcChannel.ForAddress(serviceEndpoint);
        _grpcClient = new UserService.Protos.UserService.UserServiceClient(grpcChannel);
        _logger = logger;
    }

    public async Task<User?> GetUserAsync(int userId)
    {
        try
        {
            var grpcRequest = new GetUserRequest { Id = userId };
            var grpcResponse = await _grpcClient.GetUserAsync(grpcRequest);
            
            return new User
            {
                Id = grpcResponse.Id,
                FirstName = grpcResponse.FirstName,
                LastName = grpcResponse.LastName,
                Email = grpcResponse.Email
            };
        }
        catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            _logger.Warning("User {UserId} not found via gRPC", userId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving user {UserId} via gRPC", userId);
            return null;
        }
    }
}

