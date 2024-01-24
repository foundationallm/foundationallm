﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Models
{
    /// <summary>
    /// Deifines types of vectorization artifacts.
    /// </summary>
    public enum VectorizationArtifactType
    {
        /// <summary>
        /// Text extracted from source content.
        /// </summary>
        ExtractedText,
        /// <summary>
        /// Text partition suitable for embedding.
        /// </summary>
        TextPartition,
        /// <summary>
        /// Vector embedding derived from a text partition.
        /// </summary>
        TextEmbeddingVector
    }
}
