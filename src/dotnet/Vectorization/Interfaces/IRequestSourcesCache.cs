using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Interfaces
{
    /// <summary>
    /// Defines a cache of <see cref="IRequestSourceService"/> objects.
    /// </summary>
    public interface IRequestSourcesCache
    {
        /// <summary>
        /// Gets the dictionary of <see cref="IRequestSourceService"/> objects hashed by request source names.
        /// </summary>
        Dictionary<string, IRequestSourceService> RequestSources { get; }
    }
}
