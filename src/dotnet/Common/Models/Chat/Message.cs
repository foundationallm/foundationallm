using Azure.Search.Documents.Indexes;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Common.Models.Chat;

/// <summary>
/// The message object.
/// </summary>
public record Message
{
    /// <summary>
    /// The unique identifier.
    /// </summary>
    [SearchableField(IsKey = true, IsFilterable = true)]
    public string Id { get; set; }
    /// <summary>
    /// The type of the message.
    /// </summary>
    [SimpleField]
    public string Type { get; set; }
    /// <summary>
    /// The Partition key.
    /// </summary>
    [SimpleField]
    public string SessionId { get; set; }
    /// <summary>
    /// The timestamp when the message was created.
    /// </summary>
    [SimpleField]
    public DateTime TimeStamp { get; set; }
    /// <summary>
    /// The sender of the message.
    /// </summary>
    [SimpleField]
    public string Sender { get; set; }
    /// <summary>
    /// The display name of the message sender. This could be the name of the signed in user or the name of the agent.
    /// </summary>
    [SimpleField]
    public string? SenderDisplayName { get; set; }
    /// <summary>
    /// The number of tokens associated with the message, if any.
    /// </summary>
    [SimpleField]
    public int? Tokens { get; set; }
    /// <summary>
    /// The text content of the message.
    /// </summary>
    [SimpleField]
    public string Text { get; set; }
    /// <summary>
    /// The rating associated with the message, if any.
    /// </summary>
    [SimpleField]
    public bool? Rating { get; set; }
    /// <summary>
    /// The UPN of the user who created the chat session.
    /// </summary>
    public string UPN { get; set; }
    /// <summary>
    /// Deleted flag used for soft delete.
    /// </summary>
    public bool Deleted { get; set; }
    /// <summary>
    /// The vector associated with the message.
    /// </summary>
    [FieldBuilderIgnore]
    public float[]? Vector { get; set; }
    /// <summary>
    /// The identifier for the completion prompt associated with the message.
    /// </summary>
    public string? CompletionPromptId { get; set; }

    /// <summary>
    /// Stores the expected completion for the message and used for evaluating the actual vs. expected agent completion.
    /// This should be stored in the agent response.
    /// </summary>
    public string? ExpectedCompletion { get; set; }

    /// <summary>
    /// The sources associated with the completion prompt.
    /// </summary>
    public Citation[]? Citations { get; set; }

    /// <summary>
    /// Constructor for Message.
    /// </summary>
    public Message(string sessionId, string sender, int? tokens, string text,
        float[]? vector, bool? rating, string upn, string? senderDisplayName = null,
        Citation[]? citations = null, string? expectedCompletion = null)
    {
        Id = Guid.NewGuid().ToString();
        Type = nameof(Message);
        SessionId = sessionId;
        Sender = sender;
        SenderDisplayName = senderDisplayName;
        Tokens = tokens ?? 0;
        TimeStamp = DateTime.UtcNow;
        Text = text;
        Rating = rating;
        Vector = vector;
        UPN = upn;
        ExpectedCompletion = expectedCompletion;
        Citations = citations;
    }
}
