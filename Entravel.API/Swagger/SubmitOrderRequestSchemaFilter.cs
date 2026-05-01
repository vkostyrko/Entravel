using Entravel.Contracts.Orders.SubmitOrder;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Entravel.API.Swagger;

public sealed class SubmitOrderRequestSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != typeof(SubmitOrderRequest))
        {
            return;
        }

        schema.Example = new OpenApiObject
        {
            ["customerId"] = new OpenApiString("11111111-1111-1111-1111-111111111111"),
            ["items"] = new OpenApiArray
            {
                new OpenApiObject
                {
                    ["inventoryId"] = new OpenApiString("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    ["quantity"] = new OpenApiInteger(1)
                }
            },
            ["totalAmount"] = new OpenApiDouble(100.00),
            ["discount"] = new OpenApiDouble(10)
        };
    }
}

