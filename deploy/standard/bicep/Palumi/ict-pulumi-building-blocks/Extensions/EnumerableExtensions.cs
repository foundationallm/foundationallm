using System.Collections.Immutable;
using Ict.Configuration.Network;


namespace Ict.PulumiBuildingBlocks.Extensions
{
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Helper Extension to call <see cref="IEnumerable{T}"/> and access an indexer.
        /// Example:
        /// <code>
        ///   foreach (var (item, index) in collection.WithIndex())
        ///   {
        ///      Console.WriteLine($"{index}: {item}");
        ///   }
        /// </code>
        /// </summary>
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self) 
            => self?.Select((item, index) => (item, index)) ?? new List<(T, int)>();

        /// <summary>
        /// Converts the list of IPRanges into a list of IP addresses, using CIDR notation for brevity where
        /// the range contains more than four IPs.
        /// </summary>
        /// <returns>An immutable array containing IP address and CIDR ranges</returns>
        public static ImmutableArray<string> AsMixedIpArray(this IEnumerable<IPRange> ipRanges)
        {
            return ipRanges.AsMixedIp().ToImmutableArray();
        }

        internal static IEnumerable<string> AsMixedIp(this IEnumerable<IPRange> ipRanges)
        {
            foreach (var cidr in ipRanges.SelectMany(range => range.CidrList))
            {
                if (cidr.Total <= 4)
                {
                    foreach (var addr in cidr.ListIPAddress())
                    {
                        yield return addr.ToString();
                    }
                }
                else
                {
                    yield return cidr.ToString();
                }
            }
        }
    }
}
