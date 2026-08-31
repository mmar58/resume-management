using backend.Application.DTOs.CVs;
using FluentValidation;

namespace backend.Validation;

public class CreateCVRequestValidator : AbstractValidator<CreateCVRequest>
{
    public CreateCVRequestValidator()
    {
        RuleFor(x => x.PositionId)
            .NotEmpty().WithMessage("Position ID is required.");
    }
}

public class UpdateCVRequestValidator : AbstractValidator<UpdateCVRequest>
{
    public UpdateCVRequestValidator()
    {
        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion is required for updates.");
    }
}

public class ChangeCVStatusRequestValidator : AbstractValidator<ChangeCVStatusRequest>
{
    public ChangeCVStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid CV status.");
            
        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion is required for status changes.");
    }
}
