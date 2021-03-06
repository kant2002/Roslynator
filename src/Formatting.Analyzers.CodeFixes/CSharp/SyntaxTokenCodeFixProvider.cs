﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp;
using Roslynator.Formatting.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Roslynator.Formatting.CodeFixes.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SyntaxTokenCodeFixProvider))]
    [Shared]
    internal class SyntaxTokenCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.AddEmptyLineBetweenBlockAndStatement,
                    DiagnosticIdentifiers.AddNewLineBeforeConditionalOperatorInsteadOfAfterIt,
                    DiagnosticIdentifiers.AddNewLineAfterConditionalOperatorInsteadOfBeforeIt,
                    DiagnosticIdentifiers.AddNewLineAfterExpressionBodyArrowInsteadOfBeforeIt,
                    DiagnosticIdentifiers.AddNewLineBeforeExpressionBodyArrowInsteadOfAfterIt,
                    DiagnosticIdentifiers.AddNewLineAfterAttributeList);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindToken(root, context.Span.Start, out SyntaxToken token))
                return;

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            switch (diagnostic.Id)
            {
                case DiagnosticIdentifiers.AddEmptyLineBetweenBlockAndStatement:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            CodeFixTitles.AddEmptyLine,
                            ct => CodeFixHelpers.AppendEndOfLineAsync(document, token, ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
                case DiagnosticIdentifiers.AddNewLineBeforeConditionalOperatorInsteadOfAfterIt:
                    {
                        var conditionalExpression = (ConditionalExpressionSyntax)token.Parent;

                        string title = null;
                        if (token.IsKind(SyntaxKind.QuestionToken))
                        {
                            title = (SyntaxTriviaAnalysis.IsTokenFollowedWithNewLineAndNotPrecededWithNewLine(conditionalExpression.WhenTrue, conditionalExpression.ColonToken, conditionalExpression.WhenFalse))
                                ? "Add newline before '?' and ':' instead of after it"
                                : "Add newline before '?' instead of after it";
                        }
                        else
                        {
                            title = "Add newline before ':' instead of after it";
                        }

                        CodeAction codeAction = CodeAction.Create(
                            title,
                            ct => AddNewLineBeforeConditionalOperatorInsteadOfAfterItAsync(document, conditionalExpression, ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
                case DiagnosticIdentifiers.AddNewLineAfterConditionalOperatorInsteadOfBeforeIt:
                    {
                        var conditionalExpression = (ConditionalExpressionSyntax)token.Parent;

                        string title = null;
                        if (token.IsKind(SyntaxKind.QuestionToken))
                        {
                            title = (SyntaxTriviaAnalysis.IsTokenFollowedWithNewLineAndNotPrecededWithNewLine(conditionalExpression.WhenTrue, conditionalExpression.ColonToken, conditionalExpression.WhenFalse))
                                ? "Add newline after '?' and ':' instead of before it"
                                : "Add newline after '?' instead of before it";
                        }
                        else
                        {
                            title = "Add newline after ':' instead of before it";
                        }

                        CodeAction codeAction = CodeAction.Create(
                            title,
                            ct => AddNewLineAfterConditionalOperatorInsteadOfBeforeItAsync(document, conditionalExpression, ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
                case DiagnosticIdentifiers.AddNewLineAfterExpressionBodyArrowInsteadOfBeforeIt:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            "Add newline after '=>' instead of before it",
                            ct =>
                            {
                                var expressionBody = (ArrowExpressionClauseSyntax)token.Parent;

                                return CodeFixHelpers.AddNewLineAfterInsteadOfBeforeAsync(document, token.GetPreviousToken(), token, expressionBody.Expression, ct);
                            },
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
                case DiagnosticIdentifiers.AddNewLineBeforeExpressionBodyArrowInsteadOfAfterIt:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            "Add newline before '=>' instead of after it",
                            ct =>
                            {
                                var expressionBody = (ArrowExpressionClauseSyntax)token.Parent;

                                return CodeFixHelpers.AddNewLineBeforeInsteadOfAfterAsync(document, token.GetPreviousToken(), token, expressionBody.Expression, ct);
                            },
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
                case DiagnosticIdentifiers.AddNewLineAfterAttributeList:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            CodeFixTitles.AddNewLine,
                            ct => CodeFixHelpers.AddNewLineBeforeAsync(document, token, ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
            }
        }

        private static Task<Document> AddNewLineBeforeConditionalOperatorInsteadOfAfterItAsync(
            Document document,
            ConditionalExpressionSyntax conditionalExpression,
            CancellationToken cancellationToken)
        {
            ExpressionSyntax condition = conditionalExpression.Condition;
            ExpressionSyntax whenTrue = conditionalExpression.WhenTrue;
            ExpressionSyntax whenFalse = conditionalExpression.WhenFalse;
            SyntaxToken questionToken = conditionalExpression.QuestionToken;
            SyntaxToken colonToken = conditionalExpression.ColonToken;

            ExpressionSyntax newCondition = condition;
            ExpressionSyntax newWhenTrue = whenTrue;
            ExpressionSyntax newWhenFalse = whenFalse;
            SyntaxToken newQuestionToken = questionToken;
            SyntaxToken newColonToken = colonToken;

            if (SyntaxTriviaAnalysis.IsTokenFollowedWithNewLineAndNotPrecededWithNewLine(condition, questionToken, whenTrue))
            {
                var (left, token, right) = CodeFixHelpers.AddNewLineBeforeTokenInsteadOfAfterIt(condition, questionToken, whenTrue);

                newCondition = left;
                newQuestionToken = token;
                newWhenTrue = right;
            }

            if (SyntaxTriviaAnalysis.IsTokenFollowedWithNewLineAndNotPrecededWithNewLine(whenTrue, colonToken, whenFalse))
            {
                var (left, token, right) = CodeFixHelpers.AddNewLineBeforeTokenInsteadOfAfterIt(newWhenTrue, colonToken, whenFalse);

                newWhenTrue = left;
                newColonToken = token;
                newWhenFalse = right;
            }

            ConditionalExpressionSyntax newConditionalExpression = ConditionalExpression(
                newCondition,
                newQuestionToken,
                newWhenTrue,
                newColonToken,
                newWhenFalse);

            return document.ReplaceNodeAsync(conditionalExpression, newConditionalExpression, cancellationToken);
        }

        private static Task<Document> AddNewLineAfterConditionalOperatorInsteadOfBeforeItAsync(
            Document document,
            ConditionalExpressionSyntax conditionalExpression,
            CancellationToken cancellationToken)
        {
            ExpressionSyntax condition = conditionalExpression.Condition;
            ExpressionSyntax whenTrue = conditionalExpression.WhenTrue;
            ExpressionSyntax whenFalse = conditionalExpression.WhenFalse;
            SyntaxToken questionToken = conditionalExpression.QuestionToken;
            SyntaxToken colonToken = conditionalExpression.ColonToken;

            ExpressionSyntax newCondition = condition;
            ExpressionSyntax newWhenTrue = whenTrue;
            ExpressionSyntax newWhenFalse = whenFalse;
            SyntaxToken newQuestionToken = questionToken;
            SyntaxToken newColonToken = colonToken;

            if (SyntaxTriviaAnalysis.IsTokenPrecededWithNewLineAndNotFollowedWithNewLine(condition, questionToken, whenTrue))
            {
                var (left, token, right) = CodeFixHelpers.AddNewLineAfterTokenInsteadOfBeforeIt(condition, questionToken, whenTrue);

                newCondition = left;
                newQuestionToken = token;
                newWhenTrue = right;
            }

            if (SyntaxTriviaAnalysis.IsTokenPrecededWithNewLineAndNotFollowedWithNewLine(whenTrue, colonToken, whenFalse))
            {
                var (left, token, right) = CodeFixHelpers.AddNewLineAfterTokenInsteadOfBeforeIt(newWhenTrue, colonToken, whenFalse);

                newWhenTrue = left;
                newColonToken = token;
                newWhenFalse = right;
            }

            ConditionalExpressionSyntax newConditionalExpression = ConditionalExpression(
                newCondition,
                newQuestionToken,
                newWhenTrue,
                newColonToken,
                newWhenFalse);

            return document.ReplaceNodeAsync(conditionalExpression, newConditionalExpression, cancellationToken);
        }
    }
}
