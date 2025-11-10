using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Core.Examples.Constants;

namespace FoundationaLLM.Core.Examples.Catalogs
{
    /// <summary>
    /// Contains the agent definitions for use in the FoundationaLLM Core examples.
    /// These definitions are used to create the agents in the FoundationaLLM Core examples.
    /// </summary>
    public static class AgentCatalog
    {
        #region Knowledge Management agents

        /// <summary>
        /// Catalog of knowledge management agents.
        /// </summary>
        public static readonly List<GenericAgent> KnowledgeManagementAgents =
        [
            new GenericAgent
            {
                Name = TestAgentNames.GenericInlineContextAgentName,
                Description = "A generic agent that can handle inline context completions.",
                SessionsEnabled = true,
                ConversationHistorySettings = new AgentConversationHistorySettings
                {
                    Enabled = true,
                    MaxHistory = 10
                },
                GatekeeperSettings = new AgentGatekeeperSettings
                {
                    UseSystemSetting = false
                }
            },
            new GenericAgent
            {
                Name = TestAgentNames.SemanticKernelInlineContextAgentName,
                Description = "SemanticKernel agent that can handle inline context completions.",
                SessionsEnabled = true,
                ConversationHistorySettings = new AgentConversationHistorySettings
                {
                    Enabled = true,
                    MaxHistory = 10
                },
                GatekeeperSettings = new AgentGatekeeperSettings
                {
                    UseSystemSetting = false
                }
            },
            new GenericAgent
            {
                Name = TestAgentNames.SemanticKernelAgentName,
                Description = "SemanticKernel agent that can handle completions.",
                SessionsEnabled = true,
                ConversationHistorySettings = new AgentConversationHistorySettings
                {
                    Enabled = true,
                    MaxHistory = 10
                },
                GatekeeperSettings = new AgentGatekeeperSettings
                {
                    UseSystemSetting = false
                }
            },
            new GenericAgent
            {
                Name = TestAgentNames.LangChainAgentName,
                Description = "LangChain agent that can handle completions.",
                SessionsEnabled = true,
                ConversationHistorySettings = new AgentConversationHistorySettings
                {
                    Enabled = true,
                    MaxHistory = 10
                },
                GatekeeperSettings = new AgentGatekeeperSettings
                {
                    UseSystemSetting = false
                }
            },
            new GenericAgent
            {
                Name = TestAgentNames.SemanticKernelSDZWA,
                Description = "Knowledge Management Agent that queries the San Diego Zoo Wildlife Alliance journals using SemanticKernel.",
                SessionsEnabled = true,
                ConversationHistorySettings = new AgentConversationHistorySettings
                {
                    Enabled = true,
                    MaxHistory = 10
                },
                GatekeeperSettings = new AgentGatekeeperSettings
                {
                    UseSystemSetting = false
                }            },
            new GenericAgent
            {
                Name = TestAgentNames.LangChainSDZWA,
                Description = "Knowledge Management Agent that queries the San Diego Zoo Wildlife Alliance journals using LangChain.",
                SessionsEnabled = true,
                ConversationHistorySettings = new AgentConversationHistorySettings
                {
                    Enabled = true,
                    MaxHistory = 10
                },
                GatekeeperSettings = new AgentGatekeeperSettings
                {
                    UseSystemSetting = false
                }
            },
            new GenericAgent
            {
                Name = TestAgentNames.ConversationGeneratorAgent,
                Description = "An agent that creates conversations based on product descriptions.",
                SessionsEnabled = true,
                ConversationHistorySettings = new AgentConversationHistorySettings
                {
                    Enabled = true,
                    MaxHistory = 10
                },
                GatekeeperSettings = new AgentGatekeeperSettings
                {
                    UseSystemSetting = false
                }
            },
            new GenericAgent
            {
                Name = TestAgentNames.Dune01,
                Description = "Knowledge Management Agent that queries the Dune books using SemanticKernel.",
                SessionsEnabled = true,
                ConversationHistorySettings = new AgentConversationHistorySettings
                {
                    Enabled = true,
                    MaxHistory = 10
                },
                GatekeeperSettings = new AgentGatekeeperSettings
                {
                    UseSystemSetting = false
                }
            },
            new GenericAgent
            {
                Name = TestAgentNames.Dune02,
                Description = "Inline Context Agent that writes poems about Dune suitable for being used in wartime songs.",
                SessionsEnabled = true,
                ConversationHistorySettings = new AgentConversationHistorySettings
                {
                    Enabled = true,
                    MaxHistory = 10
                },
                GatekeeperSettings = new AgentGatekeeperSettings
                {
                    UseSystemSetting = false
                }
            },
            new GenericAgent
            {
                Name = TestAgentNames.Dune03,
                Description = "Answers questions about Dune by asking for help from other agents.",
                SessionsEnabled = true,
                ConversationHistorySettings = new AgentConversationHistorySettings
                {
                    Enabled = true,
                    MaxHistory = 10
                },
                GatekeeperSettings = new AgentGatekeeperSettings
                {
                    UseSystemSetting = false
                }
            },
            new GenericAgent
            {
                Name = TestAgentNames.LangChainDune,
                Description = "Knowledge Management Agent that queries the Dune books using LangChain.",
                SessionsEnabled = true,
                ConversationHistorySettings = new AgentConversationHistorySettings
                {
                    Enabled = true,
                    MaxHistory = 10
                },
                GatekeeperSettings = new AgentGatekeeperSettings
                {
                    UseSystemSetting = false
                }
            },

        ];
        #endregion

        /// <summary>
        /// Retrieves all agents defined in the catalog.
        /// </summary>
        /// <returns></returns>
        public static List<AgentBase> GetAllAgents()
        {
            var agents = new List<AgentBase>();
            agents.AddRange(KnowledgeManagementAgents);

            return agents;
        }
    }
}
