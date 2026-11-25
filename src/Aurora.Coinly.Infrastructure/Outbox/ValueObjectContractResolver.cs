using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace Aurora.Coinly.Infrastructure.Outbox;

// Value objects have no public constructor and Domain carries no Json.NET attributes,
// so the outbox builds them through their private constructor (Currency inside Money).
internal sealed class ValueObjectContractResolver : DefaultContractResolver
{
    protected override JsonObjectContract CreateObjectContract(Type objectType)
    {
        JsonObjectContract contract = base.CreateObjectContract(objectType);

        if (objectType.Assembly != typeof(IValueObject).Assembly ||
            !typeof(IValueObject).IsAssignableFrom(objectType) ||
            objectType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Length > 0)
        {
            return contract;
        }

        // Skip the protected copy constructor the compiler generates for records.
        // S3011: limited to Domain value objects, whose private constructor only assigns.
#pragma warning disable S3011
        ConstructorInfo? constructor = objectType
            .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
#pragma warning restore S3011
            .Where(c => c.GetParameters() is not [{ } parameter] || parameter.ParameterType != objectType)
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();

        if (constructor is null)
        {
            return contract;
        }

        contract.OverrideCreator = constructor.Invoke;

        foreach (JsonProperty parameter in CreateConstructorParameters(constructor, contract.Properties))
        {
            contract.CreatorParameters.Add(parameter);
        }

        return contract;
    }
}
