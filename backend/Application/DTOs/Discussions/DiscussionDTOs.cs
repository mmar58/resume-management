namespace backend.Application.DTOs.Discussions;

public record DiscussionPostResponse(
    Guid Id,
    Guid PositionId,
    Guid AuthorId,
    string AuthorName,
    string Content,
    DateTime CreatedAt
);

public record CreateDiscussionPostRequest(
    string Content
);
