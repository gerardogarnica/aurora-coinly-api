namespace Aurora.Coinly.Domain.Methods;

public interface IPaymentMethodRepository : IRepository<PaymentMethod>
{
    Task<PaymentMethod?> GetByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<PaymentMethod>> GetListAsync(Guid userId, bool showDeleted);
}