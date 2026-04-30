using Entravel.API.ExceptionHandlers;
using Entravel.Application.Orders.SubmitOrder;
using Entravel.API.Mapping;
using Entravel.API.Validation;
using Entravel.EF.Dependencies;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SubmitOrderCommand>());
builder.Services.AddValidatorsFromAssemblyContaining<SubmitOrderCommand>();
builder.Services.AddValidatorsFromAssemblyContaining<SubmitOrderRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddEfInfrastructure();
builder.Services.AddAutoMapper(typeof(OrderMappingProfile).Assembly);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();
