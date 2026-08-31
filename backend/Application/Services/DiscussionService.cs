using backend.Application.DTOs.Common;
using backend.Application.DTOs.Discussions;
using backend.Data;
using backend.Domain.Entities;
using backend.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Services;

public class DiscussionService : IDiscussionService
{
    private readonly AppDbContext _db;
    private readonly IHubContext<DiscussionHub> _hubContext;

    public DiscussionService(AppDbContext db, IHubContext<DiscussionHub> hubContext)
    {
        _db = db;
        _hubContext = hubContext;
    }

    public async Task<PagedResponse<DiscussionPostResponse>> GetPositionDiscussionsAsync(Guid positionId, int page = 1, int pageSize = 50, CancellationToken ct = default)
    {
        var query = _db.DiscussionPosts
            .Include(p => p.Author)
            .Where(p => p.PositionId == positionId)
            .AsNoTracking();

        var totalCount = await query.CountAsync(ct);

        var posts = await query
            .OrderBy(p => p.CreatedAt) // Chronological order
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = posts.Select(MapPost).ToList();
        return new PagedResponse<DiscussionPostResponse>(items, totalCount, page, pageSize);
    }

    public async Task<DiscussionPostResponse> CreatePostAsync(Guid positionId, Guid authorId, CreateDiscussionPostRequest request, CancellationToken ct = default)
    {
        var positionExists = await _db.Positions.AnyAsync(p => p.Id == positionId, ct);
        if (!positionExists)
            throw new KeyNotFoundException("Position not found.");

        var post = new DiscussionPost
        {
            PositionId = positionId,
            AuthorId = authorId,
            Content = request.Content
        };

        _db.DiscussionPosts.Add(post);
        await _db.SaveChangesAsync(ct);

        // Reload with author for mapping
        var createdPost = await _db.DiscussionPosts
            .Include(p => p.Author)
            .FirstAsync(p => p.Id == post.Id, ct);

        var response = MapPost(createdPost);

        // Broadcast to clients listening in this position's group
        await _hubContext.Clients.Group($"position-{positionId}").SendAsync("ReceivePost", response, cancellationToken: ct);

        return response;
    }

    private static DiscussionPostResponse MapPost(DiscussionPost p) => new(
        Id: p.Id,
        PositionId: p.PositionId,
        AuthorId: p.AuthorId,
        AuthorName: p.Author.UserName ?? "Unknown",
        Content: p.Content,
        CreatedAt: p.CreatedAt
    );
}
