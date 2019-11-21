// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.CodeGeneration
{
    [Trait(Traits.Feature, Traits.Features.CodeGeneration)]
    public class ExpressionGenerationTests : AbstractCodeGenerationTests
    {
        [Fact]
        public void TestFalseExpression()
        {
            Test(
                f => f.FalseLiteralExpression(),
                cs: "false",
                csSimple: "false");
        }

        [Fact]
        public void TestTrueExpression()
        {
            Test(
                f => f.TrueLiteralExpression(),
                cs: "true",
                csSimple: "true");
        }

        [Fact]
        public void TestNullExpression()
        {
            Test(
                f => f.NullLiteralExpression(),
                cs: "null",
                csSimple: "null");
        }

        [Fact]
        public void TestThisExpression()
        {
            Test(
                f => f.ThisExpression(),
                cs: "this",
                csSimple: "this");
        }

        [Fact]
        public void TestBaseExpression()
        {
            Test(
                f => f.BaseExpression(),
                cs: "base",
                csSimple: "base");
        }

        [Fact]
        public void TestInt32LiteralExpression0()
        {
            Test(
                f => f.LiteralExpression(0),
                cs: "0",
                csSimple: "0");
        }

        [Fact]
        public void TestInt32LiteralExpression1()
        {
            Test(
                f => f.LiteralExpression(1),
                cs: "1",
                csSimple: "1");
        }

        [Fact]
        public void TestInt64LiteralExpression0()
        {
            Test(
                f => f.LiteralExpression(0L),
                cs: "0L",
                csSimple: "0L");
        }

        [Fact]
        public void TestInt64LiteralExpression1()
        {
            Test(
                f => f.LiteralExpression(1L),
                cs: "1L",
                csSimple: "1L");
        }

        [Fact]
        public void TestSingleLiteralExpression0()
        {
            Test(
                f => f.LiteralExpression(0.0f),
                cs: "0F",
                csSimple: "0F");
        }

        [Fact]
        public void TestSingleLiteralExpression1()
        {
            Test(
                f => f.LiteralExpression(0.5F),
                cs: "0.5F",
                csSimple: "0.5F");
        }

        [Fact]
        public void TestDoubleLiteralExpression0()
        {
            Test(
                f => f.LiteralExpression(0.0d),
                cs: "0D",
                csSimple: "0D");
        }

        [Fact]
        public void TestDoubleLiteralExpression1()
        {
            Test(
                f => f.LiteralExpression(0.5D),
                cs: "0.5D",
                csSimple: "0.5D");
        }

        [Fact]
        public void TestAddExpression1()
        {
            Test(
                f => f.AddExpression(
                    f.LiteralExpression(1),
                    f.LiteralExpression(2)),
                cs: "(1) + (2)",
                csSimple: "1 + 2");
        }

        [Fact]
        public void TestAddExpression2()
        {
            Test(
                f => f.AddExpression(
                    f.LiteralExpression(1),
                    f.AddExpression(
                        f.LiteralExpression(2),
                        f.LiteralExpression(3))),
                cs: "(1) + ((2) + (3))",
                csSimple: "1 + 2 + 3");
        }

        [Fact]
        public void TestAddExpression3()
        {
            Test(
                f => f.AddExpression(
                    f.AddExpression(
                        f.LiteralExpression(1),
                        f.LiteralExpression(2)),
                    f.LiteralExpression(3)),
                cs: "((1) + (2)) + (3)",
                csSimple: "1 + 2 + 3");
        }

        [Fact]
        public void TestMultiplyExpression1()
        {
            Test(
                f => f.MultiplyExpression(
                    f.LiteralExpression(1),
                    f.LiteralExpression(2)),
                cs: "(1) * (2)",
                csSimple: "1 * 2");
        }

        [Fact]
        public void TestMultiplyExpression2()
        {
            Test(
                f => f.MultiplyExpression(
                    f.LiteralExpression(1),
                    f.MultiplyExpression(
                        f.LiteralExpression(2),
                        f.LiteralExpression(3))),
                cs: "(1) * ((2) * (3))",
                csSimple: "1 * 2 * 3");
        }

        [Fact]
        public void TestMultiplyExpression3()
        {
            Test(
                f => f.MultiplyExpression(
                    f.MultiplyExpression(
                        f.LiteralExpression(1),
                        f.LiteralExpression(2)),
                    f.LiteralExpression(3)),
                cs: "((1) * (2)) * (3)",
                csSimple: "1 * 2 * 3");
        }

        [Fact]
        public void TestBinaryAndExpression1()
        {
            Test(
                f => f.BitwiseAndExpression(
                    f.LiteralExpression(1),
                    f.LiteralExpression(2)),
                cs: "(1) & (2)",
                csSimple: "1 & 2");
        }

        [Fact]
        public void TestBinaryOrExpression1()
        {
            Test(
                f => f.BitwiseOrExpression(
                    f.LiteralExpression(1),
                    f.LiteralExpression(2)),
                cs: "(1) | (2)",
                csSimple: "1 | 2");
        }

        [Fact]
        public void TestLogicalAndExpression1()
        {
            Test(
                f => f.LogicalAndExpression(
                    f.LiteralExpression(1),
                    f.LiteralExpression(2)),
                cs: "(1) && (2)",
                csSimple: "1 && 2");
        }

        [Fact]
        public void TestLogicalOrExpression1()
        {
            Test(
                f => f.LogicalOrExpression(
                    f.LiteralExpression(1),
                    f.LiteralExpression(2)),
                cs: "(1) || (2)",
                csSimple: "1 || 2");
        }

        [Fact]
        public void TestMemberAccess1()
        {
            Test(
                f => f.MemberAccessExpression(
                    f.IdentifierName("E"),
                    f.IdentifierName("M")),
                cs: "E.M",
                csSimple: "E.M");
        }

        [Fact]
        public void TestConditionalExpression1()
        {
            Test(
                f => f.ConditionalExpression(
                    f.IdentifierName("E"),
                    f.IdentifierName("T"),
                    f.IdentifierName("F")),
                cs: "(E) ? (T) : (F)",
                csSimple: "E ? T : F");
        }

        [Fact]
        public void TestInvocation1()
        {
            Test(
                f => f.InvocationExpression(
                    f.IdentifierName("E")),
                cs: "E()",
                csSimple: "E()");
        }

        [Fact]
        public void TestInvocation2()
        {
            Test(
                f => f.InvocationExpression(
                    f.IdentifierName("E"),
                    f.Argument(f.IdentifierName("a"))),
                cs: "E(a)",
                csSimple: "E(a)");
        }

        [Fact]
        public void TestInvocation3()
        {
            Test(
                f => f.InvocationExpression(
                    f.IdentifierName("E"),
                    f.Argument("n", RefKind.None, f.IdentifierName("a"))),
                cs: "E(n: a)",
                csSimple: "E(n: a)");
        }

        [Fact]
        public void TestInvocation4()
        {
            Test(
                f => f.InvocationExpression(
                    f.IdentifierName("E"),
                    f.Argument(null, RefKind.Out, f.IdentifierName("a")),
                    f.Argument(null, RefKind.Ref, f.IdentifierName("b"))),
                cs: "E(out a, ref b)",
                csSimple: "E(out a, ref b)");
        }

        [Fact]
        public void TestInvocation5()
        {
            Test(
                f => f.InvocationExpression(
                    f.IdentifierName("E"),
                    f.Argument("n1", RefKind.Out, f.IdentifierName("a")),
                    f.Argument("n2", RefKind.Ref, f.IdentifierName("b"))),
                cs: "E(n1: out a, n2: ref b)",
                csSimple: "E(n1: out a, n2: ref b)");
        }

        [Fact]
        public void TestElementAccess1()
        {
            Test(
                f => f.ElementAccessExpression(
                    f.IdentifierName("E")),
                cs: "E[]",
                csSimple: "E[]");
        }

        [Fact]
        public void TestElementAccess2()
        {
            Test(
                f => f.ElementAccessExpression(
                    f.IdentifierName("E"),
                    f.Argument(f.IdentifierName("a"))),
                cs: "E[a]",
                csSimple: "E[a]");
        }

        [Fact]
        public void TestElementAccess3()
        {
            Test(
                f => f.ElementAccessExpression(
                    f.IdentifierName("E"),
                    f.Argument("n", RefKind.None, f.IdentifierName("a"))),
                cs: "E[n: a]",
                csSimple: "E[n: a]");
        }

        [Fact]
        public void TestElementAccess4()
        {
            Test(
                f => f.ElementAccessExpression(
                    f.IdentifierName("E"),
                    f.Argument(null, RefKind.Out, f.IdentifierName("a")),
                    f.Argument(null, RefKind.Ref, f.IdentifierName("b"))),
                cs: "E[out a, ref b]",
                csSimple: "E[out a, ref b]");
        }

        [Fact]
        public void TestElementAccess5()
        {
            Test(
                f => f.ElementAccessExpression(
                    f.IdentifierName("E"),
                    f.Argument("n1", RefKind.Out, f.IdentifierName("a")),
                    f.Argument("n2", RefKind.Ref, f.IdentifierName("b"))),
                cs: "E[n1: out a, n2: ref b]",
                csSimple: "E[n1: out a, n2: ref b]");
        }

        [Fact]
        public void TestIsExpression()
        {
            Test(
                f => f.IsTypeExpression(
                    f.IdentifierName("a"),
                    CreateClass("SomeType")),
                cs: "(a) is SomeType",
                csSimple: "a is SomeType");
        }

        [Fact]
        public void TestAsExpression()
        {
            Test(
                f => f.TryCastExpression(
                    f.IdentifierName("a"),
                    CreateClass("SomeType")),
                cs: "(a) as SomeType",
                csSimple: "a as SomeType");
        }

        [Fact]
        public void TestNotExpression()
        {
            Test(
                f => f.LogicalNotExpression(
                    f.IdentifierName("a")),
                cs: "!(a)",
                csSimple: "!a");
        }

        [Fact]
        public void TestCastExpression()
        {
            Test(
                f => f.CastExpression(
                    CreateClass("SomeType"),
                    f.IdentifierName("a")),
                cs: "(SomeType)(a)",
                csSimple: "(SomeType)a");
        }

        [Fact]
        public void TestNegateExpression()
        {
            Test(
                f => f.NegateExpression(
                    f.IdentifierName("a")),
                cs: "-(a)",
                csSimple: "-a");
        }
    }
}
