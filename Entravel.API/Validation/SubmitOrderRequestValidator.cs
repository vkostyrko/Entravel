using Entravel.Contracts.Orders.SubmitOrder;
using FluentValidation;

namespace Entravel.API.Validation;

public sealed class SubmitOrderRequestValidator : AbstractValidator<SubmitOrderRequest>
{
    public SubmitOrderRequestValidator()
    {
        RuleFor(request => request.CustomerId)
            .NotEmpty();

        RuleFor(request => request.Items)
            .NotEmpty();

        RuleForEach(request => request.Items)
            .SetValidator(new OrderItemRequestValidator());
    }
}

