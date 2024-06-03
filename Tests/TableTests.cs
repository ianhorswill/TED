using TED;
using TED.Tables;
using static TED.Language;

namespace Tests
{
    [TestClass]
    public class TableTests
    {
        [TestMethod]
        public void TableAdd()
        {
            var t = new Table<uint>();
            for (var i = 0u; i < 100; i++)
                t.Add(i);
            for (var i = 0u; i < 100; i++)
            {
                Assert.AreEqual(i, t[i]);
                Assert.AreEqual(i, t.PositionReference(i));
            }
        }

        [TestMethod]
        public void SuppressDuplicatesTest1()
        {
            var t = new Table<string>() { Unique = true };
            t.Add("foo");
            Assert.AreEqual("foo", t[0]);
            Assert.AreEqual(1u, t.Length);
            t.Add("foo");
            Assert.AreEqual(1u, t.Length);
            t.Add("bar");
            Assert.AreEqual(2u, t.Length);
            Assert.AreEqual("bar", t[1]);
        }

        [TestMethod]
        public void SuppressDuplicatesTest2()
        {
            // Left shift by j places; this forces clustering for higher values of j
            // thus exercising the search
            for (int j = 0; j < 5; j++)
            {
                var t = new Table<uint>() { Unique = true };
                for (var i = 0u; i < 50; i++)
                    t.Add(i<<j);
                Assert.AreEqual(50u, t.Length);
                for (var i = 0u; i < 100; i++)
                    t.Add(i<<j);
                Assert.AreEqual(100u, t.Length);
                for (var i = 0u; i < 100; i++)
                {
                    Assert.AreEqual(i<<j, t[i]);
                    Assert.AreEqual(i<<j, t.PositionReference(i));
                }
            }
        }

        [TestMethod]
        public void ProbeTest()
        {
            var t = new Table<int>() { Unique = true };
            for (var i = 0; i < 100; i++)
                t.Add(i<<2);
            for (var i = 0; i < 400; i++)
            {
                Assert.AreEqual((i&3)==0, t.ContainsRowUsingRowSet(i));
            }
        }

        [TestMethod]
        public void FullTableProbeTest()
        {
            var t = new Table<int>() { Unique = true };
            for (var i = 0; i < 1025; i++)
            {
                t.Add(i<<2);
                Assert.IsFalse(t.ContainsRowUsingRowSet(-1));
                Assert.IsTrue(t.ContainsRowUsingRowSet(i<<2));
            }
        }

        [TestMethod]
        public void KeyIndexTest()
        {
            var t = new Table<int>();
            var index = new KeyIndex<int, int>(null!, t, new [] { 0 }, (in int n)=>n);
            t.AddIndex(index);
            for (var i = 0; i < 1025; i++)
            {
                var value = i << 2;
                t.Add(value);
            }
            for (var i = 0u; i < 1025; i++)
            {
                var value = i << 2;
                Assert.AreEqual(i, index.RowWithKey((int)value));
            }
            Assert.AreEqual(Table.NoRow, index.RowWithKey(-1));
        }

        [TestMethod, ExpectedException(typeof(DuplicateKeyException))]
        public void DuplicateKeyTest()
        {
            var t = new Table<int>();
            var index = new KeyIndex<int, int>(null!, t, new [] { 0 }, (in int n)=>n);
            t.AddIndex(index);
            t.Add(0);
            t.Add(0);
        }

        [TestMethod]
        public void GeneralIndexTest()
        {
            var n = (Var<int>)"n";
            var p = new TablePredicate<int>("p", n);
            var t = p.Table;
            var index = new GeneralIndex<int, int>(p, t, new[] { 0 }, (in int n)=>n);
            t.AddIndex(index);
            for (var i = 0; i < 1025; i++)
            {
                var value = i << 2;
                t.Add(value);
            }
            for (var i = 0u; i < 1025; i++)
            {
                var value = i << 2;
                Assert.AreEqual(i, index.FirstRowWithValue((int)value));
                Assert.AreEqual(Table.NoRow, index.NextRowWithValue(i));
            }
            Assert.AreEqual(Table.NoRow, index.FirstRowWithValue(-1));
            for (var i = 0; i < 1025; i++)
            {
                var value = i << 2;
                t.Add(value);
            }
            for (var i = 0u; i < 1025; i++)
            {
                var value = i << 2;
                var first = index.FirstRowWithValue((int)value);
                Assert.AreEqual(i+1025, first);
                var next = index.NextRowWithValue(first);
                Assert.AreEqual(i, next);
                Assert.AreEqual(Table.NoRow, index.NextRowWithValue(next));
            }
        }

        [TestMethod]
        public void GeneralIndexSamTest()
        {
            var s = new Simulation(nameof(GeneralIndexSamTest));
            s.BeginPredicates();
            var location = (Var<string>)"location";
            var actionType = (Var<int>)"actionType";
            var person = (Var<string>)"person";
            var count = (Var<int>)"count";
            var state = (Var<bool>)"state";

            var Names = Predicate("Names", person);
            Names.AddRows(new [] {"Sam", "Ian", "Rob", "Jacob", "Mercedes", "Sofia", "Suri"});
            var Locations = Predicate("Locations", location);
            Locations.AddRows(new [] {"Store", "Park", "Ride"});
            var LocationActionType = Predicate("LocationActionType", location.Key, actionType.Indexed);
            LocationActionType.AddRows(new [] {("Store", 1), ("Park", 2), ("Ride", 3)});
            var ActionTypeInteractionTime = Predicate("ActionTypeInteractionTime", actionType.Key, count);
            ActionTypeInteractionTime.AddRows(new [] {(1, 60), (2, 30), (3, 40)});

            var NewPerson = Predicate("NewPerson", person).If(Prob[0.5f], RandomElement(Names, person));
            var People = Predicate("People", person);
            People.Add.If(NewPerson);

            var PersonActionAt = Predicate("PersonActionAt", person.Key, actionType.Indexed, location.Indexed);
            PersonActionAt.Overwrite = true;
            PersonActionAt.Add[person, 0, location].If(NewPerson, RandomElement(Locations, location));

            var PersonMovingTo = Predicate("PersonMoving", person.Key, location.Indexed)
                .If(PersonActionAt[person, 0, location]);
            var ArrivedAtDestination = Predicate("ArrivedAtDestination", person)
                .If(PersonMovingTo[person, location]);

            var Decr = new Function<int, int>("Decr", i => i - 1);

            var ActionTimer = Predicate("ActionTimer", person.Key, count, state.Indexed);
            ActionTimer.Overwrite = true;
            ActionTimer.Add[person, count, true].If(ArrivedAtDestination, PersonMovingTo, LocationActionType, ActionTypeInteractionTime[actionType, count]);

            ActionTimer.Set(person, count, Decr[count]).If(ActionTimer, count >= 0);
            ActionTimer.Set(person, state, false).If(ActionTimer[person, 0, true]);

            var NotDoingAction = Definition("NotDoingAction", person).Is(!ActionTimer[person, __, true]);

            var ReadyToSelectAction = Predicate("ReadyToSelectAction", person).If(People[person], !PersonMovingTo[person, __], NotDoingAction[person]);
            var Running = Predicate("Running", person).If(ActionTimer[person, __, true]);
            s.EndPredicates();
            for (var i = 0; i < 400; i++)
                s.Update();
        }

        enum DayOfWeek
        {
            // ReSharper disable UnusedMember.Local
            Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday
            // ReSharper restore UnusedMember.Local
        };

        [TestMethod]
        public void GeneralIndexEnumeratedType()
        {
            var n = (Var<int>)"n";
            var dow = (Var<DayOfWeek>)"dow";
            var p = new TablePredicate<int, DayOfWeek>("p", n, dow);
            var t = p.Table;
            var index = new GeneralIndex<(int, DayOfWeek), DayOfWeek>(p, t, new[] { 0 }, (in (int, DayOfWeek) r)=>r.Item2);
            t.AddIndex(index);
            for (var i = 0; i < 1025; i++)
            {
                for (DayOfWeek d = DayOfWeek.Monday;d <= DayOfWeek.Sunday; d++)
                     t.Add((i,d));
            }
            Assert.AreEqual(7, index.Keys.Count());
        }

        [TestMethod]
        public void KeyCounts()
        {
            var n = (Var<int>)"n";
            var p = new TablePredicate<int>("p", n);
            var t = p.Table;
            var index = new GeneralIndex<int, int>(p, t, new[] { 0 }, (in int n)=>n);
            t.AddIndex(index);
            for (var i = 0; i < 100; i++)
            {
                for (var j = 0; j<i; j++)
                    t.Add((i+5));
            }
            foreach (var (k, i) in index.CountsByKey)
                Assert.AreEqual(k, i+5);
            Assert.AreEqual(99, index.CountsByKey.Count());
        }

        [TestMethod]
        public void RowsMatchingTest()
        {
            var i = (Var<int>)"i";
            var parity = (Var<int>)"parity";
            var t = Predicate("t", i, parity.Indexed);
            for (var j = 0; j < 10; j++)
                t.AddRow(j, j%2);
            var index = (GeneralIndex<(int,int), int>)t.IndexFor(parity, false)!;
            var rows = index.RowsMatching(0).Select(r => r.Item1).ToArray();
            CollectionAssert.AreEqual(new[] { 8,6,4,2,0 }, rows);
            CollectionAssert.AreEqual(new[] { 9,7,5,3,1 }, index.RowsMatching(1).Select(r => r.Item1).ToArray());
            CollectionAssert.AreEqual(Array.Empty<int>(), index.RowsMatching(2).Select(r => r.Item1).ToArray());
        }

        [TestMethod]
        public void IndexDeletionTest()
        {

            var n = (Var<int>)"n";
            var p = new TablePredicate<int>("p", n);
            var t = p.Table;
            var index = new GeneralIndex<int, int>(p, t, new[] { 0 }, (in int n)=>n);
            t.AddIndex(index);
            index.EnableMutation();

            IEnumerable<uint> RowsWithValue(int v)
            {
                for (var row = index.FirstRowWithValue(v); Table.ValidRow(row); row = index.NextRowWithValue(row))
                    yield return row;
            }

            for (var i = 0; i < 10; i++)
                t.Add(0);

            void CheckRows(uint[] expectedRows)
            {
                var rows = RowsWithValue(0).ToArray();
                CollectionAssert.AreEqual(expectedRows, rows);
                Assert.AreEqual(expectedRows.Length, index.RowsMatchingCount(0));
            }

            CheckRows(new uint[] { 9,8,7,6,5,4,3,2,1,0 });

            // Test removing the first element
            index.Remove(9);
            CheckRows(new uint[] { 8,7,6,5,4,3,2,1,0 });

            // Test removing the last element
            index.Remove(0);
            CheckRows(new uint[] { 8, 7, 6, 5, 4, 3, 2, 1 });

            // Test removing a middle element
            index.Remove(5);
            CheckRows(new uint[] { 8, 7, 6, 4, 3, 2, 1 });

            // Test removing all elements
            foreach (var row in RowsWithValue(0)) index.Remove(row);
            Assert.AreEqual(0, RowsWithValue(0).Count());

            // Test adding rows back in
            for (var i = 0u; i < 10u; i++)
                index.Add(i);

            CheckRows(new uint[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 });
        }

        [TestMethod]
        public void MutatorUnindexed()
        {
            var s = (Var<string>)"s";
            var n = (Var<int>)"n";

            var T = Predicate("T", s.Key, n);
            T.AddRow("foo", 1);
            T.AddRow("bar", 2);
            var a = T.Accessor(s, n);
            Assert.AreEqual(1, a["foo"]);
            Assert.AreEqual(2, a["bar"]);
            a["foo"] = 3;
            Assert.AreEqual(3, a["foo"]);
        }

        [TestMethod]
        public void MutatorIndexed()
        {
            var s = (Var<string>)"s";
            var n = (Var<int>)"n";

            var T = Predicate("T", s.Key, n.Indexed);
            var index = (GeneralIndex<(string,int), int>)T.IndexFor(1, false)!;

            IEnumerable<uint> RowsWithValue(int v)
            {
                for (var row = index.FirstRowWithValue(v); Table.ValidRow(row); row = index.NextRowWithValue(row))
                    yield return row;
            }
            T.AddRow("foo", 1);
            T.AddRow("bar", 2);
            var a = T.Accessor(s, n);
            Assert.AreEqual(1, a["foo"]);
            Assert.AreEqual(2, a["bar"]);
            a["foo"] = 3;
            Assert.AreEqual(3, a["foo"]);
            CollectionAssert.AreEqual(new uint[] {0}, RowsWithValue(3).ToArray());

            a["foo"] = 2;
            CollectionAssert.AreEqual(new uint[] {0, 1}, RowsWithValue(2).ToArray());
            Assert.AreEqual(0, RowsWithValue(3).Count());
        }

        [TestMethod]
        public void UserKeyIndexTest()
        {
            var n = (Var<int>)"n";
            var m = (Var<int>)"m";
            var nums = new[] { 1, 2, 3, 4, 5, 6 };
            // ReSharper disable once InconsistentNaming
            var Table = Predicate("Table", nums.Select(i => (i, i+1)), n.Key, m);
            var nKey = Table.KeyIndex(n);
            foreach (var i in nums)
                Assert.AreEqual(i+1, nKey[i].Item2);
        }

        [TestMethod]
        public void ReclamationTest()
        {
            var t = new Table<(int, bool)>() { ReclaimRowTest = (in (int, bool) row) => !row.Item2 };
            // Add rows for numbers from 0 to 99, but mark odd numbers for reclamation
            for (var i = 0; i < 100; i++)
                t.Add((i, i%2 == 0));
            // Make sure at least the first 50 entries are compacted.
            var counter = 0;
            foreach (var pair in t)
            {
                if (pair != (counter, true))
                {
                    Assert.IsTrue(counter > 50);
                    break;
                }
                counter += 2;
            }
            // Now force compact the rest
            t.Reclaim();
            Assert.AreEqual(50u, t.Length);
            counter = 0;
            foreach (var pair in t)
            {
                Assert.AreEqual((counter, true), pair);
                counter += 2;
            }
            // Now just to be paranoid, make sure that reclaiming a table with no reclaimable rows doesn't do anything bad.
            t.Reclaim();
            Assert.AreEqual(50u, t.Length);
            counter = 0;
            foreach (var pair in t)
            {
                Assert.AreEqual((counter, true), pair);
                counter += 2;
            }
        }
    }
}