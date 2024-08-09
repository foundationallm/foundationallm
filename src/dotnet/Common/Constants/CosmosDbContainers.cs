﻿namespace FoundationaLLM.Common.Constants
{
    /// <summary>
    /// Core FoundationaLLM Cosmos DB container names.
    /// </summary>
    public static class CosmosDbContainers
    {
        /// <summary>
        /// Stores chat sessions and related messages.
        /// </summary>
        public const string Sessions = "Sessions";

        /// <summary>
        /// Stores a mapping between user identities and chat sessions.
        /// </summary>
        public const string UserSessions = "UserSessions";

        /// <summary>
        /// Stores user profile data.
        /// </summary>
        public const string UserProfiles = "UserProfiles";

        /// <summary>
        /// Stores state data for background processing.
        /// </summary>
        public const string State = "State";

        /// <summary>
        /// The Cosmos DB change feed leases container.
        /// </summary>
        public const string Leases = "leases";
    }
}
