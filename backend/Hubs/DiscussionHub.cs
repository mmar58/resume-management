using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace backend.Hubs;

/// <summary>
/// SignalR hub for real-time discussion updates on Positions.
/// Clients join a group named after the position ID.
/// New posts are broadcast to all connected viewers within 2-5 seconds (Section 18).
/// </summary>
[Authorize]
public class DiscussionHub : Hub
{
    /// <summary>
    /// Called by clients to subscribe to a position's discussion feed.
    /// </summary>
    public async Task JoinPosition(string positionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"position-{positionId}");
    }

    /// <summary>
    /// Called by clients to unsubscribe from a position's discussion feed.
    /// </summary>
    public async Task LeavePosition(string positionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"position-{positionId}");
    }
}
