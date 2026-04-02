using System.Text.Json.Serialization;
using Core.Interfaces;
using Core.Services;
using Core.Validators;
using Data.Contexts;
using Data.Repositories;
using Data.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Testing_project.Mappers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<ProductValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IDishRepository, DishRepository>();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IDishService, DishService>();

builder.Services.AddAutoMapper(typeof(ProductMappingProfile), typeof(DishMappingProfile), typeof(IngredientMappingProfile));

builder.Services.AddDbContext<BookOfReceiptsDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.Run();