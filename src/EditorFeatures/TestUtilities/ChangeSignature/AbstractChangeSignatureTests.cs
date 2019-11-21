// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.ChangeSignature;
using Microsoft.CodeAnalysis.Editor.UnitTests.CodeActions;
using Microsoft.CodeAnalysis.Notification;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.ChangeSignature
{
    public abstract class AbstractChangeSignatureTests : AbstractCodeActionTest
    {
        protected override ParseOptions GetScriptOptions()
        {
            throw new NotSupportedException();
        }

        private string CreateDiagnosticsString(ImmutableArray<Diagnostic> diagnostics, int[] permutation, int? totalParameters, string fileContents)
        {
            if (diagnostics.Length == 0)
            {
                return string.Empty;
            }

            return string.Format("{0} diagnostic(s) introduced in signature configuration \"{1}\":\r\n{2}\r\n{3}",
                diagnostics.Length,
                GetSignatureDescriptionString(permutation, totalParameters),
                string.Join("\r\n", diagnostics.Select(d => d.GetMessage())),
                fileContents);
        }

        private string GetSignatureDescriptionString(int[] signature, int? totalParameters)
        {
            var removeDescription = string.Empty;
            if (totalParameters.HasValue)
            {
                var removed = new List<int>();
                for (var i = 0; i < totalParameters; i++)
                {
                    if (!signature.Contains(i))
                    {
                        removed.Add(i);
                    }
                }

                removeDescription = removed.Any() ? string.Format(", Removed: {{{0}}}", string.Join(", ", removed)) : string.Empty;
            }

            return string.Format("Parameters: <{0}>{1}", string.Join(", ", signature), removeDescription);
        }

        private IEnumerable<int[]> GetAllSignatureSpecifications(int[] signaturePartCounts)
        {
            var regularParameterStartIndex = signaturePartCounts[0];
            var defaultValueParameterStartIndex = signaturePartCounts[0] + signaturePartCounts[1];
            var paramParameterIndex = signaturePartCounts[0] + signaturePartCounts[1] + signaturePartCounts[2];

            var regularParameterArrangements = GetPermutedSubsets(regularParameterStartIndex, signaturePartCounts[1]);
            var defaultValueParameterArrangements = GetPermutedSubsets(defaultValueParameterStartIndex, signaturePartCounts[2]);

            var startArray = signaturePartCounts[0] == 0 ? Array.Empty<int>() : new[] { 0 };

            foreach (var regularParameterPart in regularParameterArrangements)
            {
                foreach (var defaultValueParameterPart in defaultValueParameterArrangements)
                {
                    var p1 = startArray.Concat(regularParameterPart).Concat(defaultValueParameterPart);
                    yield return p1.ToArray();

                    if (signaturePartCounts[3] == 1)
                    {
                        yield return p1.Concat(new[] { paramParameterIndex }).ToArray();
                    }
                }
            }
        }

        private IEnumerable<IEnumerable<int>> GetPermutedSubsets(int startIndex, int count)
        {
            foreach (var subset in GetSubsets(Enumerable.Range(startIndex, count)))
            {
                foreach (var permutation in GetPermutations(subset))
                {
                    yield return permutation;
                }
            }
        }

        private IEnumerable<IEnumerable<int>> GetPermutations(IEnumerable<int> list)
        {
            if (!list.Any())
            {
                yield return SpecializedCollections.EmptyEnumerable<int>();
                yield break;
            }

            var index = 0;
            foreach (var element in list)
            {
                var permutationsWithoutElement = GetPermutations(GetListWithoutElementAtIndex(list, index));
                foreach (var perm in permutationsWithoutElement)
                {
                    yield return perm.Concat(element);
                }

                index++;
            }
        }

        private IEnumerable<int> GetListWithoutElementAtIndex(IEnumerable<int> list, int skippedIndex)
        {
            var index = 0;
            foreach (var x in list)
            {
                if (index != skippedIndex)
                {
                    yield return x;
                }

                index++;
            }
        }

        private IEnumerable<IEnumerable<int>> GetSubsets(IEnumerable<int> list)
        {
            if (!list.Any())
            {
                return SpecializedCollections.SingletonEnumerable(SpecializedCollections.EmptyEnumerable<int>());
            }

            var firstElement = list.Take(1);

            var withoutFirstElement = GetSubsets(list.Skip(1));
            var withFirstElement = withoutFirstElement.Select(without => firstElement.Concat(without));

            return withoutFirstElement.Concat(withFirstElement);
        }
    }
}
