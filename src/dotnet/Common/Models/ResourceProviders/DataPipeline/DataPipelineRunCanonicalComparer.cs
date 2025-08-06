namespace FoundationaLLM.Common.Models.ResourceProviders.DataPipeline
{
    /// <summary>
    /// Provides a mechanism to compare two <see cref="DataPipelineRun"/> objects for canonical equivalence.
    /// </summary>
    /// <remarks>
    /// Two <see cref="DataPipelineRun"/> objects are considered canonically equivalent if they cannot be
    /// executed in parallel.
    /// </remarks>
    public class DataPipelineRunCanonicalComparer: IEqualityComparer<DataPipelineRun>
    {
        /// <inheritdoc/>
        public bool Equals(DataPipelineRun? x, DataPipelineRun? y)
        {
            if (x is null && y is null) return true;
            if (x is null || y is null) return false;

            return
                x.CanonicalRunId == y.CanonicalRunId;

            //TODO: Add more elaborate checks, like:
            // - Identical VectorDatabaseObjectId + VectorStoreId combination
            // - Identical KnowledgeSourceId
        }

        /// <inheritdoc/>
        public int GetHashCode(DataPipelineRun obj) =>
            HashCode.Combine(
                obj.CanonicalRunId);
    }
}
