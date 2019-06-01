namespace EasyMarkup.Samples
{
    using System;
    using NUnit.Framework;

    [EasyMarkupClass]
    public class DataBag
    {
        [EasyMarkupProperty]
        public string StrValue { get; set; } = "Val1";

        [EasyMarkupProperty]
        public int InValue { get; set; } = 2;

        [EasyMarkupProperty]
        public float FlValue { get; set; } = 3.4f;
    }

    [EasyMarkupClass]
    public class DataBagCollection
    {
        [EasyMarkupProperty]
        public DataBag DB1 { get; set; } = new DataBag();

        [EasyMarkupProperty]
        public DataBag DB2 { get; set; } = new DataBag();

        [EasyMarkupProperty]
        public DataBag DB3 { get; set; } = new DataBag();
    }

    [TestFixture]
    public class DataBagTests
    {
        [TestCase("Val1", 2, 3.4f)]
        [TestCase("Val2", 3, 4.5f)]
        [TestCase("Val3", 4, 5.6f)]
        public void Simple_GetString(string text, int integer, float fraction)
        {
            var bag = new DataBag
            {
                StrValue = text,
                InValue = integer,
                FlValue = fraction
            };

            string serialized = bag.GetEasyMarkupString();

            Console.WriteLine(serialized);
            Assert.AreEqual($"DataBag:(StrValue:{text};InValue:{integer};FlValue:{fraction};);", serialized);
        }

        [TestCase("Val1", 2, 3.4f)]
        [TestCase("Val2", 3, 4.5f)]
        [TestCase("Val3", 4, 5.6f)]
        public void Simple_LoadFromString(string text, int integer, float fraction)
        {
            string serialized = $"DataBag:(StrValue:{text};InValue:{integer};FlValue:{fraction};);";

            var bag = new DataBag();

            bool success = bag.LoadEasyMarkupString(serialized);
            Assert.IsTrue(success);

            Assert.AreEqual(text, bag.StrValue);
            Assert.AreEqual(integer, bag.InValue);
            Assert.AreEqual(fraction, bag.FlValue);
        }

        [Test]
        public void Nested_GetString()
        {
            var dbCol = new DataBagCollection();

            string serialized = dbCol.GetEasyMarkupString();

            Console.WriteLine(serialized);
            Assert.IsNotNull(serialized);
        }
    }
}