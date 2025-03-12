namespace Aurora.Coinly.Application.Categories.GetList;

public sealed record GetListQuery(bool ShowDeleted) : IQuery<IReadOnlyCollection<CategoryModel>>;