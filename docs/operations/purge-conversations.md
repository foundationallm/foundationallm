# Cosmos DB stored procedure to anonymize deleted sessions (conversations)

User conversations are stored in Cosmos DB. When a user deletes a conversation, the session document and associated documents (CompletionPrompt and Message) are marked as deleted (soft-deleted). As opposed to hard-deleting the documents, which would remove them from the database, soft-deleting allows for the possibility of restoring the conversation later. However, in some cases, it may be necessary to anonymize the deleted session to ensure that no sensitive information is retained in the database. Anonymizing the deleted session involves updating the associated documents to remove any potentially sensitive information, such as user input or analysis results. As opposed to hard-deleting the documents, which would remove them from the database, soft-deleting allows for the possibility of retaining statistical information and other metrics, such as token usage, user feedback, etc.

This stored procedure anonymizes the deleted session by updating the associated documents to remove any potentially sensitive information.

## Creating the stored procedure

To create the stored procedure, follow these steps:

1. Open the Azure portal and navigate to your Cosmos DB account.
2. Select the "Data Explorer" option from the left-hand menu.
3. Right-click the **Sessions** container and select the **New Stored Procedure** option.
4. Copy and paste the following code into the stored procedure editor:

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

5. Provide a name for the stored procedure (e.g., `anonymizeDeletedSession`).
6. Select the **Save** button to create the stored procedure.

## Executing the stored procedure

When you wish to anonymize a deleted session, you can execute the stored procedure by following these steps:

1. In the Azure portal, navigate to your Cosmos DB account and select the **Data Explorer** option.
2. Expand the **Sessions** container and expand the **Stored Procedures** option.
3. Select the stored procedure you created (e.g., `anonymizeDeletedSession`).
4. Click the **Execute** button to run the stored procedure.
5. In the input parameters panel that appears, enter the `sessionId` of the session you want to anonymize into the **Partition key value** field, then select **Execute**.
6. The stored procedure will run and anonymize the deleted session, updating the associated documents as necessary. The output will indicate the number of documents that were updated.
7. If the session was not deleted, the output will indicate that the session cannot be anonymized because it was not deleted.
