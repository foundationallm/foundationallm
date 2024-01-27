using System.Net;

namespace Ict.PulumiBuildingBlocks.Extensions
{
    public static class IpAddressExtensions
    {
        public static IPAddress Increment(this IPAddress value, int increment=1)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var ip = BitConverter.ToInt32(value.GetAddressBytes().Reverse().ToArray(), 0);
            ip += increment;
            return new IPAddress(BitConverter.GetBytes(ip).Reverse().ToArray());
        }

    }
}
