using backend.Application.DTOs.Common;
using backend.Application.DTOs.Discussions;

namespace backend.Application.Services;

public interface IDiscussionService
{
    Task<PagedResponse<DiscussionPostResponse>> GetPositionDiscussionsAsync(Guid positionId, int page = 1, int pageSize = 50, CancellationToken ct = default);
    Task<DiscussionPostResponse> CreatePostAsync(Guid positionId, Guid authorId, CreateDiscussionPostRequest request, CancellationToken ct = default);
}
