namespace Aurora.Coinly.Infrastructure.Serialization;

internal static class SerializerSettings
{
    public static readonly JsonSerializerSettings Instance = new()
    {
        TypeNameHandling = TypeNameHandling.All,
        MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        ContractResolver = new ValueObjectContractResolver()
    };
}
