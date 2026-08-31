using backend.Application.DTOs.Profile;

namespace backend.Application.Services;

public interface IProfileService
{
    Task<ProfileResponse> GetProfileAsync(Guid userId, CancellationToken ct = default);
    Task<ProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default);

    Task<List<AttributeValueResponse>> GetAttributeValuesAsync(Guid userId, CancellationToken ct = default);
    Task<AttributeValueResponse> AddAttributeToProfileAsync(Guid userId, AddAttributeToProfileRequest request, CancellationToken ct = default);
    Task<AttributeValueResponse> UpdateAttributeValueAsync(Guid userId, Guid attributeValueId, UpdateAttributeValueRequest request, CancellationToken ct = default);
    Task RemoveAttributeFromProfileAsync(Guid userId, Guid attributeValueId, CancellationToken ct = default);
}
