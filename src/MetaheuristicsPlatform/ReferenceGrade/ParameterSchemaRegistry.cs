namespace MetaheuristicsPlatform.ReferenceGrade;

public sealed record ReferenceParameterDescriptor(
    string Name,
    string TypeName,
    string DefaultValue,
    string Description,
    double? Minimum = null,
    double? Maximum = null)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name) ||
            string.IsNullOrWhiteSpace(TypeName) ||
            string.IsNullOrWhiteSpace(Description))
            throw new ArgumentException(
                "Parameter schema fields must not be empty.");

        if (Minimum is not null &&
            Maximum is not null &&
            Minimum > Maximum)
            throw new ArgumentException(
                "Parameter minimum must not exceed maximum.");
    }
}

public sealed class ParameterSchemaRegistry
{
    private readonly Dictionary<string, IReadOnlyList<ReferenceParameterDescriptor>> _schemas =
        new(StringComparer.Ordinal);

    public IReadOnlyCollection<string> AlgorithmIds => _schemas.Keys;

    public void Register(
        string algorithmId,
        IEnumerable<ReferenceParameterDescriptor> descriptors)
    {
        if (string.IsNullOrWhiteSpace(algorithmId))
            throw new ArgumentException(
                "Algorithm ID must not be empty.",
                nameof(algorithmId));

        ArgumentNullException.ThrowIfNull(descriptors);

        ReferenceParameterDescriptor[] items = descriptors.ToArray();
        foreach (ReferenceParameterDescriptor item in items)
            item.Validate();

        if (items.Select(x => x.Name).Distinct(StringComparer.Ordinal).Count() != items.Length)
            throw new ArgumentException(
                "Parameter names must be unique within one algorithm schema.",
                nameof(descriptors));

        if (!_schemas.TryAdd(algorithmId.Trim(), items))
            throw new InvalidOperationException(
                "A parameter schema is already registered for this stable algorithm ID.");
    }

    public IReadOnlyList<ReferenceParameterDescriptor> Get(string algorithmId)
    {
        if (!_schemas.TryGetValue(algorithmId, out IReadOnlyList<ReferenceParameterDescriptor>? schema))
            throw new KeyNotFoundException(
                "No parameter schema is registered for this stable algorithm ID.");

        return schema;
    }
}
