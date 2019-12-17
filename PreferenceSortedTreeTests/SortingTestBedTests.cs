namespace ScratchProjects.Tests
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using ScratchProjects.Sorting;

    [TestFixture]
    internal class SortingTestBedTests
    {
        private class TestData : ISortable<string>
        {
            public TestData(string id)
            {
                this.Id = id;
            }

            public TestData(int id)
            {
                this.Id = id.ToString();
            }

            public string Id { get; }

            public IList<string> Dependencies { get; } = new List<string>();

            public IList<string> LoadBefore { get; } = new List<string>();

            public IList<string> LoadAfter { get; } = new List<string>();
        }

        [Test]
        public void Test_NoPreferences_GetExpectedOrder()
        {
            var tree = new SortedCollection<string, TestData>();

            tree.AddSorted(new TestData(0));
            tree.AddSorted(new TestData(1));
            tree.AddSorted(new TestData(2));

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(3, list.Count);

            Assert.AreEqual("2", list[0]);
            Assert.AreEqual("1", list[1]);
            Assert.AreEqual("0", list[2]);

            Assert.Pass(ListToString(list));
        }

        [Test]
        public void Test_DupId_A_GetError()
        {
            var tree = new SortedCollection<string, TestData>();

            tree.AddSorted(new TestData(0));
            tree.AddSorted(new TestData(0));
            tree.AddSorted(new TestData(1));

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(1, list.Count);

            Assert.AreEqual("1", list[0]);

            Assert.Pass(ListToString(list));
        }

        [Test]
        public void Test_DupId_B_GetError()
        {
            var tree = new SortedCollection<string, TestData>();

            tree.AddSorted(new TestData(0));
            tree.AddSorted(new TestData(1));
            tree.AddSorted(new TestData(0));

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(1, list.Count);

            Assert.AreEqual("1", list[0]);

            Assert.Pass(ListToString(list));
        }

        [Test]
        public void Test_DupId_C_GetError()
        {
            var tree = new SortedCollection<string, TestData>();

            var i1 = new TestData(1);
            var i0 = new TestData(0);
            var i02 = new TestData(0);

            tree.AddSorted(i1);
            tree.AddSorted(i0);
            tree.AddSorted(i02);

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(1, list.Count);

            Assert.AreEqual("1", list[0]);

            Assert.Pass(ListToString(list));
        }

        [Test]
        public void Test_MissingDependency_GetExpectedOrder()
        {
            var tree = new SortedCollection<string, TestData>();

            var entity = new TestData(0);
            entity.Dependencies.Add("1");

            tree.AddSorted(entity);

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(0, list.Count);

            Assert.Pass(ListToString(list));
        }

        //-------

        [Test]
        public void Test_MutualSortPrefrence_AB_GetExpectedOrder()
        {
            var tree = new SortedCollection<string, TestData>();

            var iA = new TestData(0);
            iA.LoadBefore.Add("1");

            var iB = new TestData(1);
            iB.LoadAfter.Add("0");

            tree.AddSorted(iA);
            tree.AddSorted(iB);
            tree.AddSorted(new TestData(2));

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(3, list.Count);

            Assert.IsTrue(list.IndexOf("1") < list.IndexOf("0"));

            Assert.Pass(ListToString(list));
        }

        [Test]
        public void Test_MutualSortPrefrence_BA_GetExpectedOrder()
        {
            var tree = new SortedCollection<string, TestData>();

            var iA = new TestData(0);
            iA.LoadBefore.Add("1");

            var iB = new TestData(1);
            iB.LoadAfter.Add("0");

            tree.AddSorted(iB);
            tree.AddSorted(iA);
            tree.AddSorted(new TestData(2));

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(3, list.Count);

            Assert.IsTrue(list.IndexOf("1") < list.IndexOf("0"));

            Assert.Pass(ListToString(list));
        }

        [Test]
        public void Test_MutualSortPrefrence_ACB_GetExpectedOrder()
        {
            var tree = new SortedCollection<string, TestData>();

            var iA = new TestData(0);
            iA.LoadBefore.Add("1");

            var iB = new TestData(1);
            iB.LoadAfter.Add("0");

            tree.AddSorted(iA);
            tree.AddSorted(new TestData(2));
            tree.AddSorted(iB);

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(3, list.Count);

            Assert.IsTrue(list.IndexOf("1") < list.IndexOf("0"));

            Assert.Pass(ListToString(list));
        }

        [Test]
        public void Test_MutualSortPrefrence_BCA_GetExpectedOrder()
        {
            var tree = new SortedCollection<string, TestData>();

            var iA = new TestData(0);
            iA.LoadBefore.Add("1");

            var iB = new TestData(1);
            iB.LoadAfter.Add("0");

            tree.AddSorted(iB);
            tree.AddSorted(new TestData(2));
            tree.AddSorted(iA);

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(3, list.Count);

            Assert.IsTrue(list.IndexOf("1") < list.IndexOf("0"));

            Assert.Pass(ListToString(list));
        }

        //-------

        [Test]
        public void Test_BeforeOnlySortPrefrence_AB_GetExpectedOrder()
        {
            var tree = new SortedCollection<string, TestData>();

            var iA = new TestData(0);
            iA.LoadBefore.Add("1");

            var iB = new TestData(1);

            tree.AddSorted(iA);
            tree.AddSorted(iB);
            tree.AddSorted(new TestData(2));

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(3, list.Count);

            Assert.IsTrue(list.IndexOf("1") < list.IndexOf("0"));

            Assert.Pass(ListToString(list));
        }

        [Test]
        public void Test_BeforeOnlySortPrefrence_BA_GetExpectedOrder()
        {
            var tree = new SortedCollection<string, TestData>();

            var iA = new TestData(0);
            iA.LoadBefore.Add("1");

            var iB = new TestData(1);

            tree.AddSorted(iB);
            tree.AddSorted(iA);
            tree.AddSorted(new TestData(2));

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(3, list.Count);

            Assert.IsTrue(list.IndexOf("1") < list.IndexOf("0"));

            Assert.Pass(ListToString(list));
        }

        [Test]
        public void Test_BeforeOnlySortPrefrence_ACB_GetExpectedOrder()
        {
            var tree = new SortedCollection<string, TestData>();

            var iA = new TestData(0);
            iA.LoadBefore.Add("1");

            var iB = new TestData(1);

            tree.AddSorted(iA);
            tree.AddSorted(new TestData(2));
            tree.AddSorted(iB);

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(3, list.Count);

            Assert.IsTrue(list.IndexOf("1") < list.IndexOf("0"));

            Assert.Pass(ListToString(list));
        }

        [Test]
        public void Test_BeforeOnlySortPrefrence_BCA_GetExpectedOrder()
        {
            var tree = new SortedCollection<string, TestData>();

            var iA = new TestData(0);
            iA.LoadBefore.Add("1");

            var iB = new TestData(1);

            tree.AddSorted(iB);
            tree.AddSorted(new TestData(2));
            tree.AddSorted(iA);

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(3, list.Count);

            Assert.IsTrue(list.IndexOf("1") < list.IndexOf("0"));

            Assert.Pass(ListToString(list));
        }

        //-------

        [Test]
        public void Test_AfterOnlySortPrefrence_AB_GetExpectedOrder()
        {
            var tree = new SortedCollection<string, TestData>();

            var iA = new TestData(0);

            var iB = new TestData(1);
            iB.LoadAfter.Add("0");

            tree.AddSorted(iA);
            tree.AddSorted(iB);
            tree.AddSorted(new TestData(2));

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(3, list.Count);

            Assert.IsTrue(list.IndexOf("1") < list.IndexOf("0"));

            Assert.Pass(ListToString(list));
        }

        [Test]
        public void Test_AfterOnlySortPrefrence_BA_GetExpectedOrder()
        {
            var tree = new SortedCollection<string, TestData>();

            var iA = new TestData(0);

            var iB = new TestData(1);
            iB.LoadAfter.Add("0");

            tree.AddSorted(iB);
            tree.AddSorted(iA);
            tree.AddSorted(new TestData(2));

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(3, list.Count);

            Assert.IsTrue(list.IndexOf("1") < list.IndexOf("0"));

            Assert.Pass(ListToString(list));
        }

        //-------

        [Test]
        [Ignore("CircularLoadOrder detection not supported in this version")]
        public void Test_CircularLoadOrder_BothLoadBefore_AB_GetError()
        {
            var tree = new SortedCollection<string, TestData>();

            var iA = new TestData(0);
            iA.LoadBefore.Add("1");

            var iB = new TestData(1);
            iB.LoadBefore.Add("0");

            tree.AddSorted(iA);
            bool result = tree.AddSorted(iB);

            Assert.IsTrue(result);

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(0, list.Count);

            Assert.Pass(ListToString(list));
        }

        [Test]
        [Ignore("CircularLoadOrder detection not supported in this version")]
        public void Test_CircularLoadOrder_BothLoadBefore_BA_GetError()
        {
            var tree = new SortedCollection<string, TestData>();

            var iA = new TestData(0);
            iA.LoadBefore.Add("1");

            var iB = new TestData(1);
            iB.LoadBefore.Add("0");

            tree.AddSorted(iB);
            bool result = tree.AddSorted(iA);

            Assert.IsTrue(result);

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(0, list.Count);

            Assert.Pass(ListToString(list));
        }

        [Test]
        [Ignore("CircularLoadOrder detection not supported in this version")]
        public void Test_CircularLoadOrder_BothLoadBefore_ACB_GetError()
        {
            var tree = new SortedCollection<string, TestData>();

            var iA = new TestData(0);
            iA.LoadBefore.Add("1");

            var iB = new TestData(1);
            iB.LoadBefore.Add("0");

            tree.AddSorted(iA);
            tree.AddSorted(new TestData(2));
            bool result = tree.AddSorted(iB);

            Assert.IsTrue(result);

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(1, list.Count);

            Assert.Pass(ListToString(list));
        }

        [Test]
        [Ignore("CircularLoadOrder detection not supported in this version")]
        public void Test_CircularLoadOrder_BothLoadBefore_BCA_GetError()
        {
            var tree = new SortedCollection<string, TestData>();

            var iA = new TestData(0);
            iA.LoadBefore.Add("1");

            var iB = new TestData(1);
            iB.LoadBefore.Add("0");

            tree.AddSorted(iA);
            tree.AddSorted(new TestData(2));
            bool result = tree.AddSorted(iB);

            Assert.IsTrue(result);

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(1, list.Count);

            Assert.Pass(ListToString(list));
        }

        //-------

        [Test]
        [Ignore("CircularLoadOrder detection not supported in this version")]
        public void Test_CircularLoadOrder_BothLoadAfter_AB_GetError()
        {
            var tree = new SortedCollection<string, TestData>();

            var iA = new TestData(0);
            iA.LoadAfter.Add("1");

            var iB = new TestData(1);
            iB.LoadAfter.Add("0");

            tree.AddSorted(iA);
            bool result = tree.AddSorted(iB);

            Assert.IsTrue(result);

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(0, list.Count);

            Assert.Pass(ListToString(list));
        }

        [Test]
        [Ignore("CircularLoadOrder detection not supported in this version")]
        public void Test_CircularLoadOrder_BothLoadAfter_BA_GetError()
        {
            var tree = new SortedCollection<string, TestData>();

            var iA = new TestData(0);
            iA.LoadAfter.Add("1");

            var iB = new TestData(1);
            iB.LoadAfter.Add("0");

            tree.AddSorted(iB);
            bool result = tree.AddSorted(iA);

            Assert.IsTrue(result);

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(0, list.Count);

            Assert.Pass(ListToString(list));
        }

        [Test]
        [Ignore("CircularLoadOrder detection not supported in this version")]
        public void Test_CircularLoadOrder_BothLoadAfter_ACB_GetError()
        {
            var tree = new SortedCollection<string, TestData>();

            var iA = new TestData(0);
            iA.LoadAfter.Add("1");

            var iB = new TestData(1);
            iB.LoadAfter.Add("0");

            tree.AddSorted(iA);
            tree.AddSorted(new TestData(2));
            bool result = tree.AddSorted(iB);

            Assert.IsTrue(result);

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(1, list.Count);

            Assert.Pass(ListToString(list));
        }

        [Test]
        [Ignore("CircularLoadOrder detection not supported in this version")]
        public void Test_CircularLoadOrder_BothLoadAfter_BCA_GetError()
        {
            var tree = new SortedCollection<string, TestData>();

            var iA = new TestData(0);
            iA.LoadAfter.Add("1");

            var iB = new TestData(1);
            iB.LoadAfter.Add("0");

            tree.AddSorted(iA);
            tree.AddSorted(new TestData(2));
            bool result = tree.AddSorted(iB);

            Assert.IsTrue(result);

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(1, list.Count);

            Assert.Pass(ListToString(list));
        }

        //-------

        [Test]
        [Ignore("CircularLoadOrder detection not supported in this version")]
        public void Test_CircularLoadOrder_DependencyChain3inLoop_AllLoadAfter_GetError()
        {
            // A -> B ~ B -> C ~ C -> A
            var tree = new SortedCollection<string, TestData>();

            var iA = new TestData(0);
            iA.LoadAfter.Add("1");

            var iB = new TestData(1);
            iB.LoadAfter.Add("2");

            var iC = new TestData(2);
            iC.LoadAfter.Add("0");

            tree.AddSorted(iA);
            tree.AddSorted(iB);
            tree.AddSorted(iC);

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(0, list.Count);

            Assert.Pass(ListToString(list));
        }

        [Test]
        [Ignore("CircularLoadOrder detection not supported in this version")]
        public void Test_CircularLoadOrder_DependencyChain4inLoop_AllLoadAfter_GetError()
        {
            // A -> B ~ B -> C ~ C -> D ~ D -> A
            var tree = new SortedCollection<string, TestData>();

            var iA = new TestData(0);
            iA.LoadAfter.Add("1");

            var iB = new TestData(1);
            iB.LoadAfter.Add("2");

            var iC = new TestData(2);
            iC.LoadAfter.Add("3");

            var iD = new TestData(3);
            iD.LoadAfter.Add("0");

            tree.AddSorted(iA);
            tree.AddSorted(iB);
            tree.AddSorted(iC);
            tree.AddSorted(iD);

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(0, list.Count);

            Assert.Pass(ListToString(list));
        }

        [Test]
        [Ignore("CircularLoadOrder detection not supported in this version")]
        public void Test_CircularLoadOrder_DependencyChain3inLoop_AllLoadBefore_GetError()
        {
            // A -> B ~ B -> C ~ C -> A
            var tree = new SortedCollection<string, TestData>();

            var iA = new TestData(0);
            iA.LoadBefore.Add("1");

            var iB = new TestData(1);
            iB.LoadBefore.Add("2");

            var iC = new TestData(2);
            iC.LoadBefore.Add("0");

            tree.AddSorted(iA);
            tree.AddSorted(iB);
            tree.AddSorted(iC);

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(0, list.Count);

            Assert.Pass(ListToString(list));
        }

        [Test]
        [Ignore("CircularLoadOrder detection not supported in this version")]
        public void Test_CircularLoadOrder_DependencyChain4inLoop_AllLoadBefore_GetError()
        {
            // A -> B ~ B -> C ~ C -> D ~ D -> A
            var tree = new SortedCollection<string, TestData>();

            var iA = new TestData(0);
            iA.LoadBefore.Add("1");

            var iB = new TestData(1);
            iB.LoadBefore.Add("2");

            var iC = new TestData(2);
            iC.LoadBefore.Add("3");

            var iD = new TestData(3);
            iD.LoadBefore.Add("0");

            tree.AddSorted(iA);
            tree.AddSorted(iB);
            tree.AddSorted(iC);
            tree.AddSorted(iD);

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(0, list.Count);

            Assert.Pass(ListToString(list));
        }

        //-------

        [Test]
        [Ignore("CircularLoadOrder detection not supported in this version")]
        public void Test_CircularLoadOrder_DependencyChain3inLoop_NonDependentEntitiesIncluded()
        {
            // A -> B ~ B -> C ~ C -> A
            var tree = new SortedCollection<string, TestData>();

            var iA = new TestData(0);
            iA.LoadAfter.Add("1");

            var iB = new TestData(1);
            iB.LoadAfter.Add("2");

            var iC = new TestData(2);
            iC.LoadAfter.Add("0");

            tree.AddSorted(new TestData(3));
            tree.AddSorted(iA);
            tree.AddSorted(new TestData(4));
            tree.AddSorted(iB);
            tree.AddSorted(new TestData(5));
            tree.AddSorted(iC);
            tree.AddSorted(new TestData(6));

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(4, list.Count);
            Assert.IsTrue(list.Contains("3"));
            Assert.IsTrue(list.Contains("4"));
            Assert.IsTrue(list.Contains("5"));
            Assert.IsTrue(list.Contains("6"));

            Assert.IsFalse(list.Contains("0"));
            Assert.IsFalse(list.Contains("1"));
            Assert.IsFalse(list.Contains("2"));

            Assert.Pass(ListToString(list));
        }

        [Test]
        [Ignore("CircularLoadOrder detection not supported in this version")]
        public void Test_CircularLoadOrder_DependencyChain3inLoop_NonLoopChainEntitiesIncluded()
        {
            // A -> B ~ B -> C ~ C -> A
            var tree = new SortedCollection<string, TestData>();

            var iA = new TestData(0);
            iA.LoadAfter.Add("1");

            var iB = new TestData(1);
            iB.LoadAfter.Add("2");

            var iC = new TestData(2);
            iC.LoadAfter.Add("0");

            // -- 

            var i3 = new TestData(3);
            i3.LoadAfter.Add("4");

            var i4 = new TestData(4);
            i4.LoadAfter.Add("5");

            var i5 = new TestData(5);
            i5.LoadAfter.Add("6");

            var i6 = new TestData(6);

            tree.AddSorted(i6);
            tree.AddSorted(iA);
            tree.AddSorted(i5);
            tree.AddSorted(iB);
            tree.AddSorted(i4);
            tree.AddSorted(iC);
            tree.AddSorted(i3);

            List<string> list = tree.GetSortedIndexList();
            Console.WriteLine(ListToString(list));

            Assert.AreEqual(4, list.Count);
            Assert.IsTrue(list.Contains("3"));
            Assert.IsTrue(list.Contains("4"));
            Assert.IsTrue(list.Contains("5"));
            Assert.IsTrue(list.Contains("6"));

            Assert.IsFalse(list.Contains("0"));
            Assert.IsFalse(list.Contains("1"));
            Assert.IsFalse(list.Contains("2"));

            Assert.IsTrue(list.IndexOf("3") < list.IndexOf("4"));
            Assert.IsTrue(list.IndexOf("4") < list.IndexOf("5"));
            Assert.IsTrue(list.IndexOf("5") < list.IndexOf("6"));

            Assert.Pass(ListToString(list));
        }

        // TODO - Meta priority tests

        private static string ListToString<T>(IList<T> list)
        {
            string s = "List: ";
            if (list.Count == 0)
            {
                s += "Empty";
                return s;
            }

            int lastIndex = list.Count - 1;

            for (int i = 0; i < lastIndex; i++)
            {
                T item = list[i];
                s += item;
                s += ", ";
            }

            s += list[lastIndex];
            return s;
        }
    }

}
