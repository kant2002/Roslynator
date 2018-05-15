﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Rename;
using Roslynator.CSharp.Analysis;
using Roslynator.CSharp.Refactorings.MakeMemberAbstract;
using Roslynator.CSharp.Refactorings.MakeMemberVirtual;
using Roslynator.CSharp.Refactorings.ReplaceMethodWithProperty;

namespace Roslynator.CSharp.Refactorings
{
    internal static class MethodDeclarationRefactoring
    {
        public static async Task ComputeRefactoringsAsync(RefactoringContext context, MethodDeclarationSyntax methodDeclaration)
        {
            if (context.IsRefactoringEnabled(RefactoringIdentifiers.ChangeMethodReturnTypeToVoid)
                && context.Span.IsEmptyAndContainedInSpan(methodDeclaration))
            {
                await ChangeMethodReturnTypeToVoidRefactoring.ComputeRefactoringAsync(context, methodDeclaration).ConfigureAwait(false);
            }

            if (context.IsRefactoringEnabled(RefactoringIdentifiers.AddTypeParameter))
                AddTypeParameterRefactoring.ComputeRefactoring(context, methodDeclaration);

            if (context.IsRefactoringEnabled(RefactoringIdentifiers.ReplaceMethodWithProperty)
                && methodDeclaration.HeaderSpan().Contains(context.Span)
                && ReplaceMethodWithPropertyRefactoring.CanRefactor(methodDeclaration))
            {
                context.RegisterRefactoring(
                    $"Replace '{methodDeclaration.Identifier.ValueText}' with property",
                    cancellationToken => ReplaceMethodWithPropertyRefactoring.RefactorAsync(context.Document, methodDeclaration, cancellationToken),
                    RefactoringIdentifiers.ReplaceMethodWithProperty);
            }

            if (context.IsRefactoringEnabled(RefactoringIdentifiers.UseExpressionBodiedMember)
                && context.SupportsCSharp6
                && methodDeclaration.Body != null
                && context.Span.IsEmptyAndContainedInSpanOrBetweenSpans(methodDeclaration.Body)
                && UseExpressionBodiedMemberAnalysis.GetExpression(methodDeclaration.Body) != null)
            {
                context.RegisterRefactoring(
                    UseExpressionBodiedMemberRefactoring.Title,
                    cancellationToken => UseExpressionBodiedMemberRefactoring.RefactorAsync(context.Document, methodDeclaration, cancellationToken),
                    RefactoringIdentifiers.UseExpressionBodiedMember);
            }

            if (context.IsRefactoringEnabled(RefactoringIdentifiers.MakeMemberAbstract)
                && methodDeclaration.HeaderSpan().Contains(context.Span))
            {
                MakeMethodAbstractRefactoring.ComputeRefactoring(context, methodDeclaration);
            }

            if (context.IsRefactoringEnabled(RefactoringIdentifiers.MakeMemberVirtual)
                && methodDeclaration.HeaderSpan().Contains(context.Span))
            {
                MakeMethodVirtualRefactoring.ComputeRefactoring(context, methodDeclaration);
            }

            if (context.IsRefactoringEnabled(RefactoringIdentifiers.CopyDocumentationCommentFromBaseMember)
                && methodDeclaration.HeaderSpan().Contains(context.Span))
            {
                await CopyDocumentationCommentFromBaseMemberRefactoring.ComputeRefactoringAsync(context, methodDeclaration).ConfigureAwait(false);
            }

            if (context.IsRefactoringEnabled(RefactoringIdentifiers.RenameMethodAccordingToTypeName))
                await RenameMethodAccoringToTypeNameAsync(context, methodDeclaration).ConfigureAwait(false);

            if (context.IsRefactoringEnabled(RefactoringIdentifiers.AddMemberToInterface)
                && context.Span.IsEmptyAndContainedInSpanOrBetweenSpans(methodDeclaration.Identifier))
            {
                SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                AddMemberToInterfaceRefactoring.ComputeRefactoring(context, methodDeclaration, semanticModel);
            }

            if (context.IsRefactoringEnabled(RefactoringIdentifiers.UseListInsteadOfYield)
                && methodDeclaration.Identifier.Span.Contains(context.Span))
            {
                SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                UseListInsteadOfYieldRefactoring.ComputeRefactoring(context, methodDeclaration, semanticModel);
            }
        }

        private static async Task RenameMethodAccoringToTypeNameAsync(
            RefactoringContext context,
            MethodDeclarationSyntax methodDeclaration)
        {
            TypeSyntax returnType = methodDeclaration.ReturnType;

            if (returnType?.IsVoid() != false)
                return;

            SyntaxToken identifier = methodDeclaration.Identifier;

            if (!context.Span.IsEmptyAndContainedInSpanOrBetweenSpans(identifier))
                return;

            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

            IMethodSymbol methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken);

            ITypeSymbol typeSymbol = GetType(returnType, semanticModel, context.CancellationToken);

            if (typeSymbol == null)
                return;

            string newName = NameGenerator.CreateName(typeSymbol);

            if (string.IsNullOrEmpty(newName))
                return;

            newName = "Get" + newName;

            if (methodSymbol.IsAsync)
                newName += "Async";

            string oldName = identifier.ValueText;

            if (string.Equals(oldName, newName, StringComparison.Ordinal))
                return;

            if (!await MemberNameGenerator.IsUniqueMemberNameAsync(
                newName,
                methodSymbol,
                context.Solution,
                cancellationToken: context.CancellationToken).ConfigureAwait(false))
            {
                return;
            }

            context.RegisterRefactoring(
                $"Rename '{oldName}' to '{newName}'",
                cancellationToken => Renamer.RenameSymbolAsync(context.Solution, methodSymbol, newName, default(OptionSet), cancellationToken),
                RefactoringIdentifiers.RenameMethodAccordingToTypeName);
        }

        private static ITypeSymbol GetType(
            TypeSyntax returnType,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!(semanticModel.GetTypeSymbol(returnType, cancellationToken) is INamedTypeSymbol returnTypeSymbol))
                return null;

            INamedTypeSymbol taskSymbol = semanticModel.GetTypeByMetadataName(MetadataNames.System_Threading_Tasks_Task);

            if (taskSymbol == null)
                return null;

            if (returnTypeSymbol.Equals(taskSymbol))
                return null;

            INamedTypeSymbol taskOfTSymbol = semanticModel.GetTypeByMetadataName(MetadataNames.System_Threading_Tasks_Task_T);

            if (taskOfTSymbol == null)
                return null;

            if (!returnTypeSymbol.ConstructedFrom.Equals(taskOfTSymbol))
                return null;

            return returnTypeSymbol.TypeArguments[0];
        }
    }
}