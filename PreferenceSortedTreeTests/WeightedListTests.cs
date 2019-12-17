namespace ScratchProjects.Tests
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using Sorting;

    [TestFixture]
    internal class WeightedListTests
    {

        [Test]
        public void AddEntries_GetExpectedOrderAndWeights()
        {
            var weightedList = new WeightedList<string>();
            
            weightedList.Add("A");
            weightedList.Add("B");
            weightedList.Add("C");

            weightedList.Add("A");
            weightedList.Add("B");

            weightedList.Add("A");

            List<string> list = weightedList.ToSortedList();

            Assert.AreEqual(3, weightedList.Count);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual("C", list[0]);
            Assert.AreEqual("B", list[1]);
            Assert.AreEqual("A", list[2]);
            Assert.AreEqual("C", weightedList.GetMinWeight());
            Assert.AreEqual("A", weightedList.GetMaxWeight());
        }

        [Test]
        public void RemoveEntries_LeaveOne_GetExpectedOrderAndWeights()
        {
            var weightedList = new WeightedList<string>();
            
            weightedList.Add("A");
            weightedList.Add("B");
            weightedList.Add("C");

            weightedList.Add("A");
            weightedList.Add("B");
            weightedList.Add("C");

            weightedList.Add("A");
            weightedList.Add("B");

            weightedList.Remove("A");
            weightedList.Remove("A");

            List<string> list = weightedList.ToSortedList();

            Assert.AreEqual(3, weightedList.Count);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual("A", list[0]);
            Assert.AreEqual("C", list[1]);
            Assert.AreEqual("B", list[2]);
            Assert.AreEqual("A", weightedList.GetMinWeight());
            Assert.AreEqual("B", weightedList.GetMaxWeight());
        }

        [Test]
        public void RemoveEntries_LeaveNone_GetExpectedOrderAndWeights()
        {
            var weightedList = new WeightedList<string>();
            
            weightedList.Add("A");
            weightedList.Add("B");
            weightedList.Add("C");

            weightedList.Add("A");
            weightedList.Add("B");
            weightedList.Add("C");

            weightedList.Add("A");
            weightedList.Add("B");

            weightedList.Remove("A");
            weightedList.Remove("A");
            weightedList.Remove("A");

            List<string> list = weightedList.ToSortedList();

            Assert.AreEqual(2, weightedList.Count);
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual("C", list[0]);
            Assert.AreEqual("B", list[1]);
            Assert.AreEqual("C", weightedList.GetMinWeight());
            Assert.AreEqual("B", weightedList.GetMaxWeight());
        }

        [Test]
        public void RemoveEntries_PreserveAtZero_LeaveNone_GetExpectedOrderAndWeights()
        {
            var weightedList = new WeightedList<string>();
            
            weightedList.Add("A");
            weightedList.Add("B");
            weightedList.Add("C");

            weightedList.Add("A");
            weightedList.Add("B");
            weightedList.Add("C");

            weightedList.Add("A");
            weightedList.Add("B");

            weightedList.Remove("A", false);
            weightedList.Remove("A", false);
            weightedList.Remove("A", false);

            List<string> list = weightedList.ToSortedList();

            Assert.AreEqual(3, weightedList.Count);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual("A", list[0]);
            Assert.AreEqual("C", list[1]);
            Assert.AreEqual("B", list[2]);
            Assert.AreEqual("A", weightedList.GetMinWeight());
            Assert.AreEqual("B", weightedList.GetMaxWeight());
        }
    }
}
