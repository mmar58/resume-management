using backend.Application.DTOs.Positions;
using FluentValidation;

namespace backend.Validation;

public class CreatePositionRequestValidator : AbstractValidator<CreatePositionRequest>
{
    public CreatePositionRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(300);

        RuleFor(x => x.ShortDescription)
            .MaximumLength(2000);

        RuleFor(x => x.Company)
            .MaximumLength(200);

        RuleFor(x => x.Level)
            .MaximumLength(50);

        RuleForEach(x => x.ProjectTags)
            .MaximumLength(100)
            .When(x => x.ProjectTags != null);

        RuleForEach(x => x.AccessRules).SetValidator(new CreateAccessRuleRequestValidator());
    }
}

public class UpdatePositionRequestValidator : AbstractValidator<UpdatePositionRequest>
{
    public UpdatePositionRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(300);

        RuleFor(x => x.ShortDescription)
            .MaximumLength(2000);

        RuleFor(x => x.Company)
            .MaximumLength(200);

        RuleFor(x => x.Level)
            .MaximumLength(50);

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion is required for updates.");

        RuleForEach(x => x.ProjectTags)
            .MaximumLength(100)
            .When(x => x.ProjectTags != null);

        RuleForEach(x => x.AccessRules).SetValidator(new CreateAccessRuleRequestValidator());
    }
}

public class CreateAccessRuleRequestValidator : AbstractValidator<CreateAccessRuleRequest>
{
    public CreateAccessRuleRequestValidator()
    {
        RuleFor(x => x.AttributeDefinitionId)
            .NotEmpty().WithMessage("AttributeDefinitionId is required for an access rule.");
            
        RuleFor(x => x.Operator)
            .IsInEnum().WithMessage("Invalid operator.");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Value is required for an access rule.")
            .MaximumLength(500);
    }
}
