# Accessing system logs & audit trails

Maintaining visibility into system activities is paramount for ensuring security, troubleshooting issues, and monitoring overall system health. Our system generates various logs that provide valuable insights into events, errors, and user activities, and these logs are centrally stored in an Azure Log Analytics Workspace. Accessing these logs is crucial for effective system management. Below, we outline the procedures for accessing logs within our system.

## 1. Log location

- Our system logs are stored in the Azure Log Analytics Workspace. This centralized location within Azure ensures ease of access and management.
- After deployment, customers can redirect these logs to an existing Log Analytics Workspace within their Azure environment, if desired.

## 2. Log types

- Different types of logs are generated, including:
  - **Security Logs:** Capturing security-related events and potential threats.
  - **System Logs:** Detailing system-level activities and performance metrics.
  - **Application Logs:** Recording application-specific events and errors.

## 3. Access permissions

- Access to logs is restricted to authorized personnel with the appropriate permissions using Azure RBAC.
- Ensure that only individuals with a legitimate need for log access have the required Azure Roles.

### 4. Access methods

- **Azure Portal:**
  - Navigate to the Azure Portal and access the Log Analytics Workspace for log retrieval.
- **Azure Command-Line Interface (CLI):**
  - Use Azure CLI commands for programmatic access to logs.

### 5. Log retention

- For long term retention, export logs to a storage account or archive them to a data lake.  This is not configured by default but can be added by customers after the initial deployment.
- The default retention period for logs is 30 days.  This can be changed by customers after the initial deployment.

## 6. Monitoring Tools

- Utilize Azure Monitor and Log Analytics tools to receive real-time alerts for critical events.
- Integration with Azure monitoring solutions enhances proactive incident response and system stability.
- Additional tools like Azure Sentinel can be used for advanced security monitoring and threat detection but are not configured by default.

### 8. Audit Trails

- All diagnostics are enabled by default in the standard deployment, including audit trails for Key Vault and similar resources.
- Audit trails are stored in the Azure Log Analytics Workspace.
- Regularly review audit trails to ensure the integrity and security of log data.
