﻿using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Chat
{
    /// <summary>
    /// Represents an historic message sender and text item.
    /// </summary>
    public class MessageHistoryItem
    {
        /// <summary>
        /// The sender of the message (e.g. "Agent", "User").
        /// </summary>
        [JsonPropertyName("sender")]
        public string Sender { get; set; }
        /// <summary>
        /// The message text.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }

        /// <summary>
        /// Message history item
        /// </summary>
        /// <param name="sender">The sender of the message (e.g., "Agent", "User")</param>
        /// <param name="text">The message text.</param>
        public MessageHistoryItem(string sender, string text)
        {
            Sender = sender;
            Text = text;
        }
    }
}
