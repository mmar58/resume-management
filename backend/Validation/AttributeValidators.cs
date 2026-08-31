using backend.Application.DTOs.Attributes;
using FluentValidation;
using backend.Domain.Enums;

namespace backend.Validation;

public class CreateAttributeRequestValidator : AbstractValidator<CreateAttributeRequest>
{
    public CreateAttributeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Attribute name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Category)
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(2000);
            
        RuleFor(x => x.Options)
            .NotEmpty().WithMessage("Dropdown attributes must have at least one option.")
            .When(x => x.DataType == AttributeDataType.OneOfMany);
            
        RuleForEach(x => x.Options)
            .MaximumLength(500)
            .When(x => x.Options != null);
    }
}

public class UpdateAttributeRequestValidator : AbstractValidator<UpdateAttributeRequest>
{
    public UpdateAttributeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Attribute name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Category)
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(2000);
            
        RuleForEach(x => x.Options)
            .MaximumLength(500)
            .When(x => x.Options != null);
    }
}
