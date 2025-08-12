namespace Aurora.Coinly.Application.Categories.GetList;

public sealed record GetCategoryListQuery(bool ShowDeleted) : IQuery<IReadOnlyCollection<CategoryModel>>;