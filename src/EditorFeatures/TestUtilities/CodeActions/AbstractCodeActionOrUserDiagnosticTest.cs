// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes.Suppression;
using Microsoft.CodeAnalysis.CodeStyle;
using Microsoft.CodeAnalysis.Editor.Implementation.Preview;
using Microsoft.CodeAnalysis.Editor.UnitTests.Extensions;
using Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Remote;
using Microsoft.CodeAnalysis.Shared.Utilities;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.UnitTests;
using Roslyn.Test.Utilities;
using Roslyn.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.CodeActions
{
    [UseExportProvider]
    public abstract class AbstractCodeActionOrUserDiagnosticTest
    {
        public struct TestParameters
        {
            internal readonly IDictionary<OptionKey, object> options;
            internal readonly object fixProviderData;
            internal readonly ParseOptions parseOptions;
            internal readonly CompilationOptions compilationOptions;
            internal readonly int index;
            internal readonly CodeActionPriority? priority;
            internal readonly bool retainNonFixableDiagnostics;
            internal readonly string title;

            internal TestParameters(
                ParseOptions parseOptions = null,
                CompilationOptions compilationOptions = null,
                IDictionary<OptionKey, object> options = null,
                object fixProviderData = null,
                int index = 0,
                CodeActionPriority? priority = null,
                bool retainNonFixableDiagnostics = false,
                string title = null)
            {
                this.parseOptions = parseOptions;
                this.compilationOptions = compilationOptions;
                this.options = options;
                this.fixProviderData = fixProviderData;
                this.index = index;
                this.priority = priority;
                this.retainNonFixableDiagnostics = retainNonFixableDiagnostics;
                this.title = title;
            }

            public TestParameters WithParseOptions(ParseOptions parseOptions)
                => new TestParameters(parseOptions, compilationOptions, options, fixProviderData, index, priority, title: title);

            public TestParameters WithOptions(IDictionary<OptionKey, object> options)
                => new TestParameters(parseOptions, compilationOptions, options, fixProviderData, index, priority, title: title);

            public TestParameters WithFixProviderData(object fixProviderData)
                => new TestParameters(parseOptions, compilationOptions, options, fixProviderData, index, priority, title: title);

            public TestParameters WithIndex(int index)
                => new TestParameters(parseOptions, compilationOptions, options, fixProviderData, index, priority, title: title);

            public TestParameters WithRetainNonFixableDiagnostics(bool retainNonFixableDiagnostics)
                => new TestParameters(parseOptions, compilationOptions, options, fixProviderData, index, priority, title: title, retainNonFixableDiagnostics: retainNonFixableDiagnostics);
        }

        protected abstract string GetLanguage();
        protected abstract ParseOptions GetScriptOptions();

        protected TestWorkspace CreateWorkspaceFromOptions(
            string initialMarkup, TestParameters parameters)
        {
            var workspace = CreateWorkspaceFromFile(initialMarkup, parameters);

            workspace.ApplyOptions(parameters.options);

            return workspace;
        }

        protected abstract TestWorkspace CreateWorkspaceFromFile(string initialMarkup, TestParameters parameters);

        private TestParameters WithRegularOptions(TestParameters parameters)
            => parameters.WithParseOptions(parameters.parseOptions?.WithKind(SourceCodeKind.Regular));

        private TestParameters WithScriptOptions(TestParameters parameters)
            => parameters.WithParseOptions(parameters.parseOptions?.WithKind(SourceCodeKind.Script) ?? GetScriptOptions());

        protected async Task TestMissingInRegularAndScriptAsync(
            string initialMarkup,
            TestParameters parameters = default)
        {
            await TestMissingAsync(initialMarkup, WithRegularOptions(parameters));
            await TestMissingAsync(initialMarkup, WithScriptOptions(parameters));
        }

        protected async Task TestMissingAsync(
            string initialMarkup,
            TestParameters parameters = default)
        {
            using (var workspace = CreateWorkspaceFromOptions(initialMarkup, parameters))
            {
                var (actions, _) = await GetCodeActionsAsync(workspace, parameters);
                Assert.True(actions.Length == 0, "An action was offered when none was expected");
            }
        }

        protected async Task TestDiagnosticMissingAsync(
            string initialMarkup, TestParameters parameters = default)
        {
            using (var workspace = CreateWorkspaceFromOptions(initialMarkup, parameters))
            {
                var diagnostics = await GetDiagnosticsWorkerAsync(workspace, parameters);
                Assert.True(0 == diagnostics.Length, $"Expected no diagnostics, but got {diagnostics.Length}");
            }
        }

        protected abstract Task<(ImmutableArray<CodeAction>, CodeAction actionToInvoke)> GetCodeActionsAsync(
            TestWorkspace workspace, TestParameters parameters);

        protected abstract Task<ImmutableArray<Diagnostic>> GetDiagnosticsWorkerAsync(
            TestWorkspace workspace, TestParameters parameters);

        protected Task TestSmartTagTextAsync(string initialMarkup, string displayText, int index)
            => TestSmartTagTextAsync(initialMarkup, displayText, new TestParameters(index: index));

        protected Task TestSmartTagGlyphTagsAsync(string initialMarkup, ImmutableArray<string> glyphTags, int index)
            => TestSmartTagGlyphTagsAsync(initialMarkup, glyphTags, new TestParameters(index: index));

        protected async Task TestSmartTagTextAsync(
            string initialMarkup,
            string displayText,
            TestParameters parameters = default)
        {
            using (var workspace = CreateWorkspaceFromOptions(initialMarkup, parameters))
            {
                var (_, action) = await GetCodeActionsAsync(workspace, parameters);
                Assert.Equal(displayText, action.Title);
            }
        }

        protected async Task TestSmartTagGlyphTagsAsync(
            string initialMarkup,
            ImmutableArray<string> glyph,
            TestParameters parameters = default)
        {
            using (var workspace = CreateWorkspaceFromOptions(initialMarkup, parameters))
            {
                var (_, action) = await GetCodeActionsAsync(workspace, parameters);
                Assert.Equal(glyph, action.Tags);
            }
        }

        protected async Task TestExactActionSetOfferedAsync(
            string initialMarkup,
            IEnumerable<string> expectedActionSet,
            TestParameters parameters = default)
        {
            using (var workspace = CreateWorkspaceFromOptions(initialMarkup, parameters))
            {
                var (actions, _) = await GetCodeActionsAsync(workspace, parameters);

                var actualActionSet = actions.Select(a => a.Title);
                Assert.True(actualActionSet.SequenceEqual(expectedActionSet),
                    "Expected: " + string.Join(", ", expectedActionSet) +
                    "\nActual: " + string.Join(", ", actualActionSet));
            }
        }

        protected async Task TestActionCountAsync(
            string initialMarkup,
            int count,
            TestParameters parameters = default)
        {
            using (var workspace = CreateWorkspaceFromOptions(initialMarkup, parameters))
            {
                var (actions, _) = await GetCodeActionsAsync(workspace, parameters);

                Assert.Equal(count, actions.Length);
            }
        }

        protected async Task TestAddDocumentInRegularAndScriptAsync(
            string initialMarkup, string expectedMarkup,
            ImmutableArray<string> expectedContainers,
            string expectedDocumentName,
            TestParameters parameters = default)
        {
            await TestAddDocument(
                initialMarkup, expectedMarkup,
                expectedContainers, expectedDocumentName,
                WithRegularOptions(parameters));
            await TestAddDocument(
                initialMarkup, expectedMarkup,
                expectedContainers, expectedDocumentName,
                WithScriptOptions(parameters));
        }

        protected async Task<Tuple<Solution, Solution>> TestAddDocumentAsync(
            TestParameters parameters,
            TestWorkspace workspace,
            string expectedMarkup,
            string expectedDocumentName,
            ImmutableArray<string> expectedContainers)
        {
            var (_, action) = await GetCodeActionsAsync(workspace, parameters);
            return await TestAddDocument(
                workspace, expectedMarkup, expectedContainers,
                expectedDocumentName, action);
        }

        protected async Task TestAddDocument(
            string initialMarkup,
            string expectedMarkup,
            ImmutableArray<string> expectedContainers,
            string expectedDocumentName,
            TestParameters parameters = default)
        {
            using (var workspace = CreateWorkspaceFromOptions(initialMarkup, parameters))
            {
                var (_, action) = await GetCodeActionsAsync(workspace, parameters);
                await TestAddDocument(
                    workspace, expectedMarkup, expectedContainers,
                    expectedDocumentName, action);
            }
        }

        private async Task<Tuple<Solution, Solution>> TestAddDocument(
            TestWorkspace workspace,
            string expectedMarkup,
            ImmutableArray<string> expectedFolders,
            string expectedDocumentName,
            CodeAction action)
        {
            var operations = await VerifyActionAndGetOperationsAsync(workspace, action, default);
            return await TestAddDocument(
                workspace,
                expectedMarkup,
                operations,
                hasProjectChange: false,
                modifiedProjectId: null,
                expectedFolders: expectedFolders,
                expectedDocumentName: expectedDocumentName);
        }

        protected async Task<Tuple<Solution, Solution>> TestAddDocument(
            TestWorkspace workspace,
            string expected,
            ImmutableArray<CodeActionOperation> operations,
            bool hasProjectChange,
            ProjectId modifiedProjectId,
            ImmutableArray<string> expectedFolders,
            string expectedDocumentName)
        {
            var appliedChanges = ApplyOperationsAndGetSolution(workspace, operations);
            var oldSolution = appliedChanges.Item1;
            var newSolution = appliedChanges.Item2;

            Document addedDocument = null;
            if (!hasProjectChange)
            {
                addedDocument = SolutionUtilities.GetSingleAddedDocument(oldSolution, newSolution);
            }
            else
            {
                Assert.NotNull(modifiedProjectId);
                addedDocument = newSolution.GetProject(modifiedProjectId).Documents.SingleOrDefault(doc => doc.Name == expectedDocumentName);
            }

            Assert.NotNull(addedDocument);

            AssertEx.Equal(expectedFolders, addedDocument.Folders);
            Assert.Equal(expectedDocumentName, addedDocument.Name);
            Assert.Equal(expected, (await addedDocument.GetTextAsync()).ToString());

            var editHandler = workspace.ExportProvider.GetExportedValue<ICodeActionEditHandlerService>();
            if (!hasProjectChange)
            {
                // If there is just one document change then we expect the preview to be a WpfTextView
                var content = (await editHandler.GetPreviews(workspace, operations, CancellationToken.None).GetPreviewsAsync())[0];
                using (var diffView = content as DifferenceViewerPreview)
                {
                    Assert.NotNull(diffView.Viewer);
                }
            }
            else
            {
                // If there are more changes than just the document we need to browse all the changes and get the document change
                var contents = editHandler.GetPreviews(workspace, operations, CancellationToken.None);
                var hasPreview = false;
                var previews = await contents.GetPreviewsAsync();
                if (previews != null)
                {
                    foreach (var preview in previews)
                    {
                        if (preview != null)
                        {
                            var diffView = preview as DifferenceViewerPreview;
                            if (diffView?.Viewer != null)
                            {
                                hasPreview = true;
                                diffView.Dispose();
                                break;
                            }
                        }
                    }
                }

                Assert.True(hasPreview);
            }

            return Tuple.Create(oldSolution, newSolution);
        }

        protected static Document GetDocumentToVerify(DocumentId expectedChangedDocumentId, Solution oldSolution, Solution newSolution)
        {
            Document document;
            // If the expectedChangedDocumentId is not mentioned then we expect only single document to be changed
            if (expectedChangedDocumentId == null)
            {
                var projectDifferences = SolutionUtilities.GetSingleChangedProjectChanges(oldSolution, newSolution);

                var documentId = projectDifferences.GetChangedDocuments().FirstOrDefault() ?? projectDifferences.GetAddedDocuments().FirstOrDefault();
                Assert.NotNull(documentId);
                document = newSolution.GetDocument(documentId);
            }
            else
            {
                // This method obtains only the document changed and does not check the project state.
                document = newSolution.GetDocument(expectedChangedDocumentId);
            }

            return document;
        }

        internal async Task<ImmutableArray<CodeActionOperation>> VerifyActionAndGetOperationsAsync(
            TestWorkspace workspace, CodeAction action, TestParameters parameters)
        {
            if (action is null)
            {
                var diagnostics = await GetDiagnosticsWorkerAsync(workspace, parameters.WithRetainNonFixableDiagnostics(true));

                throw new Exception("No action was offered when one was expected. Diagnostics from the compilation: " + string.Join("", diagnostics.Select(d => Environment.NewLine + d.ToString())));
            }

            if (parameters.priority != null)
            {
                Assert.Equal(parameters.priority.Value, action.Priority);
            }

            if (parameters.title != null)
            {
                Assert.Equal(parameters.title, action.Title);
            }

            return await action.GetOperationsAsync(CancellationToken.None);
        }

        protected Tuple<Solution, Solution> ApplyOperationsAndGetSolution(
            TestWorkspace workspace,
            IEnumerable<CodeActionOperation> operations)
        {
            Tuple<Solution, Solution> result = null;
            foreach (var operation in operations)
            {
                if (operation is ApplyChangesOperation && result == null)
                {
                    var oldSolution = workspace.CurrentSolution;
                    var newSolution = ((ApplyChangesOperation)operation).ChangedSolution;
                    result = Tuple.Create(oldSolution, newSolution);
                }
                else if (operation.ApplyDuringTests)
                {
                    var oldSolution = workspace.CurrentSolution;
                    operation.TryApply(workspace, new ProgressTracker(), CancellationToken.None);
                    var newSolution = workspace.CurrentSolution;
                    result = Tuple.Create(oldSolution, newSolution);
                }
            }

            if (result == null)
            {
                throw new InvalidOperationException("No ApplyChangesOperation found");
            }

            return result;
        }

        protected virtual ImmutableArray<CodeAction> MassageActions(ImmutableArray<CodeAction> actions)
            => actions;

        protected static ImmutableArray<CodeAction> FlattenActions(ImmutableArray<CodeAction> codeActions)
        {
            return codeActions.SelectMany(a => a.NestedCodeActions.Length > 0
                ? a.NestedCodeActions
                : ImmutableArray.Create(a)).ToImmutableArray();
        }

        protected static ImmutableArray<CodeAction> GetNestedActions(ImmutableArray<CodeAction> codeActions)
            => codeActions.SelectMany(a => a.NestedCodeActions).ToImmutableArray();

        protected (OptionKey, object) SingleOption<T>(Option<T> option, T enabled)
            => (new OptionKey(option), enabled);

        protected (OptionKey, object) SingleOption<T>(PerLanguageOption<T> option, T value)
            => (new OptionKey(option, this.GetLanguage()), value);

        protected (OptionKey, object) SingleOption<T>(Option<CodeStyleOption<T>> option, T enabled, NotificationOption notification)
            => SingleOption(option, new CodeStyleOption<T>(enabled, notification));

        protected (OptionKey, object) SingleOption<T>(Option<CodeStyleOption<T>> option, CodeStyleOption<T> codeStyle)
            => (new OptionKey(option), codeStyle);

        protected (OptionKey, object) SingleOption<T>(PerLanguageOption<CodeStyleOption<T>> option, T enabled, NotificationOption notification)
            => SingleOption(option, new CodeStyleOption<T>(enabled, notification));

        protected (OptionKey, object) SingleOption<T>(PerLanguageOption<CodeStyleOption<T>> option, CodeStyleOption<T> codeStyle)
            => SingleOption(option, codeStyle, language: GetLanguage());

        protected static (OptionKey, object) SingleOption<T>(PerLanguageOption<CodeStyleOption<T>> option, CodeStyleOption<T> codeStyle, string language)
            => (new OptionKey(option, language), codeStyle);

        protected IDictionary<OptionKey, object> Option<T>(Option<CodeStyleOption<T>> option, T enabled, NotificationOption notification)
            => OptionsSet(SingleOption(option, enabled, notification));

        protected IDictionary<OptionKey, object> Option<T>(Option<CodeStyleOption<T>> option, CodeStyleOption<T> codeStyle)
            => OptionsSet(SingleOption(option, codeStyle));

        protected IDictionary<OptionKey, object> Option<T>(PerLanguageOption<CodeStyleOption<T>> option, T enabled, NotificationOption notification)
            => OptionsSet(SingleOption(option, enabled, notification));

        protected IDictionary<OptionKey, object> Option<T>(PerLanguageOption<T> option, T value)
            => OptionsSet(SingleOption(option, value));

        protected IDictionary<OptionKey, object> Option<T>(PerLanguageOption<CodeStyleOption<T>> option, CodeStyleOption<T> codeStyle)
            => OptionsSet(SingleOption(option, codeStyle));

        protected static IDictionary<OptionKey, object> OptionsSet(
            params (OptionKey key, object value)[] options)
        {
            var result = new Dictionary<OptionKey, object>();
            foreach (var option in options)
            {
                result.Add(option.key, option.value);
            }

            return result;
        }
    }
}
