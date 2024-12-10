using FoundationaLLM.Common.Utils;
using System.Linq.Expressions;
using System.Text;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Represents a key to be shared to or received from the client.  This key can be validated
    /// by recomputing the hash using the ClientSecret + Salt as input. 
    /// </summary>
    public class ClientSecretKey
    {
        /// <summary>
        /// The FoundationaLLM instance identifier this key is associated with.
        /// </summary>
        public required string InstanceId { get; set; }

        /// <summary>
        /// The context identifier this key is associated with.
        /// </summary>
        /// <remarks>
        /// The context identifier is relative to the FoundationaLLM instance identifier.
        /// </remarks>
        public required string ContextId { get; set; }

        /// <summary>
        /// The unique identifier of this secret key.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// The client secret portion of this key.  This is information only supposed to be held in
        /// memory for a short period of time and delivered to the customer as quickly as possible.
        /// </summary>
        public required string ClientSecret { get; set; }

        /// <summary>
        /// The string representation of the client secret key.
        /// </summary>
        public string ClientSecretString =>
            "keya"
            + $".{Base58.Encode(Encoding.UTF8.GetBytes(InstanceId))}"
            + $".{Base58.Encode(Encoding.UTF8.GetBytes(ContextId!))}"
            + $".{Base58.Encode(Encoding.UTF8.GetBytes(Id))}"
            + $".{ClientSecret}"
            + ".ayek";

        /// <summary>
        /// Tries to parse a <see cref="ClientSecretKey"/> instance from the given string.
        /// </summary>
        /// <param name="clientSecretString">The string representation to be parsed.</param>
        /// <param name="clientSecretKey">The <see cref="ClientSecretKey"/> that was parsed.</param>
        /// <returns><see langword="true"/> if the input string was successfully parsed, <see langword="false"/> otherwise.</returns>
        public static bool TryParse(string clientSecretString, out ClientSecretKey? clientSecretKey)
        {
            clientSecretKey = default;

            var parts = clientSecretString.Split('.');
            if (parts.Length != 6
                || parts.Any(p => string.IsNullOrWhiteSpace(p)))
            {
                return false;
            }

            if (parts[0] != "keya" || parts[5] != "ayek")
            {
                return false;
            }

            try
            {
                var instanceIdBytes = new byte[parts[1].Length];
                var contextIdBytes = new byte[parts[2].Length];
                var idBytes = new byte[parts[3].Length];
                var clientSecretBytes = new byte[parts[4].Length];

                if (!Base58.TryDecode(parts[1], instanceIdBytes, out int instanceIdNumBytesWritten)
                    || !Base58.TryDecode(parts[2], contextIdBytes, out int contextIdNumBytesWritten)
                    || !Base58.TryDecode(parts[3], idBytes, out int idNumBytesWritten))
                {
                    return false;
                }

                clientSecretKey = new ClientSecretKey
                {
                    InstanceId = Encoding.UTF8.GetString(instanceIdBytes[..instanceIdNumBytesWritten]),
                    ContextId = Encoding.UTF8.GetString(contextIdBytes[..contextIdNumBytesWritten]),
                    Id = Encoding.UTF8.GetString(idBytes[..idNumBytesWritten]),
                    ClientSecret = parts[4]
                };

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
