namespace dotless.Test.Specs
{
    using System;
    using System.Globalization;
    using System.Threading;
    using NUnit.Framework;

    public class MixinGuardsFixture : SpecFixtureBase
    {
        [Test]
        public void StackingFunctions()
        {
            var input =
                @"
.light (@a) when (lightness(@a) > 50%) {
  color: white;
}
.light (@a) when (lightness(@a) < 50%) {
  color: black;
}
.light (@a) {
  margin: 1px;
}

.light1 { .light(#ddd) }
.light2 { .light(#444) }
";

            var expected =
                @"
.light1 {
  color: white;
  margin: 1px;
}
.light2 {
  color: black;
  margin: 1px;
}";

            AssertLess(input, expected);
        }

        [Test]
        public void ArgumentsAgainstEachOther()
        {
            var input =
                @"
.max (@a, @b) when (@a > @b) {
  width: @a;
}
.max (@a, @b) when (@a < @b) {
  width: @b;
}

.max1 { .max(3, 6) }
.max2 { .max(8, 1) }
";

            var expected =
                @"
.max1 {
  width: 6;
}
.max2 {
  width: 8;
}";

            AssertLess(input, expected);
        }

        [Test]
        public void GlobalsInsideGuardsPositive()
        {
            var input =
                @"
@g: auto;

.glob (@a) when (@a = @g) {
  margin: @a @g;
}
.glob1 { .glob(auto) }
";

            var expected =
                @"
.glob1 {
  margin: auto auto;
}";

            AssertLess(input, expected);
        }

        [Test]
        public void GlobalsInsideGuardsNegative()
        {
            var input =
                @"
@g: auto;

.glob (@a) when (@a = @g) {
  margin: @a @g;
}

.glob2 { .glob(default) }";

            var expected = @"";

            AssertLess(input, expected);
        }

        [Test]
        public void OtherOperators()
        {
            var input =
                @"
.ops (@a) when (@a >= 0) {
  height: gt-or-eq;
}
.ops (@a) when (@a =< 0) {
  height: lt-or-eq;
}
.ops (@a) when not(@a = 0) {
  height: not-eq;
}
.ops1 { .ops(0) }
.ops2 { .ops(1) }
.ops3 { .ops(-1) }";

            var expected =
                @"
.ops1 {
  height: gt-or-eq;
  height: lt-or-eq;
}
.ops2 {
  height: gt-or-eq;
  height: not-eq;
}
.ops3 {
  height: lt-or-eq;
  height: not-eq;
}";

            AssertLess(input, expected);
        }

        [Test]
        public void ScopeAndDefaultValuesPositive()
        {
            var input =
                @"
@a: auto;

.default (@a: inherit) when (@a = inherit) {
  content: default;
}
.default1 { .default }
";

            var expected =
                @"
.default1 {
  content: default;
}
";

            AssertLess(input, expected);
        }

        [Test]
        public void ScopeAndDefaultValuesNegative()
        {
            var input =
                @"
@a: auto;

.default (@a: inherit) when (@a = inherit) {
  content: default;
}
.default2 { .default(@a) }
";

            var expected = @"";

            AssertLess(input, expected);
        }

        [Test]
        public void TrueAndFalse()
        {
            var input = @"
.test (@a) when (@a) {
    content: ""true."";
}
.test (@a) when not (@a) {
    content: ""false."";
}

.test1 { .test(true) }
.test2 { .test(false) }
.test3 { .test(1) }
.test4 { .test(boo) }
.test5 { .test(""true"") }
";
            var expected = @"
.test1 {
  content: ""true."";
}
.test2 {
  content: ""false."";
}
.test3 {
  content: ""false."";
}
.test4 {
  content: ""false."";
}
.test5 {
  content: ""false."";
}
";
            AssertLess(input, expected);
        }

        [Test]
        public void BooleanExpressions()
        {
            var input =
                @"
.bool () when (true) and (false)                             { content: true and false } // FALSE
.bool () when (true) and (true)                              { content: true and true } // TRUE
.bool () when (true)                                         { content: true } // TRUE
.bool () when (false) and (false)                            { content: true } // FALSE
.bool () when (false), (true)                                { content: false, true } // TRUE
.bool () when (false) and (true) and (true),  (true)         { content: false and true and true, true } // TRUE
.bool () when (true)  and (true) and (false), (false)        { content: true and true and false, false } // FALSE
.bool () when (false), (true) and (true)                     { content: false, true and true } // TRUE
.bool () when (false), (false), (true)                       { content: false, false, true } // TRUE
.bool () when (false), (false) and (true), (false)           { content: false, false and true, false } // FALSE
.bool () when (false), (true) and (true) and (true), (false) { content: false, true and true and true, false } // TRUE
.bool () when not (false)                                    { content: not false }
.bool () when not (true) and not (false)                     { content: not true and not false }
.bool () when not (true) and not (true)                      { content: not true and not true }
.bool () when not (false) and (false), not (false)           { content: not false and false, not false }

.bool1 { .bool }";

            var expected =
                @"
.bool1 {
  content: true and true;
  content: true;
  content: false, true;
  content: false and true and true, true;
  content: false, true and true;
  content: false, false, true;
  content: false, true and true and true, false;
  content: not false;
  content: not false and false, not false;
}";

            AssertLess(input, expected);
        }
    }
}