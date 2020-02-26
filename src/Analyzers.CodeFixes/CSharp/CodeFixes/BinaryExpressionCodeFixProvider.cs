﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Refactorings;
using Roslynator.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BinaryExpressionCodeFixProvider))]
    [Shared]
    public class BinaryExpressionCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.SimplifyBooleanComparison,
                    DiagnosticIdentifiers.CallSkipAndAnyInsteadOfCount,
                    DiagnosticIdentifiers.AvoidNullLiteralExpressionOnLeftSideOfBinaryExpression,
                    DiagnosticIdentifiers.UseStringIsNullOrEmptyMethod,
                    DiagnosticIdentifiers.SimplifyCoalesceExpression,
                    DiagnosticIdentifiers.RemoveRedundantAsOperator,
                    DiagnosticIdentifiers.UnconstrainedTypeParameterCheckedForNull,
                    DiagnosticIdentifiers.ValueTypeObjectIsNeverEqualToNull,
                    DiagnosticIdentifiers.JoinStringExpressions,
                    DiagnosticIdentifiers.UseExclusiveOrOperator,
                    DiagnosticIdentifiers.SimplifyBooleanExpression,
                    DiagnosticIdentifiers.UseShortCircuitingOperator,
                    DiagnosticIdentifiers.UnnecessaryOperator);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out BinaryExpressionSyntax binaryExpression))
                return;

            Document document = context.Document;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.SimplifyBooleanComparison:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Simplify boolean comparison",
                                cancellationToken => SimplifyBooleanComparisonRefactoring.RefactorAsync(document, binaryExpression, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);

                            break;
                        }
                    case DiagnosticIdentifiers.CallSkipAndAnyInsteadOfCount:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Call 'Skip' and 'Any' instead of 'Count'",
                                cancellationToken => CallSkipAndAnyInsteadOfCountRefactoring.RefactorAsync(document, binaryExpression, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.AvoidNullLiteralExpressionOnLeftSideOfBinaryExpression:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Swap operands",
                                cancellationToken => DocumentRefactorings.SwapBinaryOperandsAsync(document, binaryExpression, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.UseStringIsNullOrEmptyMethod:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Use 'string.IsNullOrEmpty' method",
                                ct => UseStringIsNullOrEmptyMethodAsync(document, binaryExpression, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.SimplifyCoalesceExpression:
                        {
                            ExpressionSyntax expression = binaryExpression.Left;

                            if (expression == null
                                || !context.Span.Contains(expression.Span))
                            {
                                expression = binaryExpression.Right;
                            }

                            CodeAction codeAction = CodeAction.Create(
                                "Simplify coalesce expression",
                                cancellationToken => SimplifyCoalesceExpressionRefactoring.RefactorAsync(document, binaryExpression, expression, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.RemoveRedundantAsOperator:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Remove redundant 'as' operator",
                                cancellationToken => RemoveRedundantAsOperatorRefactoring.RefactorAsync(document, binaryExpression, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.UnconstrainedTypeParameterCheckedForNull:
                        {
                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(binaryExpression.Left, context.CancellationToken);

                            CodeAction codeAction = CodeAction.Create(
                                $"Use EqualityComparer<{typeSymbol.Name}>.Default",
                                cancellationToken => UnconstrainedTypeParameterCheckedForNullRefactoring.RefactorAsync(document, binaryExpression, typeSymbol, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.ValueTypeObjectIsNeverEqualToNull:
                        {
                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(binaryExpression.Left, context.CancellationToken);

                            string title;

                            if (CSharpFacts.IsSimpleType(typeSymbol.SpecialType)
                                || typeSymbol.ContainsMember<IMethodSymbol>(WellKnownMemberNames.EqualityOperatorName))
                            {
                                ExpressionSyntax expression = typeSymbol.GetDefaultValueSyntax(document.GetDefaultSyntaxOptions());

                                title = $"Replace 'null' with '{expression}'";
                            }
                            else
                            {
                                title = $"Use EqualityComparer<{SymbolDisplay.ToMinimalDisplayString(typeSymbol, semanticModel, binaryExpression.Right.SpanStart, SymbolDisplayFormats.Default)}>.Default";
                            }

                            CodeAction codeAction = CodeAction.Create(
                                title,
                                cancellationToken => ValueTypeObjectIsNeverEqualToNullRefactoring.RefactorAsync(document, binaryExpression, typeSymbol, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.JoinStringExpressions:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Join string expressions",
                                cancellationToken => JoinStringExpressionsRefactoring.RefactorAsync(document, binaryExpression, context.Span, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.UseExclusiveOrOperator:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Use ^ operator",
                                cancellationToken => UseExclusiveOrOperatorRefactoring.RefactorAsync(document, binaryExpression, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.SimplifyBooleanExpression:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Simplify boolean expression",
                                cancellationToken => SimplifyBooleanExpressionRefactoring.RefactorAsync(document, binaryExpression, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.UseShortCircuitingOperator:
                        {
                            SyntaxToken operatorToken = binaryExpression.OperatorToken;

                            SyntaxKind kind = binaryExpression.Kind();

                            SyntaxToken newToken = default;

                            if (kind == SyntaxKind.BitwiseAndExpression)
                            {
                                newToken = Token(operatorToken.LeadingTrivia, SyntaxKind.AmpersandAmpersandToken, operatorToken.TrailingTrivia);
                            }
                            else if (kind == SyntaxKind.BitwiseOrExpression)
                            {
                                newToken = Token(operatorToken.LeadingTrivia, SyntaxKind.BarBarToken, operatorToken.TrailingTrivia);
                            }

                            CodeAction codeAction = CodeAction.Create(
                                $"Use '{newToken.ToString()}' operator",
                                ct =>
                                {
                                    BinaryExpressionSyntax newBinaryExpression = null;

                                    if (kind == SyntaxKind.BitwiseAndExpression)
                                    {
                                        newBinaryExpression = LogicalAndExpression(binaryExpression.Left, newToken, binaryExpression.Right);
                                    }
                                    else if (kind == SyntaxKind.BitwiseOrExpression)
                                    {
                                        newBinaryExpression = LogicalOrExpression(binaryExpression.Left, newToken, binaryExpression.Right);
                                    }

                                    return document.ReplaceNodeAsync(binaryExpression, newBinaryExpression, ct);
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.UnnecessaryOperator:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Use '==' operator",
                                ct =>
                                {
                                    SyntaxToken operatorToken = binaryExpression.OperatorToken;

                                    BinaryExpressionSyntax newBinaryExpression = EqualsExpression(
                                        binaryExpression.Left,
                                        Token(operatorToken.LeadingTrivia, SyntaxKind.EqualsEqualsToken, operatorToken.TrailingTrivia),
                                        binaryExpression.Right);

                                    return document.ReplaceNodeAsync(binaryExpression, newBinaryExpression, ct);
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }

        private static async Task<Document> UseStringIsNullOrEmptyMethodAsync(
            Document document,
            BinaryExpressionSyntax binaryExpression,
            CancellationToken cancellationToken)
        {
            if (binaryExpression.IsKind(SyntaxKind.EqualsExpression))
            {
                SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

                BinaryExpressionInfo binaryExpressionInfo = SyntaxInfo.BinaryExpressionInfo(binaryExpression);

                ExpressionSyntax expression = (CSharpUtility.IsEmptyStringExpression(binaryExpressionInfo.Left, semanticModel, cancellationToken))
                    ? binaryExpressionInfo.Right
                    : binaryExpressionInfo.Left;

                ExpressionSyntax newNode = SimpleMemberInvocationExpression(
                    CSharpTypeFactory.StringType(),
                    IdentifierName("IsNullOrEmpty"),
                    Argument(expression));

                newNode = newNode
                    .WithTriviaFrom(binaryExpression)
                    .WithFormatterAnnotation();

                return await document.ReplaceNodeAsync(binaryExpression, newNode, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                NullCheckExpressionInfo nullCheck = SyntaxInfo.NullCheckExpressionInfo(binaryExpression.Left);

                ExpressionSyntax newNode = SimpleMemberInvocationExpression(
                    CSharpTypeFactory.StringType(),
                    IdentifierName("IsNullOrEmpty"),
                    Argument(nullCheck.Expression));

                if (nullCheck.IsCheckingNotNull)
                    newNode = LogicalNotExpression(newNode);

                newNode = newNode
                    .WithTriviaFrom(binaryExpression)
                    .WithFormatterAnnotation();

                return await document.ReplaceNodeAsync(binaryExpression, newNode, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}