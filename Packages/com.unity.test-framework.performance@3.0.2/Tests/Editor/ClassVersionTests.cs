using NUnit.Framework;

namespace Unity.PerformanceTesting.Tests.Editor
{
    [Version("1")]
    class ClassVersionTests
    {
        [Test, Performance]
        public void Default_VersionAttribute_IsSet()
        {
            Assert.AreEqual("1.1", PerformanceTest.Active.Version);
        }

        [Test, Performance, Version("TEST")]
        public void VersionAttribute_IsSet()
        {
            Assert.AreEqual("1.TEST", PerformanceTest.Active.Version);
        }

        [TestCase("1"), TestCase("2")]
        [Test, Performance, Version("TEST")]
        public void VersionAttribute_IsSet_OnTestCase(string name)
        {
            Assert.AreEqual("1.TEST", PerformanceTest.Active.Version);
        }
    }
}