namespace ScratchProjects.Tests
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using Sorting;

    [TestFixture]
    public class SortedTreeTests
    {
        private class TestSortable : ISortable<string>
        {
            public TestSortable(string id)
            {
                Id = id;
                Dependencies = new List<string>();
                LoadBefore = new List<string>();
                LoadAfter = new List<string>();
            }

            public TestSortable(string id, params string[] dependencies)
               : this(id)
            {
                foreach (string dependency in dependencies)
                    Dependencies.Add(dependency);
            }

            public string Id { get; }

            public IList<string> Dependencies { get; }

            public IList<string> LoadBefore { get; }

            public IList<string> LoadAfter { get; }
        }

        [Test]
        public void CleanRedundantDependencies_SingleRedundancy_RedundanciesCleared()
        {
            var rng = new Random();
            const string innerDependency = "mostUsedDependency";
            const string outterDependency = "redundant";
            var entries = new List<TestSortable>
            {
                new TestSortable(innerDependency),
                new TestSortable(outterDependency, innerDependency),
                new TestSortable("commonEntry1", innerDependency),
                new TestSortable("commonEntry2", innerDependency, outterDependency),
                new TestSortable("commonEntry3", innerDependency, outterDependency),
                new TestSortable("commonEntry4"),
                new TestSortable("commonEntry5", innerDependency, outterDependency),
                new TestSortable("commonEntry6", innerDependency, outterDependency),
                new TestSortable("commonEntry7", innerDependency),
            };
            int originalCount = entries.Count;
            var tree = new SortedTree<string, TestSortable>();

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
            var entries = new List<TestSortable>
            {
                new TestSortable(innerDependency),
                new TestSortable(outterDep1, innerDependency),
                new TestSortable(outterDep2, innerDependency, outterDep1),
                new TestSortable(outterDep3, outterDep2, innerDependency),
                new TestSortable("commonEntry1", innerDependency),
                new TestSortable("commonEntry1a", outterDep2, innerDependency),
                new TestSortable("commonEntry2", innerDependency, outterDep1),
                new TestSortable("commonEntry2a", innerDependency, outterDep1, outterDep2),
                new TestSortable("commonEntry3", innerDependency, outterDep2, outterDep3, outterDep1),
                new TestSortable("commonEntry3a", innerDependency, outterDep1),
                new TestSortable("commonEntry4"),
                new TestSortable("commonEntry5", innerDependency, outterDep2),
                new TestSortable("commonEntry5a", innerDependency, outterDep1, outterDep2, outterDep3),
                new TestSortable("commonEntry6", innerDependency, outterDep1),
                new TestSortable("commonEntry6a", innerDependency, outterDep2, outterDep1),
                new TestSortable("commonEntry7", innerDependency),
            };
            int originalCount = entries.Count;
            var tree = new SortedTree<string, TestSortable>();

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
    }
}