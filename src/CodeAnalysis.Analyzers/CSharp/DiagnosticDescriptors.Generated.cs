﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// <auto-generated>

using System;
using Microsoft.CodeAnalysis;

namespace Roslynator.CodeAnalysis.CSharp
{
    public static partial class DiagnosticDescriptors
    {
        /// <summary>RCS9001</summary>
        public static readonly DiagnosticDescriptor UsePatternMatching = Factory.Create(
            id:                 DiagnosticIdentifiers.UsePatternMatching, 
            title:              "Use pattern matching.", 
            messageFormat:      "Use pattern matching.", 
            category:           DiagnosticCategories.Usage, 
            defaultSeverity:    DiagnosticSeverity.Hidden, 
            isEnabledByDefault: true, 
            description:        null, 
            helpLinkUri:        DiagnosticIdentifiers.UsePatternMatching, 
            customTags:         Array.Empty<string>());

        /// <summary>RCS9002</summary>
        public static readonly DiagnosticDescriptor UsePropertySyntaxNodeSpanStart = Factory.Create(
            id:                 DiagnosticIdentifiers.UsePropertySyntaxNodeSpanStart, 
            title:              "Use property SyntaxNode.SpanStart.", 
            messageFormat:      "Use property SyntaxNode.SpanStart.", 
            category:           DiagnosticCategories.Performance, 
            defaultSeverity:    DiagnosticSeverity.Info, 
            isEnabledByDefault: true, 
            description:        null, 
            helpLinkUri:        DiagnosticIdentifiers.UsePropertySyntaxNodeSpanStart, 
            customTags:         Array.Empty<string>());

        /// <summary>RCS9003</summary>
        public static readonly DiagnosticDescriptor UnnecessaryConditionalAccess = Factory.Create(
            id:                 DiagnosticIdentifiers.UnnecessaryConditionalAccess, 
            title:              "Unnecessary conditional access.", 
            messageFormat:      "Unnecessary conditional access.", 
            category:           DiagnosticCategories.Performance, 
            defaultSeverity:    DiagnosticSeverity.Info, 
            isEnabledByDefault: true, 
            description:        null, 
            helpLinkUri:        DiagnosticIdentifiers.UnnecessaryConditionalAccess, 
            customTags:         WellKnownDiagnosticTags.Unnecessary);

        public static readonly DiagnosticDescriptor UnnecessaryConditionalAccessFadeOut = DiagnosticDescriptorFactory.CreateFadeOut(UnnecessaryConditionalAccess);

        /// <summary>RCS9004</summary>
        public static readonly DiagnosticDescriptor CallAnyInsteadOfAccessingCount = Factory.Create(
            id:                 DiagnosticIdentifiers.CallAnyInsteadOfAccessingCount, 
            title:              "Call 'Any' instead of accessing 'Count'.", 
            messageFormat:      "Call 'Any' instead of accessing 'Count'.", 
            category:           DiagnosticCategories.Performance, 
            defaultSeverity:    DiagnosticSeverity.Info, 
            isEnabledByDefault: true, 
            description:        null, 
            helpLinkUri:        DiagnosticIdentifiers.CallAnyInsteadOfAccessingCount, 
            customTags:         Array.Empty<string>());

        /// <summary>RCS9005</summary>
        public static readonly DiagnosticDescriptor UnnecessaryNullCheck = Factory.Create(
            id:                 DiagnosticIdentifiers.UnnecessaryNullCheck, 
            title:              "Unnecessary null check.", 
            messageFormat:      "Unnecessary null check.", 
            category:           DiagnosticCategories.Performance, 
            defaultSeverity:    DiagnosticSeverity.Info, 
            isEnabledByDefault: true, 
            description:        null, 
            helpLinkUri:        DiagnosticIdentifiers.UnnecessaryNullCheck, 
            customTags:         WellKnownDiagnosticTags.Unnecessary);

        /// <summary>RCS9006</summary>
        public static readonly DiagnosticDescriptor UseElementAccess = Factory.Create(
            id:                 DiagnosticIdentifiers.UseElementAccess, 
            title:              "Use element access.", 
            messageFormat:      "Use element access.", 
            category:           DiagnosticCategories.Usage, 
            defaultSeverity:    DiagnosticSeverity.Info, 
            isEnabledByDefault: true, 
            description:        null, 
            helpLinkUri:        DiagnosticIdentifiers.UseElementAccess, 
            customTags:         Array.Empty<string>());

        /// <summary>RCS9007</summary>
        public static readonly DiagnosticDescriptor UseReturnValue = Factory.Create(
            id:                 DiagnosticIdentifiers.UseReturnValue, 
            title:              "Use return value.", 
            messageFormat:      "Use return value.", 
            category:           DiagnosticCategories.Usage, 
            defaultSeverity:    DiagnosticSeverity.Warning, 
            isEnabledByDefault: true, 
            description:        null, 
            helpLinkUri:        DiagnosticIdentifiers.UseReturnValue, 
            customTags:         Array.Empty<string>());

        /// <summary>RCS9008</summary>
        public static readonly DiagnosticDescriptor CallLastInsteadOfUsingElementAccess = Factory.Create(
            id:                 DiagnosticIdentifiers.CallLastInsteadOfUsingElementAccess, 
            title:              "Call 'Last' instead of using [].", 
            messageFormat:      "Call 'Last' instead of using [].", 
            category:           DiagnosticCategories.Usage, 
            defaultSeverity:    DiagnosticSeverity.Info, 
            isEnabledByDefault: true, 
            description:        null, 
            helpLinkUri:        DiagnosticIdentifiers.CallLastInsteadOfUsingElementAccess, 
            customTags:         Array.Empty<string>());

        /// <summary>RCS9009</summary>
        public static readonly DiagnosticDescriptor UnknownLanguageName = Factory.Create(
            id:                 DiagnosticIdentifiers.UnknownLanguageName, 
            title:              "Unknown language name.", 
            messageFormat:      "Unknown language name.", 
            category:           DiagnosticCategories.General, 
            defaultSeverity:    DiagnosticSeverity.Warning, 
            isEnabledByDefault: true, 
            description:        null, 
            helpLinkUri:        DiagnosticIdentifiers.UnknownLanguageName, 
            customTags:         Array.Empty<string>());

        /// <summary>RCS9010</summary>
        public static readonly DiagnosticDescriptor SpecifyExportCodeRefactoringProviderAttributeName = Factory.Create(
            id:                 DiagnosticIdentifiers.SpecifyExportCodeRefactoringProviderAttributeName, 
            title:              "Specify ExportCodeRefactoringProviderAttribute.Name.", 
            messageFormat:      "Specify ExportCodeRefactoringProviderAttribute.Name.", 
            category:           DiagnosticCategories.Usage, 
            defaultSeverity:    DiagnosticSeverity.Hidden, 
            isEnabledByDefault: true, 
            description:        null, 
            helpLinkUri:        DiagnosticIdentifiers.SpecifyExportCodeRefactoringProviderAttributeName, 
            customTags:         Array.Empty<string>());

        /// <summary>RCS9011</summary>
        public static readonly DiagnosticDescriptor SpecifyExportCodeFixProviderAttributeName = Factory.Create(
            id:                 DiagnosticIdentifiers.SpecifyExportCodeFixProviderAttributeName, 
            title:              "Specify ExportCodeFixProviderAttribute.Name.", 
            messageFormat:      "Specify ExportCodeFixProviderAttribute.Name.", 
            category:           DiagnosticCategories.Usage, 
            defaultSeverity:    DiagnosticSeverity.Hidden, 
            isEnabledByDefault: true, 
            description:        null, 
            helpLinkUri:        DiagnosticIdentifiers.SpecifyExportCodeFixProviderAttributeName, 
            customTags:         Array.Empty<string>());

    }
}