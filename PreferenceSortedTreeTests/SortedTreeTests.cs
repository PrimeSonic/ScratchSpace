namespace ScratchProjects.Tests
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using Sorting;

    [TestFixture]
    public class SortedTreeTests
    {
        private class TestDependencies : ISortable<string>
        {
            public TestDependencies(string id)
            {
                this.Id = id;
            }

            public TestDependencies(string id, params string[] dependencies)
               : this(id)
            {
                foreach (string dependency in dependencies)
                    this.Dependencies.Add(dependency);
            }

            public string Id { get; }

            public IList<string> Dependencies { get; } = new List<string>();

            public IList<string> LoadBefore { get; } = new List<string>();

            public IList<string> LoadAfter { get; } = new List<string>();
        }

        private class TestLoadBefore : ISortable<string>
        {
            public TestLoadBefore(string id)
            {
                this.Id = id;
            }

            public TestLoadBefore(string id, params string[] loadBefore)
               : this(id)
            {
                foreach (string before in loadBefore)
                    this.LoadBefore.Add(before);
            }

            public string Id { get; }

            public IList<string> Dependencies { get; } = new List<string>();

            public IList<string> LoadBefore { get; } = new List<string>();

            public IList<string> LoadAfter { get; } = new List<string>();
        }

        [Test]
        public void CleanRedundantDependencies_SingleRedundancy_RedundanciesCleared()
        {
            var rng = new Random();
            const string innerDependency = "mostUsedDependency";
            const string outterDependency = "redundant";
            var entries = new List<TestDependencies>
            {
                new TestDependencies(innerDependency),
                new TestDependencies(outterDependency, innerDependency),
                new TestDependencies("commonEntry1", innerDependency),
                new TestDependencies("commonEntry2", innerDependency, outterDependency),
                new TestDependencies("commonEntry3", innerDependency, outterDependency),
                new TestDependencies("commonEntry4"),
                new TestDependencies("commonEntry5", innerDependency, outterDependency),
                new TestDependencies("commonEntry6", innerDependency, outterDependency),
                new TestDependencies("commonEntry7", innerDependency),
            };
            int originalCount = entries.Count;
            var tree = new SortedCollection<string, TestDependencies>();

            // Add entries in random order
            while (entries.Count > 0)
            {
                int rngSelected = rng.Next(0, entries.Count);
                Assert.IsTrue(tree.AddSorted(entries[rngSelected]));
                entries.RemoveAt(rngSelected);
            }

            Assert.AreEqual(originalCount, tree.Count);

            tree.CleanRedundantDependencies();

            Assert.AreEqual(originalCount, tree.Count);
            Assert.AreEqual(4, tree.DependencyUsedBy(outterDependency));
            Assert.AreEqual(3, tree.DependencyUsedBy(innerDependency));

            Assert.IsTrue(tree[outterDependency].Dependencies.Contains(innerDependency));

            Assert.IsTrue(tree["commonEntry1"].Dependencies.Contains(innerDependency));
            Assert.IsTrue(tree["commonEntry2"].Dependencies.Contains(outterDependency));
            Assert.IsTrue(tree["commonEntry3"].Dependencies.Contains(outterDependency));

            Assert.IsTrue(tree["commonEntry5"].Dependencies.Contains(outterDependency));
            Assert.IsTrue(tree["commonEntry6"].Dependencies.Contains(outterDependency));
            Assert.IsTrue(tree["commonEntry7"].Dependencies.Contains(innerDependency));

            Assert.IsFalse(tree["commonEntry2"].Dependencies.Contains(innerDependency));
            Assert.IsFalse(tree["commonEntry3"].Dependencies.Contains(innerDependency));

            Assert.IsFalse(tree["commonEntry5"].Dependencies.Contains(innerDependency));
            Assert.IsFalse(tree["commonEntry6"].Dependencies.Contains(innerDependency));
        }

        [Test]
        public void CleanRedundantDependencies_MultipleRedundancy_RedundanciesCleared()
        {
            var rng = new Random();
            const string innerDependency = "mostUsedDependency";
            const string outterDep1 = "redundant1";
            const string outterDep2 = "redundant2";
            const string outterDep3 = "redundant3";
            var entries = new List<TestDependencies>
            {
                new TestDependencies(innerDependency),
                new TestDependencies(outterDep1, innerDependency),
                new TestDependencies(outterDep2, innerDependency, outterDep1),
                new TestDependencies(outterDep3, outterDep2, innerDependency),
                new TestDependencies("commonEntry1", innerDependency),
                new TestDependencies("commonEntry1a", outterDep2, innerDependency),
                new TestDependencies("commonEntry2", innerDependency, outterDep1),
                new TestDependencies("commonEntry2a", innerDependency, outterDep1, outterDep2),
                new TestDependencies("commonEntry3", innerDependency, outterDep2, outterDep3, outterDep1),
                new TestDependencies("commonEntry3a", innerDependency, outterDep1),
                new TestDependencies("commonEntry4"),
                new TestDependencies("commonEntry5", innerDependency, outterDep2),
                new TestDependencies("commonEntry5a", innerDependency, outterDep1, outterDep2, outterDep3),
                new TestDependencies("commonEntry6", innerDependency, outterDep1),
                new TestDependencies("commonEntry6a", innerDependency, outterDep2, outterDep1),
                new TestDependencies("commonEntry7", innerDependency),
            };
            int originalCount = entries.Count;
            var tree = new SortedCollection<string, TestDependencies>();

            // Add entries in random order
            while (entries.Count > 0)
            {
                int rngSelected = rng.Next(0, entries.Count);
                Assert.IsTrue(tree.AddSorted(entries[rngSelected]));
                entries.RemoveAt(rngSelected);
            }

            Assert.AreEqual(originalCount, tree.Count);

            tree.CleanRedundantDependencies();

            Assert.AreEqual(originalCount, tree.Count);

            Assert.IsTrue(tree[outterDep1].Dependencies.Contains(innerDependency));
            Assert.IsTrue(tree[outterDep2].Dependencies.Contains(outterDep1));
            Assert.IsTrue(tree[outterDep3].Dependencies.Contains(outterDep2));

            Assert.IsFalse(tree["commonEntry1a"].Dependencies.Contains(innerDependency));

            Assert.IsFalse(tree["commonEntry2"].Dependencies.Contains(innerDependency));

            Assert.IsFalse(tree["commonEntry2a"].Dependencies.Contains(innerDependency));
            Assert.IsFalse(tree["commonEntry2a"].Dependencies.Contains(outterDep1));

            Assert.IsFalse(tree["commonEntry3"].Dependencies.Contains(innerDependency));
            Assert.IsFalse(tree["commonEntry3"].Dependencies.Contains(outterDep1));
            Assert.IsFalse(tree["commonEntry3"].Dependencies.Contains(outterDep2));

            Assert.IsFalse(tree["commonEntry3a"].Dependencies.Contains(innerDependency));

            Assert.IsFalse(tree["commonEntry5"].Dependencies.Contains(outterDep1));
            Assert.IsFalse(tree["commonEntry5"].Dependencies.Contains(innerDependency));

            Assert.IsFalse(tree["commonEntry5a"].Dependencies.Contains(innerDependency));
            Assert.IsFalse(tree["commonEntry5a"].Dependencies.Contains(outterDep1));
            Assert.IsFalse(tree["commonEntry5a"].Dependencies.Contains(outterDep2));

            Assert.IsFalse(tree["commonEntry6a"].Dependencies.Contains(innerDependency));
            Assert.IsFalse(tree["commonEntry6a"].Dependencies.Contains(outterDep1));

            Assert.AreEqual(4, tree.DependencyUsedBy(outterDep1));
            Assert.AreEqual(5, tree.DependencyUsedBy(outterDep2));
            Assert.AreEqual(2, tree.DependencyUsedBy(outterDep3));
            Assert.AreEqual(3, tree.DependencyUsedBy(innerDependency));
        }

        [Test]
        public void CleanRedundantLoadBefore_SingleRedundancy_RedundanciesCleared()
        {
            var rng = new Random();
            const string innerDependency = "mostUsedDependency";
            const string outterDependency = "redundant";
            var entries = new List<TestLoadBefore>
            {
                new TestLoadBefore(innerDependency),
                new TestLoadBefore(outterDependency, innerDependency),
                new TestLoadBefore("commonEntry1", innerDependency),
                new TestLoadBefore("commonEntry2", innerDependency, outterDependency),
                new TestLoadBefore("commonEntry3", innerDependency, outterDependency),
                new TestLoadBefore("commonEntry4"),
                new TestLoadBefore("commonEntry5", innerDependency, outterDependency),
                new TestLoadBefore("commonEntry6", innerDependency, outterDependency),
                new TestLoadBefore("commonEntry7", innerDependency),
            };
            int originalCount = entries.Count;
            var tree = new SortedCollection<string, TestLoadBefore>();

            // Add entries in random order
            while (entries.Count > 0)
            {
                int rngSelected = rng.Next(0, entries.Count);
                Assert.IsTrue(tree.AddSorted(entries[rngSelected]));
                entries.RemoveAt(rngSelected);
            }

            Assert.AreEqual(originalCount, tree.Count);

            tree.CleanRedundantLoadBefore();

            Assert.AreEqual(originalCount, tree.Count);

            Assert.IsTrue(tree[outterDependency].LoadBefore.Contains(innerDependency));

            Assert.IsTrue(tree["commonEntry1"].LoadBefore.Contains(innerDependency));
            Assert.IsTrue(tree["commonEntry2"].LoadBefore.Contains(outterDependency));
            Assert.IsTrue(tree["commonEntry3"].LoadBefore.Contains(outterDependency));

            Assert.IsTrue(tree["commonEntry5"].LoadBefore.Contains(outterDependency));
            Assert.IsTrue(tree["commonEntry6"].LoadBefore.Contains(outterDependency));
            Assert.IsTrue(tree["commonEntry7"].LoadBefore.Contains(innerDependency));

            Assert.IsFalse(tree["commonEntry2"].LoadBefore.Contains(innerDependency));
            Assert.IsFalse(tree["commonEntry3"].LoadBefore.Contains(innerDependency));

            Assert.IsFalse(tree["commonEntry5"].LoadBefore.Contains(innerDependency));
            Assert.IsFalse(tree["commonEntry6"].LoadBefore.Contains(innerDependency));
        }

        [Test]
        public void CleanRedundantLoadBefore_MultipleRedundancy_RedundanciesCleared()
        {
            var rng = new Random();
            const string innerDependency = "mostUsedDependency";
            const string outterDep1 = "redundant1";
            const string outterDep2 = "redundant2";
            const string outterDep3 = "redundant3";
            var entries = new List<TestLoadBefore>
            {
                new TestLoadBefore(innerDependency),
                new TestLoadBefore(outterDep1, innerDependency),
                new TestLoadBefore(outterDep2, innerDependency, outterDep1),
                new TestLoadBefore(outterDep3, outterDep2, innerDependency),
                new TestLoadBefore("commonEntry1", innerDependency),
                new TestLoadBefore("commonEntry1a", outterDep2, innerDependency),
                new TestLoadBefore("commonEntry2", innerDependency, outterDep1),
                new TestLoadBefore("commonEntry2a", innerDependency, outterDep1, outterDep2),
                new TestLoadBefore("commonEntry3", innerDependency, outterDep2, outterDep3, outterDep1),
                new TestLoadBefore("commonEntry3a", innerDependency, outterDep1),
                new TestLoadBefore("commonEntry4"),
                new TestLoadBefore("commonEntry5", innerDependency, outterDep2),
                new TestLoadBefore("commonEntry5a", innerDependency, outterDep1, outterDep2, outterDep3),
                new TestLoadBefore("commonEntry6", innerDependency, outterDep1),
                new TestLoadBefore("commonEntry6a", innerDependency, outterDep2, outterDep1),
                new TestLoadBefore("commonEntry7", innerDependency),
            };
            int originalCount = entries.Count;
            var tree = new SortedCollection<string, TestLoadBefore>();

            // Add entries in random order
            while (entries.Count > 0)
            {
                int rngSelected = rng.Next(0, entries.Count);
                Assert.IsTrue(tree.AddSorted(entries[rngSelected]));
                entries.RemoveAt(rngSelected);
            }

            Assert.AreEqual(originalCount, tree.Count);

            tree.CleanRedundantLoadBefore();

            Assert.AreEqual(originalCount, tree.Count);

            Assert.IsTrue(tree[outterDep1].LoadBefore.Contains(innerDependency));
            Assert.IsTrue(tree[outterDep2].LoadBefore.Contains(outterDep1));
            Assert.IsTrue(tree[outterDep3].LoadBefore.Contains(outterDep2));

            Assert.IsFalse(tree["commonEntry1a"].LoadBefore.Contains(innerDependency));

            Assert.IsFalse(tree["commonEntry2"].LoadBefore.Contains(innerDependency));

            Assert.IsFalse(tree["commonEntry2a"].LoadBefore.Contains(innerDependency));
            Assert.IsFalse(tree["commonEntry2a"].LoadBefore.Contains(outterDep1));

            Assert.IsFalse(tree["commonEntry3"].LoadBefore.Contains(innerDependency));
            Assert.IsFalse(tree["commonEntry3"].LoadBefore.Contains(outterDep1));
            Assert.IsFalse(tree["commonEntry3"].LoadBefore.Contains(outterDep2));

            Assert.IsFalse(tree["commonEntry3a"].LoadBefore.Contains(innerDependency));

            Assert.IsFalse(tree["commonEntry5"].LoadBefore.Contains(outterDep1));
            Assert.IsFalse(tree["commonEntry5"].LoadBefore.Contains(innerDependency));

            Assert.IsFalse(tree["commonEntry5a"].LoadBefore.Contains(innerDependency));
            Assert.IsFalse(tree["commonEntry5a"].LoadBefore.Contains(outterDep1));
            Assert.IsFalse(tree["commonEntry5a"].LoadBefore.Contains(outterDep2));

            Assert.IsFalse(tree["commonEntry6a"].LoadBefore.Contains(innerDependency));
            Assert.IsFalse(tree["commonEntry6a"].LoadBefore.Contains(outterDep1));

        }
    }
}