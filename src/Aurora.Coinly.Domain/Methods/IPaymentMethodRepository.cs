namespace Aurora.Coinly.Domain.Methods;

public interface IPaymentMethodRepository : IRepository<PaymentMethod>
{
    Task<PaymentMethod?> GetByIdAsync(Guid id);
    Task<IEnumerable<PaymentMethod>> GetListAsync(bool showDeleted);
}