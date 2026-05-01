using Entravel.Application.Validation;
using FluentValidation;

namespace Entravel.Application.Orders.SubmitOrder;

public sealed class SubmitOrderCommandValidator : AbstractValidator<SubmitOrderCommand>
{
    private const int MinQuantity = 1;
    private const decimal MinTotalAmount = 0.01m;
    private const decimal MinDiscount = 0m;
    private const decimal MaxDiscount = 100m;

    public SubmitOrderCommandValidator()
    {
        RuleFor(command => command.CustomerId)
            .NotEmpty()
            .WithMessage(ValidationMessages.CustomerIdRequired);

        RuleFor(command => command.Items)
            .NotEmpty()
            .WithMessage(ValidationMessages.ItemsRequired);

        RuleForEach(command => command.Items).ChildRules(orderItem =>
        {
            orderItem.RuleFor(item => item.InventoryId)
                .NotEmpty()
                .WithMessage(ValidationMessages.ItemInventoryIdRequired);

            orderItem.RuleFor(item => item.Quantity)
                .GreaterThanOrEqualTo(MinQuantity)
                .WithMessage(ValidationMessages.ItemQuantityMustBePositive);
        });

        RuleFor(command => command.TotalAmount)
            .GreaterThanOrEqualTo(MinTotalAmount)
            .WithMessage(ValidationMessages.TotalAmountMustBePositive);

        RuleFor(command => command.Discount)
            .InclusiveBetween(MinDiscount, MaxDiscount)
            .WithMessage("Discount must be between 0 and 100.");
    }
}

