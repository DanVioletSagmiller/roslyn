// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.CodeGeneration
{
    [Trait(Traits.Feature, Traits.Features.CodeGeneration)]
    public class ExpressionPrecedenceGenerationTests : AbstractCodeGenerationTests
    {
        [Fact]
        public void TestAddMultiplyPrecedence1()
        {
            Test(
                f => f.MultiplyExpression(
                    f.AddExpression(
                        f.LiteralExpression(1),
                        f.LiteralExpression(2)),
                    f.LiteralExpression(3)),
                cs: "((1) + (2)) * (3)",
                csSimple: "(1 + 2) * 3");
        }

        [Fact]
        public void TestAddMultiplyPrecedence2()
        {
            Test(
                f => f.AddExpression(
                    f.MultiplyExpression(
                        f.LiteralExpression(1),
                        f.LiteralExpression(2)),
                    f.LiteralExpression(3)),
                cs: "((1) * (2)) + (3)",
                csSimple: "1 * 2 + 3");
        }

        [Fact]
        public void TestAddMultiplyPrecedence3()
        {
            Test(
                f => f.MultiplyExpression(
                    f.LiteralExpression(1),
                    f.AddExpression(
                        f.LiteralExpression(2),
                        f.LiteralExpression(3))),
                cs: "(1) * ((2) + (3))",
                csSimple: "1 * (2 + 3)");
        }

        [Fact]
        public void TestAddMultiplyPrecedence4()
        {
            Test(
                f => f.AddExpression(
                    f.LiteralExpression(1),
                    f.MultiplyExpression(
                        f.LiteralExpression(2),
                        f.LiteralExpression(3))),
                cs: "(1) + ((2) * (3))",
                csSimple: "1 + 2 * 3");
        }

        [Fact]
        public void TestBitwiseAndOrPrecedence1()
        {
            Test(
                f => f.BitwiseAndExpression(
                    f.BitwiseOrExpression(
                        f.LiteralExpression(1),
                        f.LiteralExpression(2)),
                    f.LiteralExpression(3)),
                cs: "((1) | (2)) & (3)",
                csSimple: "(1 | 2) & 3");
        }

        [Fact]
        public void TestBitwiseAndOrPrecedence2()
        {
            Test(
                f => f.BitwiseOrExpression(
                    f.BitwiseAndExpression(
                        f.LiteralExpression(1),
                        f.LiteralExpression(2)),
                    f.LiteralExpression(3)),
                cs: "((1) & (2)) | (3)",
                csSimple: "1 & 2 | 3");
        }

        [Fact]
        public void TestBitwiseAndOrPrecedence3()
        {
            Test(
                f => f.BitwiseAndExpression(
                    f.LiteralExpression(1),
                    f.BitwiseOrExpression(
                        f.LiteralExpression(2),
                        f.LiteralExpression(3))),
                cs: "(1) & ((2) | (3))",
                csSimple: "1 & (2 | 3)");
        }

        [Fact]
        public void TestBitwiseAndOrPrecedence4()
        {
            Test(
                f => f.BitwiseOrExpression(
                    f.LiteralExpression(1),
                    f.BitwiseAndExpression(
                        f.LiteralExpression(2),
                        f.LiteralExpression(3))),
                cs: "(1) | ((2) & (3))",
                csSimple: "1 | 2 & 3");
        }

        [Fact]
        public void TestLogicalAndOrPrecedence1()
        {
            Test(
                f => f.LogicalAndExpression(
                    f.LogicalOrExpression(
                        f.LiteralExpression(1),
                        f.LiteralExpression(2)),
                    f.LiteralExpression(3)),
                cs: "((1) || (2)) && (3)",
                csSimple: "(1 || 2) && 3");
        }

        [Fact]
        public void TestLogicalAndOrPrecedence2()
        {
            Test(
                f => f.LogicalOrExpression(
                    f.LogicalAndExpression(
                        f.LiteralExpression(1),
                        f.LiteralExpression(2)),
                    f.LiteralExpression(3)),
                cs: "((1) && (2)) || (3)",
                csSimple: "1 && 2 || 3");
        }

        [Fact]
        public void TestLogicalAndOrPrecedence3()
        {
            Test(
                f => f.LogicalAndExpression(
                    f.LiteralExpression(1),
                    f.LogicalOrExpression(
                        f.LiteralExpression(2),
                        f.LiteralExpression(3))),
                cs: "(1) && ((2) || (3))",
                csSimple: "1 && (2 || 3)");
        }

        [Fact]
        public void TestLogicalAndOrPrecedence4()
        {
            Test(
                f => f.LogicalOrExpression(
                    f.LiteralExpression(1),
                    f.LogicalAndExpression(
                        f.LiteralExpression(2),
                        f.LiteralExpression(3))),
                cs: "(1) || ((2) && (3))",
                csSimple: "1 || 2 && 3");
        }

        [Fact]
        public void TestMemberAccessOffOfAdd1()
        {
            Test(
                f => f.MemberAccessExpression(
                    f.AddExpression(
                        f.LiteralExpression(1),
                        f.LiteralExpression(2)),
                    f.IdentifierName("M")),
                cs: "((1) + (2)).M",
                csSimple: "(1 + 2).M");
        }

        [Fact]
        public void TestConditionalExpression1()
        {
            Test(
                f => f.ConditionalExpression(
                    f.AssignmentStatement(
                        f.IdentifierName("E1"),
                        f.IdentifierName("E2")),
                    f.IdentifierName("T"),
                    f.IdentifierName("F")),
                cs: "(E1 = (E2)) ? (T) : (F)",
                csSimple: "(E1 = E2) ? T : F");
        }

        [Fact]
        public void TestConditionalExpression2()
        {
            Test(
                f => f.AddExpression(
                        f.ConditionalExpression(
                            f.IdentifierName("E1"),
                            f.IdentifierName("T1"),
                            f.IdentifierName("F1")),
                        f.ConditionalExpression(
                            f.IdentifierName("E2"),
                            f.IdentifierName("T2"),
                            f.IdentifierName("F2"))),
                cs: "((E1) ? (T1) : (F1)) + ((E2) ? (T2) : (F2))",
                csSimple: "(E1 ? T1 : F1) + (E2 ? T2 : F2)");
        }

        [Fact]
        public void TestMemberAccessOffOfElementAccess()
        {
            Test(
                f => f.ElementAccessExpression(
                    f.AddExpression(
                        f.LiteralExpression(1),
                        f.LiteralExpression(2)),
                    f.Argument(f.IdentifierName("M"))),
                cs: "((1) + (2))[M]",
                csSimple: "(1 + 2)[M]");
        }

        [Fact]
        public void TestMemberAccessOffOfIsExpression()
        {
            Test(
                f => f.MemberAccessExpression(
                    f.IsTypeExpression(
                        f.IdentifierName("a"),
                        CreateClass("SomeType")),
                    f.IdentifierName("M")),
                cs: "((a) is SomeType).M",
                csSimple: "(a is SomeType).M");
        }

        [Fact]
        public void TestIsOfMemberAccessExpression()
        {
            Test(
                f => f.IsTypeExpression(
                    f.MemberAccessExpression(
                        f.IdentifierName("a"),
                        f.IdentifierName("M")),
                    CreateClass("SomeType")),
                cs: "(a.M) is SomeType",
                csSimple: "a.M is SomeType");
        }

        [Fact]
        public void TestMemberAccessOffOfAsExpression()
        {
            Test(
                f => f.MemberAccessExpression(
                    f.TryCastExpression(
                        f.IdentifierName("a"),
                        CreateClass("SomeType")),
                    f.IdentifierName("M")),
                cs: "((a) as SomeType).M",
                csSimple: "(a as SomeType).M");
        }

        [Fact]
        public void TestAsOfMemberAccessExpression()
        {
            Test(
                f => f.TryCastExpression(
                         f.MemberAccessExpression(
                            f.IdentifierName("a"),
                            f.IdentifierName("M")),
                        CreateClass("SomeType")),
                cs: "(a.M) as SomeType",
                csSimple: "a.M as SomeType");
        }

        [Fact]
        public void TestMemberAccessOffOfNotExpression()
        {
            Test(
                f => f.MemberAccessExpression(
                    f.LogicalNotExpression(
                        f.IdentifierName("a")),
                    f.IdentifierName("M")),
                cs: "(!(a)).M",
                csSimple: "(!a).M");
        }

        [Fact]
        public void TestNotOfMemberAccessExpression()
        {
            Test(
                f => f.LogicalNotExpression(
                    f.MemberAccessExpression(
                        f.IdentifierName("a"),
                        f.IdentifierName("M"))),
                cs: "!(a.M)",
                csSimple: "!a.M");
        }

        [Fact]
        public void TestMemberAccessOffOfCastExpression()
        {
            Test(
                f => f.MemberAccessExpression(
                    f.CastExpression(
                        CreateClass("SomeType"),
                        f.IdentifierName("a")),
                    f.IdentifierName("M")),
                cs: "((SomeType)(a)).M",
                csSimple: "((SomeType)a).M");
        }

        [Fact]
        public void TestCastOfAddExpression()
        {
            Test(
                f => f.CastExpression(
                    CreateClass("SomeType"),
                    f.AddExpression(
                        f.IdentifierName("a"),
                        f.IdentifierName("b"))),
                cs: "(SomeType)((a) + (b))",
                csSimple: "(SomeType)(a + b)");
        }

        [Fact]
        public void TestNegateOfAddExpression()
        {
            Test(
                f => f.NegateExpression(
                    f.AddExpression(
                        f.IdentifierName("a"),
                        f.IdentifierName("b"))),
                cs: "-((a) + (b))",
                csSimple: "-(a + b)");
        }

        [Fact]
        public void TestMemberAccessOffOfNegate()
        {
            Test(
                f => f.MemberAccessExpression(
                    f.NegateExpression(
                        f.IdentifierName("a")),
                    f.IdentifierName("M")),
                cs: "(-(a)).M",
                csSimple: "(-a).M");
        }

        [Fact]
        public void TestNegateOfMemberAccess()
        {
            Test(f =>
                f.NegateExpression(
                    f.MemberAccessExpression(
                        f.IdentifierName("a"),
                        f.IdentifierName("M"))),
                cs: "-(a.M)",
                csSimple: "-a.M");
        }
    }
}
