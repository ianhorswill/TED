using TED;
using TED.Interpreter;
using static TED.Language;

namespace Tests
{
    [TestClass]
    public class SyntaxExtensionTests
    {
        [TestMethod]
        public void ChooseTest()
        {
            var p = new Program(nameof(ChooseTest));
            p.BeginPredicates();
            var i = (Var<int>)"i";
            var j = (Var<int>)"j";
            var A = Predicate("A", i);
            var B = Predicate("B", i);
            var C = Predicate("C", i);
            var D = Predicate("D", i);
            p.EndPredicates();

            var expansion = ((Goal)Choose(A, B).Or(C, D)).ToString();
            Assert.AreEqual("FirstOf[And[A[i], B[i]], And[C[i], D[i]]]", expansion);
        }

        [TestMethod]
        public void EitherTest()
        {
            var p = new Program(nameof(ChooseTest));
            p.BeginPredicates();
            var i = (Var<int>)"i";
            var j = (Var<int>)"j";
            var A = Predicate("A", i);
            var B = Predicate("B", i);
            var C = Predicate("C", i);
            var D = Predicate("D", i);
            p.EndPredicates();

            var expansion = ((Goal)Either(A, B).Or(C, D)).ToString();
            Assert.AreEqual("Or[And[A[i], B[i]], And[C[i], D[i]]]", expansion);
        }
    }
}