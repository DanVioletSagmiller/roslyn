// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualStudio.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces
{
    public partial class TestWorkspace
    {
        private const string CSharpExtension = ".cs";
        private const string CSharpScriptExtension = ".csx";
        private const string VisualBasicExtension = ".vb";
        private const string VisualBasicScriptExtension = ".vbx";

        private const string WorkspaceElementName = "Workspace";
        private const string ProjectElementName = "Project";
        private const string SubmissionElementName = "Submission";
        private const string MetadataReferenceElementName = "MetadataReference";
        private const string MetadataReferenceFromSourceElementName = "MetadataReferenceFromSource";
        private const string ProjectReferenceElementName = "ProjectReference";
        private const string CompilationOptionsElementName = "CompilationOptions";
        private const string RootNamespaceAttributeName = "RootNamespace";
        private const string OutputTypeAttributeName = "OutputType";
        private const string ReportDiagnosticAttributeName = "ReportDiagnostic";
        private const string CryptoKeyFileAttributeName = "CryptoKeyFile";
        private const string StrongNameProviderAttributeName = "StrongNameProvider";
        private const string DelaySignAttributeName = "DelaySign";
        private const string ParseOptionsElementName = "ParseOptions";
        private const string LanguageVersionAttributeName = "LanguageVersion";
        private const string FeaturesAttributeName = "Features";
        private const string DocumentationModeAttributeName = "DocumentationMode";
        private const string DocumentElementName = "Document";
        private const string AdditionalDocumentElementName = "AdditionalDocument";
        private const string AnalyzerConfigDocumentElementName = "AnalyzerConfigDocument";
        private const string AnalyzerElementName = "Analyzer";
        private const string AssemblyNameAttributeName = "AssemblyName";
        private const string CommonReferencesAttributeName = "CommonReferences";
        private const string CommonReferencesWinRTAttributeName = "CommonReferencesWinRT";
        private const string CommonReferencesNet45AttributeName = "CommonReferencesNet45";
        private const string CommonReferencesPortableAttributeName = "CommonReferencesPortable";
        private const string CommonReferenceFacadeSystemRuntimeAttributeName = "CommonReferenceFacadeSystemRuntime";
        private const string FilePathAttributeName = "FilePath";
        private const string FoldersAttributeName = "Folders";
        private const string KindAttributeName = "Kind";
        private const string LanguageAttributeName = "Language";
        private const string GlobalImportElementName = "GlobalImport";
        private const string IncludeXmlDocCommentsAttributeName = "IncludeXmlDocComments";
        private const string IsLinkFileAttributeName = "IsLinkFile";
        private const string LinkAssemblyNameAttributeName = "LinkAssemblyName";
        private const string LinkProjectNameAttributeName = "LinkProjectName";
        private const string LinkFilePathAttributeName = "LinkFilePath";
        private const string PreprocessorSymbolsAttributeName = "PreprocessorSymbols";
        private const string AnalyzerDisplayAttributeName = "Name";
        private const string AnalyzerFullPathAttributeName = "FullPath";
        private const string AliasAttributeName = "Alias";
        private const string ProjectNameAttribute = "Name";
        private const string CheckOverflowAttributeName = "CheckOverflow";
        private const string AllowUnsafeAttributeName = "AllowUnsafe";
        private const string OutputKindName = "OutputKind";
        private const string NullableAttributeName = "Nullable";

        internal static string GetDefaultTestSourceDocumentName(int index, string extension)
           => "test" + (index + 1) + extension;

        #region C#

        #endregion
    }
}
