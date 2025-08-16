using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.CQRS;
using Ambev.DeveloperEvaluation.ORM.Redis;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Users.ListUsers;

public class ListUsersHandler : IQueryHandler<ListUsersQuery, ListUsersResult>
{
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly ILogger<ListUsersHandler> _logger;

    public ListUsersHandler(
        IUserRepository userRepository,
        ICacheService cacheService,
        IMapper mapper,
        ILogger<ListUsersHandler> logger)
    {
        _userRepository = userRepository;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ListUsersResult> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listing users with filters - Page: {Page}, Limit: {Limit}, Sort: {Sort}",
            request.Page, request.Limit, request.Sort);

        var cacheKey = $"users:list:{request.Page}:{request.Limit}:{request.Sort}:{request.Email}:{request.Username}:{request.Role}:{request.Status}";
        
        var cachedResult = await _cacheService.GetAsync<ListUsersResult>(cacheKey);
        if (cachedResult != null)
        {
            _logger.LogInformation("Users list retrieved from cache");
            return cachedResult;
        }

        var paginatedUsers = await _userRepository.GetAllAsync(request.Page, request.Limit, request.Sort, request.Email, request.Username, request.Role, request.Status, cancellationToken);

        var result = new ListUsersResult
        {
            Users = _mapper.Map<List<GetUserResult>>(paginatedUsers),
            TotalCount = paginatedUsers.TotalCount,
            Page = request.Page,
            PageSize = request.Limit
        };
        
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
        
        _logger.LogInformation("Retrieved {Count} users from database", result.Users.Count);
        
        return result;
    }
}