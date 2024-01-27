using System.Collections.Immutable;
using Ict.AzureNative.Utilities;
using Microsoft.Extensions.Configuration;

namespace Ict.PulumiBuildingBlocks.Extensions;

public static class DictionaryExtensions
{
    /// <summary>
    /// Removes the value defined in <paramref name="excludePrefix"/> parameter from all the keys
    /// in the dictionary if they start with it. The keys which do not start with that prefix will
    /// remain unaltered.
    ///
    /// For example, if the key is <c>Example:Segment1:Segment2</c> and the <paramref name="excludePrefix"/>
    /// is <c>Example</c> then the result key will be <c>Segment1:Segment2</c>.
    /// </summary>
    public static IReadOnlyDictionary<string, string> RemoveKeyPrefix(
        this IReadOnlyDictionary<string, string> dictionary, string? excludePrefix = null)
    {
        Check.NotNull(dictionary, nameof(dictionary));

        if (string.IsNullOrWhiteSpace(excludePrefix))
        {
            return dictionary;
        }

        var result = new Dictionary<string, string>();
        foreach (var (key, value) in dictionary)
        {
            result.Add(key.StartsWith(excludePrefix, StringComparison.InvariantCulture)
                ? key.Replace(excludePrefix, string.Empty)
                : key, value);
        }

        return result.ToImmutableDictionary();
    }

    /// <summary>
    /// Build an object of type <typeparamref name="T"/> using the keys of the dictionary to generate properties
    /// and the values to populate those properties.
    /// </summary>
    /// <remarks>
    /// * The dictionary keys should match exactly the name case of the properties on type <typeparamref name="T"/>
    /// otherwise they will not be matched. For example if configuration store key is <c>example:environment</c> then 
    /// the property will be ignored (assuming the properties are correctly named for C#) as the expected key is
    /// <c>Example:Environment</c>.
    /// * If a dictionary key cannot matched to one of the properties on type <typeparamref name="T"/> the setting
    /// will be silently ignored.
    /// </remarks>
    public static T Map<T>(this IReadOnlyDictionary<string, string> dictionary) where T : new()
    {
        Check.NotNull(dictionary, nameof(dictionary));

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(dictionary)
            .Build();

        return configuration.Get<T>() ?? new T();
    }
}
