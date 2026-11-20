using DomainQueryEngine.Core;

namespace DomainQueryEngine.SearchUI.Models
{
    public sealed class ResultRowViewModel
    {
        public ComparedResult Item { get; init; } = new();
        public bool IsBest { get; init; }
        /// <summary>The price is far above the other providers' (more than the median times the outlier factor).</summary>
        public bool IsOutlier { get; init; }
    }
}
