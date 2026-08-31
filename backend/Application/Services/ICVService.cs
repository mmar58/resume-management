using backend.Application.DTOs.Common;
using backend.Application.DTOs.CVs;

namespace backend.Application.Services;

public interface ICVService
{
    // For Candidates: Create, update, view their own CVs, submit/withdraw
    Task<CVResponse> CreateCVAsync(Guid userId, CreateCVRequest request, CancellationToken ct = default);
    Task<CVResponse> UpdateCVAsync(Guid userId, Guid cvId, UpdateCVRequest request, CancellationToken ct = default);
    Task<CVResponse> GetCandidateCVAsync(Guid userId, Guid cvId, CancellationToken ct = default);
    Task<PagedResponse<CVSummaryResponse>> GetCandidateCVsAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<CVResponse> ChangeCVStatusAsync(Guid userId, Guid cvId, ChangeCVStatusRequest request, CancellationToken ct = default);

    // For Recruiters: Browse submitted CVs, view full CV, like/unlike
    Task<PagedResponse<CVSummaryResponse>> GetSubmittedCVsAsync(Guid recruiterId, Guid? positionId = null, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<CVResponse> GetCVForReviewAsync(Guid recruiterId, Guid cvId, CancellationToken ct = default);
    Task ToggleCVLikeAsync(Guid recruiterId, Guid cvId, CancellationToken ct = default);
}
