# Purging Deleted Conversations

This guide explains how to anonymize deleted conversations in Cosmos DB to remove sensitive information while preserving statistical data.

## Overview

When users delete conversations in FoundationaLLM:
- The session is **soft-deleted** (marked as deleted)
- Original content remains in the database
- Statistical data (tokens, feedback) is preserved

For compliance or security requirements, you may need to anonymize this content.

## Anonymization Approach

The anonymization process:
1. Identifies soft-deleted sessions
2. Replaces sensitive content with "Deleted"
3. Preserves metadata for analytics

### What Gets Anonymized

| Document Type | Anonymized Fields |
|---------------|-------------------|
| `CompletionPrompt` | `prompt` |
| `Message` | `text`, `content.value`, `analysisResults.toolInput`, `analysisResults.toolOutput` |

### What's Preserved

| Data | Purpose |
|------|---------|
| Document IDs | Reference integrity |
| Timestamps | Analytics |
| Token counts | Usage metrics |
| User feedback | Quality metrics |
| Session structure | Statistical analysis |

## Creating the Stored Procedure

### Step 1: Open Azure Portal

1. Navigate to your Cosmos DB account
2. Select **Data Explorer**

### Step 2: Create Stored Procedure

1. Expand your database
2. Right-click the **Sessions** container
3. Select **New Stored Procedure**

### Step 3: Add Stored Procedure Code

Name: `anonymizeDeletedSession`

```javascript
function anonymizeDeletedSession() {
    var collection = getContext().getCollection();
    var response = getContext().getResponse();

    // Retrieve Session document
    var sessionQuery = `SELECT * FROM c WHERE c.type = 'Session'`;

    var isSessionAccepted = collection.queryDocuments(
        collection.getSelfLink(),
        sessionQuery,
        {},
        function (err, sessionDocuments, responseOptions) {
            if (err) throw err;

            if (sessionDocuments.length === 0 || !sessionDocuments[0].deleted) {
                response.setBody("Session cannot be anonymized because it was not deleted.");
                return;
            }

            // Session marked as deleted, proceed
            var query = `SELECT * FROM c WHERE c.type = 'CompletionPrompt' OR c.type = 'Message'`;

            var isAccepted = collection.queryDocuments(
                collection.getSelfLink(),
                query,
                {},
                function (err, documents, responseOptions) {
                    if (err) throw err;

                    if (documents.length === 0) {
                        response.setBody("No related documents found.");
                        return;
                    }

                    var updatedCount = 0;
                    var processedCount = 0;

                    documents.forEach(function (doc) {
                        if (doc.type === "CompletionPrompt") {
                            doc.prompt = "Deleted";
                        }

                        if (doc.type === "Message") {
                            doc.text = "Deleted";

                            if (Array.isArray(doc.content)) {
                                doc.content.forEach(function (contentItem) {
                                    contentItem.value = "Deleted";
                                });
                            }

                            if (Array.isArray(doc.analysisResults)) {
                                doc.analysisResults.forEach(function (analysisItem) {
                                    analysisItem.toolInput = "Deleted";
                                    analysisItem.toolOutput = "Deleted";
                                });
                            }
                        }

                        // Update the document
                        var acceptUpdate = collection.replaceDocument(doc._self, doc, function (err) {
                            if (err) throw err;

                            updatedCount++;
                            processedCount++;

                            // Check if all documents processed
                            if (processedCount === documents.length) {
                                response.setBody("Updated " + updatedCount + " documents.");
                            }
                        });

                        if (!acceptUpdate) throw new Error("Update not accepted, aborting");
                    });
                }
            );

            if (!isAccepted) throw new Error("Query was not accepted by server.");
        }
    );

    if (!isSessionAccepted) throw new Error("Session query was not accepted by server.");
}
```

### Step 4: Save

Click **Save** to create the stored procedure.

## Executing the Stored Procedure

### Via Azure Portal

1. Navigate to **Data Explorer**
2. Expand **Sessions** container > **Stored Procedures**
3. Select `anonymizeDeletedSession`
4. Click **Execute**
5. In the input panel:
   - **Partition key value**: Enter the `sessionId` of the deleted session
6. Click **Execute**

### Expected Output

**Success:**
```
Updated X documents.
```

**Session Not Deleted:**
```
Session cannot be anonymized because it was not deleted.
```

**No Related Documents:**
```
No related documents found.
```

### Via Azure CLI

```bash
# Execute stored procedure
az cosmosdb sql stored-procedure execute \
  --account-name <cosmos-account> \
  --database-name <database> \
  --container-name Sessions \
  --name anonymizeDeletedSession \
  --partition-key-value <session-id> \
  --resource-group <resource-group>
```

## Bulk Anonymization

For bulk anonymization of multiple sessions:

### Query Deleted Sessions

```sql
SELECT c.id, c.sessionId 
FROM c 
WHERE c.type = 'Session' AND c.deleted = true
```

### Automation Script

```powershell
# Get all deleted sessions
$deletedSessions = az cosmosdb sql query \
    --account-name $cosmosAccount \
    --database-name $database \
    --container-name Sessions \
    --query "SELECT c.sessionId FROM c WHERE c.type = 'Session' AND c.deleted = true" \
    | ConvertFrom-Json

# Anonymize each session
foreach ($session in $deletedSessions) {
    Write-Host "Anonymizing session: $($session.sessionId)"
    
    az cosmosdb sql stored-procedure execute `
        --account-name $cosmosAccount `
        --database-name $database `
        --container-name Sessions `
        --name anonymizeDeletedSession `
        --partition-key-value $session.sessionId `
        --resource-group $resourceGroup
}
```

## Scheduling Automatic Purges

### Using Azure Functions

Create a timer-triggered function to run periodically:

```csharp
[FunctionName("AnonymizeDeletedSessions")]
public static async Task Run(
    [TimerTrigger("0 0 2 * * *")] TimerInfo timer, // Daily at 2 AM
    [CosmosDB(
        databaseName: "%CosmosDbDatabase%",
        containerName: "Sessions",
        Connection = "CosmosDbConnection")] CosmosClient client,
    ILogger log)
{
    // Implementation to find and anonymize deleted sessions
}
```

> **TODO:** Provide complete Azure Function implementation for scheduled anonymization.

## Compliance Considerations

| Requirement | Implementation |
|-------------|----------------|
| **GDPR Right to Erasure** | Anonymize user data on request |
| **Data Retention** | Preserve aggregate statistics |
| **Audit Trail** | Log anonymization operations |
| **Verification** | Query to confirm anonymization |

## Verification Query

Confirm anonymization:

```sql
SELECT 
    c.type,
    c.text,
    c.prompt
FROM c 
WHERE c.sessionId = '<session-id>'
AND (c.type = 'Message' OR c.type = 'CompletionPrompt')
```

Expected result: All `text` and `prompt` fields show "Deleted".

## Related Topics

- [Backups & Data Resiliency](backups.md)
- [Platform Security](../security-permissions/platform-security.md)
- [Logs & Monitoring](../monitoring-troubleshooting/logs.md)
