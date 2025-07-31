namespace Aurora.Coinly.Application.Categories.GetList;

public sealed record GetCategoryListQuery(Guid UserId, bool ShowDeleted) : IQuery<IReadOnlyCollection<CategoryModel>>;