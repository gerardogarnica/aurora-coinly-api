namespace Aurora.Coinly.Application.Categories.GetById;

public sealed record GetByIdQuery(Guid Id) : IQuery<CategoryModel>;