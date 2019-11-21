// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CodeRefactorings.MoveType;
using Microsoft.CodeAnalysis.Editor.UnitTests.CodeActions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.UnitTests;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.MoveType
{
    public abstract class AbstractMoveTypeTest : AbstractCodeActionTest
    {
        private string RenameFileCodeActionTitle = FeaturesResources.Rename_file_to_0;
        private string RenameTypeCodeActionTitle = FeaturesResources.Rename_type_to_0;

        protected override CodeRefactoringProvider CreateCodeRefactoringProvider(Workspace workspace, TestParameters parameters)
            => new MoveTypeCodeRefactoringProvider();

        protected async Task TestMoveTypeToNewFileAsync(
            string originalCode,
            string expectedSourceTextAfterRefactoring,
            string expectedDocumentName,
            string destinationDocumentText,
            ImmutableArray<string> destinationDocumentContainers = default,
            bool expectedCodeAction = true,
            int index = 0,
            Action<Workspace> onAfterWorkspaceCreated = null)
        {
            var testOptions = new TestParameters(index: index);
            if (expectedCodeAction)
            {
                using (var workspace = CreateWorkspaceFromFile(originalCode, testOptions))
                {
                    onAfterWorkspaceCreated?.Invoke(workspace);

                    // replace with default values on null.
                    destinationDocumentContainers = destinationDocumentContainers.NullToEmpty();

                    var sourceDocumentId = workspace.Documents[0].Id;

                    // Verify the newly added document and its text
                    var oldSolutionAndNewSolution = await TestAddDocumentAsync(
                        testOptions, workspace, destinationDocumentText,
                        expectedDocumentName, destinationDocumentContainers);

                    // Verify source document's text after moving type.
                    var oldSolution = oldSolutionAndNewSolution.Item1;
                    var newSolution = oldSolutionAndNewSolution.Item2;
                    var changedDocumentIds = SolutionUtilities.GetChangedDocuments(oldSolution, newSolution);
                    Assert.True(changedDocumentIds.Contains(sourceDocumentId), "source document was not changed.");

                    var modifiedSourceDocument = newSolution.GetDocument(sourceDocumentId);
                    Assert.Equal(expectedSourceTextAfterRefactoring, (await modifiedSourceDocument.GetTextAsync()).ToString());
                }
            }
            else
            {
                await TestMissingAsync(originalCode);
            }
        }
    }
}
