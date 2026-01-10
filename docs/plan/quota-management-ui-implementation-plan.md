# Implementation Plan: Quotas Section in Management Portal

## Overview

This plan outlines the creation of a new **Quotas** section in the Management Portal with two subsections:
1. **Quota Management** - CRUD operations for quota definitions
2. **Quota Dashboards** - Real-time usage monitoring and reporting

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                        Management Portal (Vue.js)                    │
├─────────────────────────────────────────────────────────────────────┤
│  Quotas Section                                                      │
│  ├── Quota Management (CRUD)                                        │
│  │   ├── List all quotas                                            │
│  │   ├── Create new quota                                           │
│  │   ├── Edit existing quota                                        │
│  │   └── Delete quota                                               │
│  └── Quota Dashboards (Reporting)                                   │
│      ├── Real-time usage metrics                                    │
│      ├── Per-user/per-agent breakdown                              │
│      └── Historical usage charts                                    │
└─────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│                     Management API (ASP.NET Core)                    │
├─────────────────────────────────────────────────────────────────────┤
│  New: FoundationaLLM.Quota Resource Provider                        │
│  ├── GET    /quotaDefinitions          (List all)                   │
│  ├── GET    /quotaDefinitions/{name}   (Get one)                    │
│  ├── POST   /quotaDefinitions/{name}   (Create/Update)              │
│  ├── DELETE /quotaDefinitions/{name}   (Delete)                     │
│  ├── GET    /quotaMetrics              (Get usage metrics)          │
│  └── GET    /quotaMetrics/history      (Get historical data)        │
└─────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│                          Storage Layer                               │
├─────────────────────────────────────────────────────────────────────┤
│  Azure Blob Storage: quota/quota-store.json (definitions)           │
│  Azure Cosmos DB or Table Storage (metrics/history - new)           │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Phase 1: Backend - Quota Resource Provider

### 1.1 Create New Resource Provider

**Files to create:**

| File | Purpose |
|------|---------|
| `src/dotnet/Common/Constants/ResourceProviders/QuotaResourceProviderMetadata.cs` | Resource provider metadata |
| `src/dotnet/Common/Constants/ResourceProviders/QuotaResourceTypeNames.cs` | Resource type constants |
| `src/dotnet/Quota/ResourceProviders/QuotaResourceProviderService.cs` | Main resource provider service |
| `src/dotnet/Quota/Quota.csproj` | New project file |

**Add to `ResourceProviderNames.cs`:**
```csharp
/// <summary>
/// The name of the FoundationaLLM.Quota resource provider.
/// </summary>
public const string FoundationaLLM_Quota = "FoundationaLLM.Quota";
```

### 1.2 Quota Definition Model Enhancement

**Update `QuotaDefinition.cs`** to include resource properties:

```csharp
public class QuotaDefinition : ResourceBase
{
    // Existing properties...
    
    [JsonPropertyName("object_id")]
    public string? ObjectId { get; set; }
    
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }
    
    [JsonPropertyName("created_by")]
    public string? CreatedBy { get; set; }
    
    [JsonPropertyName("created_on")]
    public DateTime? CreatedOn { get; set; }
    
    [JsonPropertyName("updated_by")]
    public string? UpdatedBy { get; set; }
    
    [JsonPropertyName("updated_on")]
    public DateTime? UpdatedOn { get; set; }
}
```

### 1.3 New Model: Quota Usage Metrics

**Create `src/dotnet/Common/Models/Quota/QuotaUsageMetrics.cs`:**

```csharp
public class QuotaUsageMetrics
{
    [JsonPropertyName("quota_name")]
    public required string QuotaName { get; set; }
    
    [JsonPropertyName("quota_context")]
    public required string QuotaContext { get; set; }
    
    [JsonPropertyName("partition_id")]
    public required string PartitionId { get; set; }
    
    [JsonPropertyName("current_count")]
    public int CurrentCount { get; set; }
    
    [JsonPropertyName("limit")]
    public int Limit { get; set; }
    
    [JsonPropertyName("utilization_percentage")]
    public double UtilizationPercentage { get; set; }
    
    [JsonPropertyName("lockout_active")]
    public bool LockoutActive { get; set; }
    
    [JsonPropertyName("lockout_remaining_seconds")]
    public int LockoutRemainingSeconds { get; set; }
    
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }
}
```

### 1.4 New Model: Quota Usage History

**Create `src/dotnet/Common/Models/Quota/QuotaUsageHistory.cs`:**

```csharp
public class QuotaUsageHistory
{
    [JsonPropertyName("quota_name")]
    public required string QuotaName { get; set; }
    
    [JsonPropertyName("partition_id")]
    public required string PartitionId { get; set; }
    
    [JsonPropertyName("time_bucket")]
    public DateTimeOffset TimeBucket { get; set; }
    
    [JsonPropertyName("request_count")]
    public int RequestCount { get; set; }
    
    [JsonPropertyName("exceeded_count")]
    public int ExceededCount { get; set; }
    
    [JsonPropertyName("lockout_count")]
    public int LockoutCount { get; set; }
}
```

### 1.5 Update IQuotaService Interface

**Add new methods to `IQuotaService.cs`:**

```csharp
public interface IQuotaService
{
    // Existing methods...
    
    /// <summary>
    /// Gets all quota definitions.
    /// </summary>
    Task<List<QuotaDefinition>> GetQuotaDefinitionsAsync();
    
    /// <summary>
    /// Gets a quota definition by name.
    /// </summary>
    Task<QuotaDefinition?> GetQuotaDefinitionAsync(string name);
    
    /// <summary>
    /// Creates or updates a quota definition.
    /// </summary>
    Task<QuotaDefinition> UpsertQuotaDefinitionAsync(QuotaDefinition quotaDefinition);
    
    /// <summary>
    /// Deletes a quota definition.
    /// </summary>
    Task DeleteQuotaDefinitionAsync(string name);
    
    /// <summary>
    /// Gets current usage metrics for all quotas.
    /// </summary>
    Task<List<QuotaUsageMetrics>> GetQuotaUsageMetricsAsync();
    
    /// <summary>
    /// Gets usage history for a specific quota.
    /// </summary>
    Task<List<QuotaUsageHistory>> GetQuotaUsageHistoryAsync(
        string quotaName,
        DateTimeOffset startTime,
        DateTimeOffset endTime);
}
```

### 1.6 Resource Provider Endpoints

**API Routes to implement:**

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/instances/{instanceId}/providers/FoundationaLLM.Quota/quotaDefinitions` | List all quotas |
| `GET` | `/instances/{instanceId}/providers/FoundationaLLM.Quota/quotaDefinitions/{name}` | Get single quota |
| `POST` | `/instances/{instanceId}/providers/FoundationaLLM.Quota/quotaDefinitions/{name}` | Create/update quota |
| `DELETE` | `/instances/{instanceId}/providers/FoundationaLLM.Quota/quotaDefinitions/{name}` | Delete quota |
| `POST` | `/instances/{instanceId}/providers/FoundationaLLM.Quota/quotaDefinitions/checkname` | Validate name |
| `GET` | `/instances/{instanceId}/providers/FoundationaLLM.Quota/quotaMetrics` | Get current metrics |
| `POST` | `/instances/{instanceId}/providers/FoundationaLLM.Quota/quotaMetrics/filter` | Filter metrics |
| `GET` | `/instances/{instanceId}/providers/FoundationaLLM.Quota/quotaMetrics/history` | Get history |

---

## Phase 2: Frontend - Management Portal UI

### 2.1 New Page Structure

**Create the following files:**

```
src/ui/ManagementPortal/pages/quotas/
├── index.vue                          # Quota list (Quota Management)
├── create.vue                         # Create new quota
├── edit/
│   └── [quotaName].vue               # Edit existing quota
└── dashboards/
    └── index.vue                      # Quota Dashboards
```

### 2.2 Update Sidebar Navigation

**Modify `components/Sidebar.vue`** to add Quotas section:

```vue
<!-- Add after Security section -->
<!-- Quotas -->
<h3 class="sidebar__section-header">
    <span class="pi pi-chart-bar" aria-hidden="true"></span>
    <span>Quotas</span>
</h3>
<ul>
    <li>
        <NuxtLink
            to="/quotas"
            :class="{ 'router-link-active': isRouteActive('/quotas') && !isRouteActive('/quotas/dashboards') }"
            class="sidebar__item"
        >Quota Management</NuxtLink>
    </li>
    <li>
        <NuxtLink
            to="/quotas/dashboards"
            :class="{ 'router-link-active': isRouteActive('/quotas/dashboards') }"
            class="sidebar__item"
        >Quota Dashboards</NuxtLink>
    </li>
</ul>
```

### 2.3 TypeScript Types

**Add to `js/types.ts`:**

```typescript
export interface QuotaDefinition {
    name: string;
    description: string;
    context: string;
    type: 'RawRequestRateLimit' | 'AgentRequestRateLimit';
    metric_partition: 'None' | 'UserPrincipalName' | 'UserIdentifier';
    metric_limit: number;
    metric_window_seconds: number;
    lockout_duration_seconds: number;
    distributed_enforcement: boolean;
    object_id?: string;
    display_name?: string;
    created_by?: string;
    created_on?: string;
    updated_by?: string;
    updated_on?: string;
}

export interface QuotaUsageMetrics {
    quota_name: string;
    quota_context: string;
    partition_id: string;
    current_count: number;
    limit: number;
    utilization_percentage: number;
    lockout_active: boolean;
    lockout_remaining_seconds: number;
    timestamp: string;
}

export interface QuotaUsageHistory {
    quota_name: string;
    partition_id: string;
    time_bucket: string;
    request_count: number;
    exceeded_count: number;
    lockout_count: number;
}
```

### 2.4 API Methods

**Add to `js/api.ts`:**

```typescript
/*
    Quotas
*/
async getQuotas(): Promise<ResourceProviderGetResult<QuotaDefinition>[]> {
    return await this.fetch(
        `/instances/${this.instanceId}/providers/FoundationaLLM.Quota/quotaDefinitions?api-version=${this.apiVersion}`,
    );
},

async getQuota(quotaName: string): Promise<ResourceProviderGetResult<QuotaDefinition>[]> {
    return await this.fetch(
        `/instances/${this.instanceId}/providers/FoundationaLLM.Quota/quotaDefinitions/${quotaName}?api-version=${this.apiVersion}`,
    );
},

async createQuota(quota: QuotaDefinition): Promise<ResourceProviderUpsertResult> {
    return await this.fetch(
        `/instances/${this.instanceId}/providers/FoundationaLLM.Quota/quotaDefinitions/${quota.name}?api-version=${this.apiVersion}`,
        {
            method: 'POST',
            body: quota,
        },
    );
},

async deleteQuota(quotaName: string): Promise<void> {
    return await this.fetch(
        `/instances/${this.instanceId}/providers/FoundationaLLM.Quota/quotaDefinitions/${quotaName}?api-version=${this.apiVersion}`,
        {
            method: 'DELETE',
        },
    );
},

async checkQuotaName(name: string): Promise<CheckNameResponse> {
    return await this.fetch(
        `/instances/${this.instanceId}/providers/FoundationaLLM.Quota/quotaDefinitions/checkname?api-version=${this.apiVersion}`,
        {
            method: 'POST',
            body: { name },
        },
    );
},

async getQuotaMetrics(): Promise<QuotaUsageMetrics[]> {
    return await this.fetch(
        `/instances/${this.instanceId}/providers/FoundationaLLM.Quota/quotaMetrics?api-version=${this.apiVersion}`,
    );
},

async getQuotaMetricsHistory(
    quotaName: string,
    startTime: string,
    endTime: string
): Promise<QuotaUsageHistory[]> {
    return await this.fetch(
        `/instances/${this.instanceId}/providers/FoundationaLLM.Quota/quotaMetrics/history?api-version=${this.apiVersion}`,
        {
            method: 'POST',
            body: {
                quota_name: quotaName,
                start_time: startTime,
                end_time: endTime,
            },
        },
    );
},
```

### 2.5 Quota Management Page (`pages/quotas/index.vue`)

**Features:**
- DataTable listing all quota definitions
- Columns: Name, Context, Type, Partition, Limit, Window, Actions
- Create button → `/quotas/create`
- Edit button → `/quotas/edit/[quotaName]`
- Delete button with confirmation dialog
- Search/filter capability

### 2.6 Create Quota Page (`pages/quotas/create.vue`)

**Form Fields:**

| Field | Type | Validation |
|-------|------|------------|
| Name | Text input | Required, alphanumeric + hyphens |
| Description | Textarea | Optional |
| Context | Dropdown + text | Required, format validation |
| Type | Dropdown | `RawRequestRateLimit` / `AgentRequestRateLimit` |
| Metric Partition | Dropdown | `None` / `UserPrincipalName` / `UserIdentifier` |
| Metric Limit | Number input | Required, min 1 |
| Metric Window (seconds) | Number input | Required, multiple of 20 |
| Lockout Duration (seconds) | Number input | Required, min 0 |
| Distributed Enforcement | Toggle | Boolean |

**Context Helper:**
- Dropdown for service (e.g., `CoreAPI`)
- Dropdown for controller (populated based on service)
- Optional: Agent name field (shown when `AgentRequestRateLimit` selected)

### 2.7 Edit Quota Page (`pages/quotas/edit/[quotaName].vue`)

Similar to create page but:
- Pre-populated with existing values
- Name field is read-only
- Shows created/updated metadata

### 2.8 Quota Dashboards Page (`pages/quotas/dashboards/index.vue`)

**Features:**

#### Summary Cards
- Total active quotas
- Quotas currently in lockout
- Average utilization %
- Highest utilization quota

#### Real-Time Usage Table
| Column | Description |
|--------|-------------|
| Quota Name | Name of the quota |
| Context | API:Controller:Agent |
| Partition | User/Global identifier |
| Current / Limit | e.g., "45 / 100" |
| Utilization | Progress bar + percentage |
| Status | Active / Lockout |
| Lockout Remaining | Countdown timer if in lockout |

#### Historical Charts (using Chart.js or similar)
- **Line Chart**: Request count over time (per quota)
- **Bar Chart**: Exceeded/lockout events per day
- **Heatmap**: Usage patterns by hour of day

#### Filters
- Time range selector (Last hour, 24h, 7d, 30d, Custom)
- Quota selector (All / specific quota)
- Partition filter (All users / specific user)

---

## Phase 3: New Components

### 3.1 QuotaForm Component

**Create `components/QuotaForm.vue`:**
- Reusable form for create/edit
- Context builder with validation
- Real-time validation feedback

### 3.2 QuotaUsageCard Component

**Create `components/QuotaUsageCard.vue`:**
- Displays single quota usage metrics
- Progress bar visualization
- Auto-refresh capability

### 3.3 QuotaUsageChart Component

**Create `components/QuotaUsageChart.vue`:**
- Line/bar chart for historical data
- Time range selection
- Responsive design

---

## Phase 4: Backend Metrics Collection

### 4.1 Enhance QuotaService for Metrics

**Update `QuotaService.cs`** to:
- Track metrics per partition
- Store periodic snapshots for history
- Expose current state via API

### 4.2 Metrics Storage Options

**Option A: In-Memory with Periodic Snapshot**
- Keep rolling window in memory
- Snapshot to blob storage periodically
- Suitable for moderate traffic

**Option B: Cosmos DB**
- Real-time writes for all events
- Efficient time-series queries
- Better for high-volume scenarios

**Recommended: Start with Option A**, migrate to Option B if needed.

---

## Phase 5: File Changes Summary

### Backend Files to Create

| File | Description |
|------|-------------|
| `src/dotnet/Quota/Quota.csproj` | New project |
| `src/dotnet/Quota/ResourceProviders/QuotaResourceProviderService.cs` | Resource provider |
| `src/dotnet/Common/Constants/ResourceProviders/QuotaResourceProviderMetadata.cs` | Metadata |
| `src/dotnet/Common/Constants/ResourceProviders/QuotaResourceTypeNames.cs` | Type names |
| `src/dotnet/Common/Models/Quota/QuotaUsageMetrics.cs` | Metrics model |
| `src/dotnet/Common/Models/Quota/QuotaUsageHistory.cs` | History model |

### Backend Files to Modify

| File | Changes |
|------|---------|
| `src/dotnet/Common/Constants/ResourceProviders/ResourceProviderNames.cs` | Add `FoundationaLLM_Quota` |
| `src/dotnet/Common/Interfaces/IQuotaService.cs` | Add CRUD + metrics methods |
| `src/dotnet/Common/Services/Quota/QuotaService.cs` | Implement new methods |
| `src/dotnet/Common/Models/Quota/QuotaDefinition.cs` | Add resource properties |
| `src/dotnet/ManagementAPI/Program.cs` | Register Quota resource provider |

### Frontend Files to Create

| File | Description |
|------|-------------|
| `src/ui/ManagementPortal/pages/quotas/index.vue` | Quota list page |
| `src/ui/ManagementPortal/pages/quotas/create.vue` | Create quota page |
| `src/ui/ManagementPortal/pages/quotas/edit/[quotaName].vue` | Edit quota page |
| `src/ui/ManagementPortal/pages/quotas/dashboards/index.vue` | Dashboards page |
| `src/ui/ManagementPortal/components/QuotaForm.vue` | Reusable form |
| `src/ui/ManagementPortal/components/QuotaUsageCard.vue` | Usage card |
| `src/ui/ManagementPortal/components/QuotaUsageChart.vue` | Charts |

### Frontend Files to Modify

| File | Changes |
|------|---------|
| `src/ui/ManagementPortal/components/Sidebar.vue` | Add Quotas section |
| `src/ui/ManagementPortal/js/api.ts` | Add quota API methods |
| `src/ui/ManagementPortal/js/types.ts` | Add quota TypeScript types |

---

## Phase 6: Implementation Order

| Step | Task | Estimated Effort |
|------|------|------------------|
| 1 | Create Quota Resource Provider (backend) | 2-3 days |
| 2 | Implement CRUD operations in QuotaService | 1-2 days |
| 3 | Add API types and methods (frontend) | 0.5 day |
| 4 | Create Quota Management pages (list, create, edit) | 2-3 days |
| 5 | Update Sidebar navigation | 0.5 day |
| 6 | Implement metrics collection in QuotaService | 2-3 days |
| 7 | Create Quota Dashboards page with charts | 2-3 days |
| 8 | Testing and refinement | 2-3 days |

---

## Phase 7: Local Development & Testing

### 7.1 Development Environment Setup

#### Prerequisites
- .NET SDK installed
- Node.js installed
- VS Code / Cursor with appropriate extensions
- Access to Azure resources (storage account, app configuration)

### 7.2 Building the Solution

After making changes to the .NET backend, rebuild the solution:

```powershell
cd C:\Repos\foundationallm\src\dotnet && dotnet build FoundationaLLM.sln --configuration Debug 2>&1 | Select-Object -Last 30
```

> **Note:** Always rebuild before running to ensure your changes are compiled.

### 7.3 Running Backend APIs Locally

#### Core API (Debug Mode)
```powershell
cd C:\Repos\foundationallm\src\dotnet\CoreAPI && dotnet run --configuration Debug --no-build 2>&1
```

#### Management API (Debug Mode)
```powershell
cd C:\Repos\foundationallm\src\dotnet\ManagementAPI && dotnet run --configuration Debug --no-build 2>&1
```

> **Tip:** Run both APIs in separate terminal windows to test full functionality.

### 7.4 Running Frontend Portals Locally

#### User Portal
1. Open VS Code / Cursor
2. Use the **"User Portal UI - Backend"** launch configuration
3. The portal will start and connect to the local Core API

#### Management Portal
1. Open VS Code / Cursor
2. Use the **"Management Portal UI - Backend"** launch configuration
3. The portal will start and connect to the local Management API

### 7.5 Testing Workflow

#### Backend Testing Sequence

| Step | Command | Purpose |
|------|---------|---------|
| 1 | `dotnet build FoundationaLLM.sln --configuration Debug` | Compile all changes |
| 2 | Start Management API | Host the quota resource provider |
| 3 | Test API endpoints with REST client or curl | Verify CRUD operations |

#### Example API Test Commands

```bash
# List all quotas
curl -X GET "https://localhost:7001/instances/{instanceId}/providers/FoundationaLLM.Quota/quotaDefinitions?api-version=2024-02-16" \
  -H "Authorization: Bearer {token}"

# Create a quota
curl -X POST "https://localhost:7001/instances/{instanceId}/providers/FoundationaLLM.Quota/quotaDefinitions/test-quota?api-version=2024-02-16" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "test-quota",
    "description": "Test quota for development",
    "context": "CoreAPI:Completions",
    "type": "RawRequestRateLimit",
    "metric_partition": "UserPrincipalName",
    "metric_limit": 100,
    "metric_window_seconds": 60,
    "lockout_duration_seconds": 60,
    "distributed_enforcement": false
  }'

# Get quota metrics
curl -X GET "https://localhost:7001/instances/{instanceId}/providers/FoundationaLLM.Quota/quotaMetrics?api-version=2024-02-16" \
  -H "Authorization: Bearer {token}"
```

#### Frontend Testing Sequence

| Step | Action | Purpose |
|------|--------|---------|
| 1 | Start Management API locally | Backend must be running |
| 2 | Launch "Management Portal UI - Backend" | Start frontend dev server |
| 3 | Navigate to `/quotas` | Test Quota Management page |
| 4 | Create/Edit/Delete quotas | Verify CRUD operations |
| 5 | Navigate to `/quotas/dashboards` | Test Quota Dashboards page |

### 7.6 Test Scenarios

#### Quota Management Tests

| Test Case | Steps | Expected Result |
|-----------|-------|-----------------|
| List quotas | Navigate to `/quotas` | See table of existing quotas |
| Create quota | Click "Create", fill form, submit | New quota appears in list |
| Validate name | Enter invalid name (spaces, special chars) | Validation error shown |
| Edit quota | Click edit on existing quota | Form pre-populated, can update |
| Delete quota | Click delete, confirm | Quota removed from list |
| Duplicate name | Try to create with existing name | Error message displayed |

#### Quota Dashboard Tests

| Test Case | Steps | Expected Result |
|-----------|-------|-----------------|
| View metrics | Navigate to `/quotas/dashboards` | See current usage for all quotas |
| Real-time update | Make API requests while viewing | Metrics update in real-time |
| Historical data | Select time range | Charts display historical usage |
| Filter by quota | Select specific quota | Dashboard filters to that quota |
| Lockout display | Trigger a lockout | Status shows "Lockout" with countdown |

### 7.7 Integration Testing

To test quota enforcement end-to-end:

1. **Create a test quota** via Management Portal:
   ```json
   {
     "name": "dev-test-quota",
     "context": "CoreAPI:Completions",
     "type": "RawRequestRateLimit",
     "metric_partition": "UserPrincipalName",
     "metric_limit": 5,
     "metric_window_seconds": 60,
     "lockout_duration_seconds": 30
   }
   ```

2. **Start Core API** locally

3. **Make rapid requests** to trigger the quota:
   ```powershell
   # PowerShell script to test quota
   for ($i = 1; $i -le 10; $i++) {
       $response = Invoke-RestMethod -Uri "https://localhost:5001/instances/{instanceId}/completions" `
           -Method POST -Headers @{Authorization="Bearer $token"} `
           -Body '{"user_prompt":"test"}' -ContentType "application/json"
       Write-Host "Request $i : $($response.StatusCode)"
   }
   ```

4. **Verify in Dashboard** that:
   - Usage count increases
   - Lockout triggers after limit exceeded
   - 429 responses are returned

### 7.8 Debugging Tips

| Issue | Solution |
|-------|----------|
| API returns 401 | Check bearer token is valid and not expired |
| API returns 404 | Verify resource provider is registered in `Program.cs` |
| Portal shows blank page | Check browser console for JavaScript errors |
| Metrics not updating | Ensure Core API has quota service enabled |
| Changes not reflected | Rebuild solution with `dotnet build` |

### 7.9 Development Checklist

Before submitting changes:

- [ ] Solution builds without errors
- [ ] All existing unit tests pass
- [ ] New unit tests written for quota resource provider
- [ ] Management API starts and serves quota endpoints
- [ ] Core API quota enforcement still works
- [ ] Management Portal Quota Management pages functional
- [ ] Management Portal Quota Dashboards display correctly
- [ ] No console errors in browser
- [ ] Code follows existing patterns and conventions

---

## Phase 8: Documentation Updates

### 8.1 Documentation Structure

The new Quotas section documentation will follow the existing pattern in the Management Portal docs:

```
docs/docs/management-portal/
├── how-to-guides/
│   └── quotas/                              # NEW FOLDER
│       ├── quota-management.md              # NEW - Managing quotas via UI
│       ├── quota-dashboards.md              # NEW - Using quota dashboards
│       └── configuring-quotas.md            # MOVE & UPDATE from parent
├── reference/
│   └── concepts/
│       ├── quotas.md                        # UPDATE - Add UI references
│       └── api-limits.md                    # UPDATE - Cross-references
└── toc.yml                                  # UPDATE - Add Quotas section
```

### 8.2 Files to Create

#### 8.2.1 `docs/docs/management-portal/how-to-guides/quotas/quota-management.md`

**Content outline:**
- Overview of Quota Management page
- Accessing Quota Management
- Viewing Existing Quotas (table columns description)
- Creating a New Quota (form fields, validation, context format)
- Editing a Quota
- Deleting a Quota
- Best Practices (naming conventions, setting limits, partition selection)
- Related Topics links

#### 8.2.2 `docs/docs/management-portal/how-to-guides/quotas/quota-dashboards.md`

**Content outline:**
- Overview of Quota Dashboards
- Accessing Quota Dashboards
- Dashboard Components (summary cards, real-time table, historical charts)
- Usage Colors explanation
- Filtering Options (time range, quota, partition)
- Use Cases (monitoring production, investigating issues, capacity planning)
- Auto-Refresh functionality
- Exporting Data
- Related Topics links

### 8.3 Files to Update

#### 8.3.1 Update `docs/docs/management-portal/toc.yml`

Add new Quotas section after Security:

```yaml
- name: Quotas
  href: how-to-guides/quotas/quota-management.md
  items:
  - name: Quota Management
    href: how-to-guides/quotas/quota-management.md
  - name: Quota Dashboards
    href: how-to-guides/quotas/quota-dashboards.md
```

Remove the standalone "Configuring Quotas" entry as it will be consolidated.

#### 8.3.2 Update `docs/docs/management-portal/reference/concepts/quotas.md`

Add UI references:
- Section on managing quotas via Management Portal
- Section on monitoring quota usage via dashboards
- Update Related Topics with new links

#### 8.3.3 Update `docs/docs/management-portal/how-to-guides/configuring-quotas.md`

Replace TODO placeholders with actual content:
- Via Management Portal (Recommended) section
- Agent-Level Quotas configuration
- Quota monitoring in Management Portal section

#### 8.3.4 Update `docs/docs/management-portal/reference/concepts/api-limits.md`

Add cross-references:
- Configuring via Management Portal section
- Monitoring Quota Usage section

#### 8.3.5 Update `docs/docs/management-portal/index.md`

Add Quotas to the feature list:
- Quota Management description
- Quota Dashboards description

### 8.4 Screenshots to Add

Create screenshots for the following (store in `docs/docs/management-portal/media/quotas/`):

| Screenshot | Filename | Description |
|------------|----------|-------------|
| Quota list page | `quota-list.png` | Table showing all quotas |
| Create quota form | `quota-create-form.png` | Empty create form |
| Create quota - filled | `quota-create-filled.png` | Form with example values |
| Edit quota form | `quota-edit-form.png` | Edit form with existing values |
| Delete confirmation | `quota-delete-confirm.png` | Delete confirmation dialog |
| Dashboard overview | `dashboard-overview.png` | Full dashboard view |
| Dashboard cards | `dashboard-summary-cards.png` | Summary cards close-up |
| Usage table | `dashboard-usage-table.png` | Real-time usage table |
| Usage chart | `dashboard-usage-chart.png` | Historical usage chart |
| Filter options | `dashboard-filters.png` | Filter dropdown options |

### 8.5 Documentation Checklist

Before completing the documentation phase:

- [ ] All new markdown files created
- [ ] TOC updated with new entries
- [ ] Existing files updated with cross-references
- [ ] All TODO placeholders replaced
- [ ] Screenshots captured and added
- [ ] Links tested and working
- [ ] Spelling and grammar checked
- [ ] Follows existing documentation style
- [ ] Code examples are accurate
- [ ] Tables are properly formatted

### 8.6 Documentation Review Process

1. **Self-review** — Author reviews all changes
2. **Technical review** — Developer verifies accuracy
3. **Editorial review** — Check style and clarity
4. **Build verification** — Ensure docs site builds without errors

---

## Complete Phase Summary

| Phase | Description | Effort |
|-------|-------------|--------|
| **Phase 1** | Backend - Quota Resource Provider | 3-5 days |
| **Phase 2** | Frontend - Management Portal UI | 5-7 days |
| **Phase 3** | New Vue Components | 2-3 days |
| **Phase 4** | Backend Metrics Collection | 2-3 days |
| **Phase 5** | File Changes Implementation | (included above) |
| **Phase 6** | Implementation Order | (schedule) |
| **Phase 7** | Local Development & Testing | 1-2 days |
| **Phase 8** | Documentation Updates | 2-3 days |
| **Total** | | **17-23 days** |

---

## Appendix A: Existing Quota System Reference

### Current Quota Types

| Type | Description |
|------|-------------|
| `RawRequestRateLimit` | Limits total API requests to a controller |
| `AgentRequestRateLimit` | Limits completion requests to specific agents |

### Current Partition Types

| Partition | Description |
|-----------|-------------|
| `None` | Global limit for all users combined |
| `UserPrincipalName` | Per-user limits by Azure AD UPN |
| `UserIdentifier` | Per-user by internal user ID |

### Current Storage Location

- **Container:** `quota`
- **File:** `/quota-store.json`
- **Storage Account:** Main FoundationaLLM storage account

### Supported Controllers

| Controller | Context |
|------------|---------|
| `Completions` | `CoreAPI:Completions` |
| `CompletionsStatus` | `CoreAPI:CompletionsStatus` |
| `Sessions` | `CoreAPI:Sessions` |
| `Files` | `CoreAPI:Files` |
| `Branding` | `CoreAPI:Branding` |
| `Configuration` | `CoreAPI:Configuration` |
| `UserProfiles` | `CoreAPI:UserProfiles` |
| `Status` | `CoreAPI:Status` |
| `OneDriveWorkSchool` | `CoreAPI:OneDriveWorkSchool` |

---

## Appendix B: Related Code Locations

### Backend

| Component | Location |
|-----------|----------|
| Quota Service | `src/dotnet/Common/Services/Quota/QuotaService.cs` |
| Quota Definition Model | `src/dotnet/Common/Models/Quota/QuotaDefinition.cs` |
| Quota Middleware | `src/dotnet/Common/Middleware/QuotaMiddleware.cs` |
| Quota Interface | `src/dotnet/Common/Interfaces/IQuotaService.cs` |
| Resource Provider Names | `src/dotnet/Common/Constants/ResourceProviders/ResourceProviderNames.cs` |

### Frontend

| Component | Location |
|-----------|----------|
| API Client | `src/ui/ManagementPortal/js/api.ts` |
| Type Definitions | `src/ui/ManagementPortal/js/types.ts` |
| Sidebar Navigation | `src/ui/ManagementPortal/components/Sidebar.vue` |

### Documentation

| Document | Location |
|----------|----------|
| Quotas Reference | `docs/docs/management-portal/reference/concepts/quotas.md` |
| API Limits | `docs/docs/management-portal/reference/concepts/api-limits.md` |
| Configuring Quotas | `docs/docs/management-portal/how-to-guides/configuring-quotas.md` |
