// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Editor.Implementation.IntelliSense.SignatureHelp.Presentation;
using Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces;
using Microsoft.CodeAnalysis.LanguageServices;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.SignatureHelp;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Test.Utilities;
using Roslyn.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.SignatureHelp
{
    [UseExportProvider]
    public abstract class AbstractSignatureHelpProviderTests<TWorkspaceFixture> : TestBase, IClassFixture<TWorkspaceFixture>
        where TWorkspaceFixture : TestWorkspaceFixture, new()
    {
        protected TWorkspaceFixture workspaceFixture;

        internal abstract ISignatureHelpProvider CreateSignatureHelpProvider();

        protected AbstractSignatureHelpProviderTests(TWorkspaceFixture workspaceFixture)
        {
            this.workspaceFixture = workspaceFixture;
        }

        public override void Dispose()
        {
            this.workspaceFixture.DisposeAfterTest();
            base.Dispose();
        }

        /// <summary>
        /// Verifies that sighelp comes up at the indicated location in markup ($$), with the indicated span [| ... |].
        /// </summary>
        /// <param name="markup">Input markup with $$ denoting the cursor position, and [| ... |]
        /// denoting the expected sighelp span</param>
        /// <param name="expectedOrderedItemsOrNull">The exact expected sighelp items list. If null, this part of the test is ignored.</param>
        /// <param name="usePreviousCharAsTrigger">If true, uses the last character before $$ to trigger sighelp.
        /// If false, invokes sighelp explicitly at the cursor location.</param>
        /// <param name="sourceCodeKind">The sourcecodekind to run this test on. If null, runs on both regular and script sources.</param>
        protected virtual async Task TestAsync(
            string markup,
            IEnumerable<SignatureHelpTestItem> expectedOrderedItemsOrNull = null,
            bool usePreviousCharAsTrigger = false,
            SourceCodeKind? sourceCodeKind = null,
            bool experimental = false)
        {
            if (sourceCodeKind.HasValue)
            {
                await TestSignatureHelpWorkerAsync(markup, sourceCodeKind.Value, experimental, expectedOrderedItemsOrNull, usePreviousCharAsTrigger);
            }
            else
            {
                await TestSignatureHelpWorkerAsync(markup, SourceCodeKind.Regular, experimental, expectedOrderedItemsOrNull, usePreviousCharAsTrigger);
                await TestSignatureHelpWorkerAsync(markup, SourceCodeKind.Script, experimental, expectedOrderedItemsOrNull, usePreviousCharAsTrigger);
            }
        }

        private async Task TestSignatureHelpWorkerAsync(
            string markupWithPositionAndOptSpan,
            SourceCodeKind sourceCodeKind,
            bool experimental,
            IEnumerable<SignatureHelpTestItem> expectedOrderedItemsOrNull = null,
            bool usePreviousCharAsTrigger = false)
        {
            markupWithPositionAndOptSpan = markupWithPositionAndOptSpan.NormalizeLineEndings();

            TextSpan? textSpan = null;
            MarkupTestFile.GetPositionAndSpans(
                markupWithPositionAndOptSpan,
                out var code,
                out var cursorPosition,
                out ImmutableArray<TextSpan> textSpans);

            if (textSpans.Any())
            {
                textSpan = textSpans.First();
            }

            var parseOptions = CreateExperimentalParseOptions();

            // regular
            var document1 = workspaceFixture.UpdateDocument(code, sourceCodeKind);
            if (experimental)
            {
                document1 = document1.Project.WithParseOptions(parseOptions).GetDocument(document1.Id);
            }

            await TestSignatureHelpWorkerSharedAsync(code, cursorPosition, sourceCodeKind, document1, textSpan, expectedOrderedItemsOrNull, usePreviousCharAsTrigger);

            // speculative semantic model
            if (await CanUseSpeculativeSemanticModelAsync(document1, cursorPosition))
            {
                var document2 = workspaceFixture.UpdateDocument(code, sourceCodeKind, cleanBeforeUpdate: false);
                if (experimental)
                {
                    document2 = document2.Project.WithParseOptions(parseOptions).GetDocument(document2.Id);
                }

                await TestSignatureHelpWorkerSharedAsync(code, cursorPosition, sourceCodeKind, document2, textSpan, expectedOrderedItemsOrNull, usePreviousCharAsTrigger);
            }
        }

        protected abstract ParseOptions CreateExperimentalParseOptions();

        private static async Task<bool> CanUseSpeculativeSemanticModelAsync(Document document, int position)
        {
            var service = document.GetLanguageService<ISyntaxFactsService>();
            var node = (await document.GetSyntaxRootAsync()).FindToken(position).Parent;

            return !service.GetMemberBodySpanForSpeculativeBinding(node).IsEmpty;
        }

        protected virtual void VerifyTriggerCharacters(char[] expectedTriggerCharacters, char[] unexpectedTriggerCharacters, SourceCodeKind? sourceCodeKind = null)
        {
            if (sourceCodeKind.HasValue)
            {
                VerifyTriggerCharactersWorker(expectedTriggerCharacters, unexpectedTriggerCharacters, sourceCodeKind.Value);
            }
            else
            {
                VerifyTriggerCharactersWorker(expectedTriggerCharacters, unexpectedTriggerCharacters, SourceCodeKind.Regular);
                VerifyTriggerCharactersWorker(expectedTriggerCharacters, unexpectedTriggerCharacters, SourceCodeKind.Script);
            }
        }

        private void VerifyTriggerCharactersWorker(char[] expectedTriggerCharacters, char[] unexpectedTriggerCharacters, SourceCodeKind sourceCodeKind)
        {
            var signatureHelpProvider = CreateSignatureHelpProvider();

            foreach (var expectedTriggerCharacter in expectedTriggerCharacters)
            {
                Assert.True(signatureHelpProvider.IsTriggerCharacter(expectedTriggerCharacter), "Expected '" + expectedTriggerCharacter + "' to be a trigger character");
            }

            foreach (var unexpectedTriggerCharacter in unexpectedTriggerCharacters)
            {
                Assert.False(signatureHelpProvider.IsTriggerCharacter(unexpectedTriggerCharacter), "Expected '" + unexpectedTriggerCharacter + "' to NOT be a trigger character");
            }
        }

        protected virtual async Task VerifyCurrentParameterNameAsync(string markup, string expectedParameterName, SourceCodeKind? sourceCodeKind = null)
        {
            if (sourceCodeKind.HasValue)
            {
                await VerifyCurrentParameterNameWorkerAsync(markup, expectedParameterName, sourceCodeKind.Value);
            }
            else
            {
                await VerifyCurrentParameterNameWorkerAsync(markup, expectedParameterName, SourceCodeKind.Regular);
                await VerifyCurrentParameterNameWorkerAsync(markup, expectedParameterName, SourceCodeKind.Script);
            }
        }

        private static async Task<SignatureHelpState> GetArgumentStateAsync(int cursorPosition, Document document, ISignatureHelpProvider signatureHelpProvider, SignatureHelpTriggerInfo triggerInfo)
        {
            var items = await signatureHelpProvider.GetItemsAsync(document, cursorPosition, triggerInfo, CancellationToken.None);
            return items == null ? null : new SignatureHelpState(items.ArgumentIndex, items.ArgumentCount, items.ArgumentName, null);
        }

        private async Task VerifyCurrentParameterNameWorkerAsync(string markup, string expectedParameterName, SourceCodeKind sourceCodeKind)
        {
            MarkupTestFile.GetPosition(markup.NormalizeLineEndings(), out var code, out int cursorPosition);

            var document = workspaceFixture.UpdateDocument(code, sourceCodeKind);

            var signatureHelpProvider = CreateSignatureHelpProvider();
            var triggerInfo = new SignatureHelpTriggerInfo(SignatureHelpTriggerReason.InvokeSignatureHelpCommand);
            var items = await signatureHelpProvider.GetItemsAsync(document, cursorPosition, triggerInfo, CancellationToken.None);
            Assert.Equal(expectedParameterName, (await GetArgumentStateAsync(cursorPosition, document, signatureHelpProvider, triggerInfo)).ArgumentName);
        }

        private void CompareAndAssertCollectionsAndCurrentParameter(
            IEnumerable<SignatureHelpTestItem> expectedTestItems, SignatureHelpItems actualSignatureHelpItems)
        {
            Assert.Equal(expectedTestItems.Count(), actualSignatureHelpItems.Items.Count());

            for (var i = 0; i < expectedTestItems.Count(); i++)
            {
                CompareSigHelpItemsAndCurrentPosition(
                    actualSignatureHelpItems,
                    actualSignatureHelpItems.Items.ElementAt(i),
                    expectedTestItems.ElementAt(i));
            }
        }

        private void CompareSigHelpItemsAndCurrentPosition(
            SignatureHelpItems items,
            SignatureHelpItem actualSignatureHelpItem,
            SignatureHelpTestItem expectedTestItem)
        {
            var currentParameterIndex = -1;
            if (expectedTestItem.CurrentParameterIndex != null)
            {
                if (expectedTestItem.CurrentParameterIndex.Value >= 0 && expectedTestItem.CurrentParameterIndex.Value < actualSignatureHelpItem.Parameters.Length)
                {
                    currentParameterIndex = expectedTestItem.CurrentParameterIndex.Value;
                }
            }

            var signature = new Signature(applicableToSpan: null, signatureHelpItem: actualSignatureHelpItem, selectedParameterIndex: currentParameterIndex);

            // We're a match if the signature matches...
            // We're now combining the signature and documentation to make classification work.
            if (!string.IsNullOrEmpty(expectedTestItem.MethodDocumentation))
            {
                Assert.Equal(expectedTestItem.Signature + "\r\n" + expectedTestItem.MethodDocumentation, signature.Content);
            }
            else
            {
                Assert.Equal(expectedTestItem.Signature, signature.Content);
            }

            if (expectedTestItem.PrettyPrintedSignature != null)
            {
                Assert.Equal(expectedTestItem.PrettyPrintedSignature, signature.PrettyPrintedContent);
            }

            if (expectedTestItem.MethodDocumentation != null)
            {
                Assert.Equal(expectedTestItem.MethodDocumentation, actualSignatureHelpItem.DocumentationFactory(CancellationToken.None).GetFullText());
            }

            if (expectedTestItem.ParameterDocumentation != null)
            {
                Assert.Equal(expectedTestItem.ParameterDocumentation, signature.CurrentParameter.Documentation);
            }

            if (expectedTestItem.CurrentParameterIndex != null)
            {
                Assert.Equal(expectedTestItem.CurrentParameterIndex, items.ArgumentIndex);
            }

            if (expectedTestItem.Description != null)
            {
                Assert.Equal(expectedTestItem.Description, ToString(actualSignatureHelpItem.DescriptionParts));
            }
        }

        private string ToString(IEnumerable<TaggedText> list)
        {
            return string.Concat(list.Select(i => i.ToString()));
        }

        private async Task TestSignatureHelpWorkerSharedAsync(
            string code,
            int cursorPosition,
            SourceCodeKind sourceCodeKind,
            Document document,
            TextSpan? textSpan,
            IEnumerable<SignatureHelpTestItem> expectedOrderedItemsOrNull = null,
            bool usePreviousCharAsTrigger = false)
        {
            var signatureHelpProvider = CreateSignatureHelpProvider();
            var triggerInfo = new SignatureHelpTriggerInfo(SignatureHelpTriggerReason.InvokeSignatureHelpCommand);

            if (usePreviousCharAsTrigger)
            {
                triggerInfo = new SignatureHelpTriggerInfo(
                    SignatureHelpTriggerReason.TypeCharCommand,
                    code.ElementAt(cursorPosition - 1));

                if (!signatureHelpProvider.IsTriggerCharacter(triggerInfo.TriggerCharacter.Value))
                {
                    return;
                }
            }

            var items = await signatureHelpProvider.GetItemsAsync(document, cursorPosition, triggerInfo, CancellationToken.None);

            // If we're expecting 0 items, then there's no need to compare them
            if ((expectedOrderedItemsOrNull == null || !expectedOrderedItemsOrNull.Any()) && items == null)
            {
                return;
            }

            AssertEx.NotNull(items, "Signature help provider returned null for items. Did you forget $$ in the test or is the test otherwise malformed, e.g. quotes not escaped?");

            // Verify the span
            if (textSpan != null)
            {
                Assert.Equal(textSpan, items.ApplicableSpan);
            }

            if (expectedOrderedItemsOrNull != null)
            {
                CompareAndAssertCollectionsAndCurrentParameter(expectedOrderedItemsOrNull, items);
                CompareSelectedIndex(expectedOrderedItemsOrNull, items.SelectedItemIndex);
            }
        }

        private void CompareSelectedIndex(IEnumerable<SignatureHelpTestItem> expectedOrderedItemsOrNull, int? selectedItemIndex)
        {
            if (expectedOrderedItemsOrNull == null ||
                !expectedOrderedItemsOrNull.Any(i => i.IsSelected))
            {
                return;
            }

            Assert.True(expectedOrderedItemsOrNull.Count(i => i.IsSelected) == 1, "Only one expected item can be marked with 'IsSelected'");
            Assert.True(selectedItemIndex != null, "Expected an item to be selected, but no item was actually selected");

            var counter = 0;
            foreach (var item in expectedOrderedItemsOrNull)
            {
                if (item.IsSelected)
                {
                    Assert.True(selectedItemIndex == counter,
                        $"Expected item with index {counter} to be selected, but the actual selected index is {selectedItemIndex}.");
                }
                else
                {
                    Assert.True(selectedItemIndex != counter,
                        $"Found unexpected selected item. Actual selected index is {selectedItemIndex}.");
                }

                counter++;
            }
        }
    }
}
