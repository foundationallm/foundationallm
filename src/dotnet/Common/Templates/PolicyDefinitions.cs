using System.Collections.ObjectModel;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Represents all policy definitions used in PBAC.
    /// </summary>
    public static class PolicyDefinitions
    {
        public static readonly ReadOnlyDictionary<string, PolicyDefinition> All = new (
            new Dictionary<string, PolicyDefinition>()
            {
                {
                    "/providers/FoundationaLLM.Authorization/policyDefinitions/00000000-0000-0000-0001-000000000001",
                    new PolicyDefinition
                    {
                        Name = "00000000-0000-0000-0001-000000000001",
                        Type = "FoundationaLLM.Authorization/policyDefinitions",
                        ObjectId = "/providers/FoundationaLLM.Authorization/policyDefinitions/00000000-0000-0000-0001-000000000001",
                        DisplayName = "User Principal Name (UPN) match",
                        Description = "FoundationaLLM PBAC policy definition that defines ownership by a match on the User Principal Name (UPN).",
                        AssignableScopes = [
                            "/",],
                        MatchingStrategy = new PolicyMatchingStrategy
                        {
                            UserIdentityProperties = [
                                "UPN"
                            ]
                        },
                        CreatedOn = DateTimeOffset.Parse("2024-09-25T00:00:00.0000000Z"),
                        UpdatedOn = DateTimeOffset.Parse("2024-09-25T00:00:00.0000000Z"),
                        CreatedBy = null,
                        UpdatedBy = null
                    }
                },
            });
    }
}
