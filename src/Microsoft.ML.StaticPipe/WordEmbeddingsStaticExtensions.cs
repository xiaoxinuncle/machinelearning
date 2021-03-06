﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Microsoft.ML.Transforms.Text;

namespace Microsoft.ML.StaticPipe
{
    public static class WordEmbeddingsStaticExtensions
    {
        /// <include file='../Microsoft.ML.Transforms/Text/doc.xml' path='doc/members/member[@name="WordEmbeddings"]/*' />
        /// <param name="input">Vector of tokenized text.</param>
        /// <param name="modelKind">The pretrained word embedding model.</param>
        /// <returns></returns>
        public static Vector<float> WordEmbeddings(this VarVector<string> input, WordEmbeddingsExtractingEstimator.PretrainedModelKind modelKind = WordEmbeddingsExtractingEstimator.PretrainedModelKind.Sswe)
        {
            Contracts.CheckValue(input, nameof(input));
            return new OutColumn(input, modelKind);
        }

        /// <include file='../Microsoft.ML.Transforms/Text/doc.xml' path='doc/members/member[@name="WordEmbeddings"]/*' />
        /// <param name="input">Vector of tokenized text.</param>
        /// <param name="customModelFile">The custom word embedding model file.</param>
        public static Vector<float> WordEmbeddings(this VarVector<string> input, string customModelFile)
        {
            Contracts.CheckValue(input, nameof(input));
            return new OutColumn(input, customModelFile);
        }

        private sealed class OutColumn : Vector<float>
        {
            public PipelineColumn Input { get; }

            public OutColumn(VarVector<string> input, WordEmbeddingsExtractingEstimator.PretrainedModelKind modelKind = WordEmbeddingsExtractingEstimator.PretrainedModelKind.Sswe)
                : base(new Reconciler(modelKind), input)
            {
                Input = input;
            }

            public OutColumn(VarVector<string> input, string customModelFile = null)
                : base(new Reconciler(customModelFile), input)
            {
                Input = input;
            }
        }

        private sealed class Reconciler : EstimatorReconciler
        {
            private readonly WordEmbeddingsExtractingEstimator.PretrainedModelKind? _modelKind;
            private readonly string _customLookupTable;

            public Reconciler(WordEmbeddingsExtractingEstimator.PretrainedModelKind modelKind = WordEmbeddingsExtractingEstimator.PretrainedModelKind.Sswe)
            {
                _modelKind = modelKind;
                _customLookupTable = null;
            }

            public Reconciler(string customModelFile)
            {
                _modelKind = null;
                _customLookupTable = customModelFile;
            }

            public override IEstimator<ITransformer> Reconcile(IHostEnvironment env,
                PipelineColumn[] toOutput,
                IReadOnlyDictionary<PipelineColumn, string> inputNames,
                IReadOnlyDictionary<PipelineColumn, string> outputNames,
                IReadOnlyCollection<string> usedNames)
            {
                Contracts.Assert(toOutput.Length == 1);

                var cols = new WordEmbeddingsExtractingEstimator.ColumnInfo[toOutput.Length];
                for (int i = 0; i < toOutput.Length; ++i)
                {
                    var outCol = (OutColumn)toOutput[i];
                    cols[i] = new WordEmbeddingsExtractingEstimator.ColumnInfo(outputNames[outCol], inputNames[outCol.Input]);
                }

                bool customLookup = !string.IsNullOrWhiteSpace(_customLookupTable);
                if (customLookup)
                    return new WordEmbeddingsExtractingEstimator(env, _customLookupTable, cols);
                else
                    return new WordEmbeddingsExtractingEstimator(env, _modelKind.Value, cols);
            }
        }
    }
}
