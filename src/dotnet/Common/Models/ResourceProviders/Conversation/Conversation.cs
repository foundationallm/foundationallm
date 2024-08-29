using FoundationaLLM.Common.Constants.Chat;
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
    /// The type of the session.
    /// </summary>
    public new string Type { get; set; }

    /// <summary>
    /// The Partition key.
    /// </summary>
    public string SessionId { get; set; }
    /// <summary>
    /// The number of tokens used in the session.
    /// </summary>
    public int? TokensUsed { get; set; }
    /// <summary>
    /// The name of the session.
    /// </summary>
    public override required string Name { get; set; }
    /// <summary>
    /// The UPN of the user who created the chat session.
    /// </summary>
    public string UPN { get; set; }
    /// <summary>
    /// Deleted flag used for soft delete.
    /// </summary>
    public override bool Deleted { get; set; }
    /// <summary>
    /// The list of messages associated with the session.
    /// </summary>
    [JsonIgnore]
    public List<Message> Messages { get; set; }

    /// <summary>
    /// Constructor for Session.
    /// </summary>
    public Conversation()
    {
        Id = Guid.NewGuid().ToString();
        Type = ConversationTypes.Session;
        SessionId = Id;
        TokensUsed = 0;
        Name = "New Chat";
        Messages = [];
        UPN = string.Empty;
    }

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
