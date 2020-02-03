// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RR0214SimplifyConditionalExpressionTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.SimplifyConditionalExpression;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.SimplifyConditionalExpression)]
        public async Task Test_ToLogicalAnd()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        bool x = false, y = false;

        bool z = [||]x ? false : y;
    }
}
", @"
class C
{
    void M()
    {
        bool x = false, y = false;

        bool z = !x && y;
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.SimplifyConditionalExpression)]
        public async Task Test_ToLogicalOr()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        bool x = false, y = false;

        bool z = [||]x ? y : true;
    }
}
", @"
class C
{
    void M()
    {
        bool x = false, y = false;

        bool z = !x || y;
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
