using backend.Application.DTOs.Discussions;
using FluentValidation;

namespace backend.Validation;

public class CreateDiscussionPostRequestValidator : AbstractValidator<CreateDiscussionPostRequest>
{
    public CreateDiscussionPostRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(4000);
    }
}
