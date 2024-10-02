using FoundationaLLM.Common.Models.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Conversation;

/// <summary>
/// The session object.
/// </summary>
public class Conversation : ResourceBase
{
    /// <summary>
    /// The unique identifier.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The Partition key.
    /// </summary>
    public required string SessionId { get; set; }

    /// <summary>
    /// The number of tokens used in the session.
    /// </summary>
    public int TokensUsed { get; set; } = 0;

    /// <summary>
    /// The UPN of the user who created the chat session.
    /// </summary>
    public string UPN { get; set; } = string.Empty;

    /// <summary>
    /// Deleted flag used for soft delete.
    /// </summary>
    public override bool Deleted { get; set; }

    /// <summary>
    /// The list of messages associated with the session.
    /// </summary>
    [JsonIgnore]
    public List<Message> Messages { get; set; } = [];

    /// <summary>
    /// Adds a message to the list of messages associated with the session.
    /// </summary>
    /// <param name="message">The message to be added.</param>
    public void AddMessage(Message message) =>
        Messages.Add(message);

    /// <summary>
    /// Updates an existing message in the list of messages associated with the session.
    /// </summary>
    /// <param name="message">The updated message.</param>
    public void UpdateMessage(Message message)
    {
        var match = Messages.Single(m => m.Id == message.Id);
        var index = Messages.IndexOf(match);
        Messages[index] = message;
    }
}
