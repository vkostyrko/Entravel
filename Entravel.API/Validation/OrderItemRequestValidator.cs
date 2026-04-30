using Entravel.Contracts.Orders.SubmitOrder;
using FluentValidation;

namespace Entravel.API.Validation;

public sealed class OrderItemRequestValidator : AbstractValidator<OrderItemRequest>
{
    private const int MinQuantity = 1;

    public OrderItemRequestValidator()
    {
        RuleFor(orderItem => orderItem.InventoryId)
            .NotEmpty();

        RuleFor(orderItem => orderItem.Quantity)
            .GreaterThanOrEqualTo(MinQuantity);
    }
}

