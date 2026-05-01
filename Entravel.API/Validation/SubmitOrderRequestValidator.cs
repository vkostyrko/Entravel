using Entravel.Contracts.Orders.SubmitOrder;
using FluentValidation;

namespace Entravel.API.Validation;

public sealed class SubmitOrderRequestValidator : AbstractValidator<SubmitOrderRequest>
{
    private const decimal MinTotalAmount = 0.01m;
    private const decimal MinDiscount = 0m;
    private const decimal MaxDiscount = 100m;

    public SubmitOrderRequestValidator()
    {
        RuleFor(request => request.CustomerId)
            .NotEmpty();

        RuleFor(request => request.Items)
            .NotEmpty();

        RuleForEach(request => request.Items)
            .SetValidator(new OrderItemRequestValidator());

        RuleFor(request => request.TotalAmount)
            .GreaterThanOrEqualTo(MinTotalAmount);

        RuleFor(request => request.Discount)
            .InclusiveBetween(MinDiscount, MaxDiscount);
    }
}

