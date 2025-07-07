namespace FoundationaLLM.Context.Models
{
    /// <summary>
    /// Equality comparer for KnowledgeGraphIndexRelatedNode based on the UniqueId of RelatedEntity.
    /// </summary>
    public class KnowledgeGraphIndexRelatedNodeComparer : IEqualityComparer<KnowledgeGraphIndexRelatedNode>
    {
        public bool Equals(KnowledgeGraphIndexRelatedNode? x, KnowledgeGraphIndexRelatedNode? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null
                || x.RelatedEntity is null
                || y is null
                || y.RelatedEntity is null)
                return false;
            return x.RelatedEntity.UniqueId == y.RelatedEntity.UniqueId;
        }

        public int GetHashCode(KnowledgeGraphIndexRelatedNode obj) =>
            obj.RelatedEntity?.UniqueId?.GetHashCode() ?? 0;
    }
}
