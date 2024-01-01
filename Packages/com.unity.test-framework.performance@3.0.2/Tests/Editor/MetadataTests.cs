using System;
using NUnit.Framework;
using Unity.PerformanceTesting.Editor;
using Unity.PerformanceTesting.Runtime;

namespace Unity.PerformanceTesting.Tests.Editor
{
    class MetadataTests
    {
        [Test]
        public void TestRunBuilder_GetPerformanceTestRun()
        {
            var builder = new TestRunBuilder();
            var run = builder.GetPerformanceTestRun();

            Assert.Greater(run.Dependencies.Count, 0);
            Assert.NotNull(run.Editor);
            Assert.NotNull(run.Player);
            Assert.NotNull(run.Hardware);
            Assert.NotNull(run.TestSuite);
            Assert.NotNull(run.Date);
            Assert.AreEqual(0, run.Results.Count);
        }

        [Test]
        public void TestRunBuilder_CreateRunInfo()
        {
            var builder = new TestRunBuilder();
            var run = builder.CreateRunInfo();

            Assert.Greater(run.Dependencies.Count, 0);
            Assert.IsFalse(string.IsNullOrEmpty(run.Editor.Branch));
            Assert.IsFalse(string.IsNullOrEmpty(run.Editor.Version));
            Assert.NotNull(run.Player);
            Assert.IsNull(run.Player.Platform);
            Assert.NotNull(run.Date);
            Assert.AreEqual(0, run.Results.Count);
        }
        
        [Test]
        public void TestRunBuilder_CreateBuildInfo()
        {
            var builder = new TestRunBuilder();
            var date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToUniversalTime();

            var run = builder.CreateBuildInfo();

            Assert.Greater(run.Dependencies.Count, 0);
            Assert.IsFalse(string.IsNullOrEmpty(run.Editor.Branch));
            Assert.IsFalse(string.IsNullOrEmpty(run.Editor.Version));
            Assert.NotNull(run.Player);
            Assert.IsNull(run.Player.Platform);
            Assert.NotNull(run.Date);
            Assert.AreEqual(Utils.ConvertToUnixTimestamp(date), run.Date);
            Assert.AreEqual(run.Results.Count, 0);
        }

        [Test]
        public void Metadata_GetFromResources()
        {
            var builder = new TestRunBuilder();
            builder.Setup();

            var run = Metadata.GetFromResources();

            Assert.Greater(run.Dependencies.Count, 0);
            Assert.NotNull(run.Editor);
            Assert.NotNull(run.Player);
            Assert.NotNull(run.Hardware);
            Assert.NotNull(run.TestSuite);
            Assert.NotNull(run.Date);
            Assert.AreEqual(0, run.Results.Count);

            builder.Cleanup();
        }
    }
}