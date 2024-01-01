using System.Collections;
using NUnit.Framework;
using Unity.PerformanceTesting.Data;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.PerformanceTesting.Tests.Editor
{
    class MeasurementCountOverrideTests
    {
        private readonly int m_OriginalCount = RunSettings.Instance.MeasurementCount;
        
        [OneTimeSetUp]
        public void Setup()
        {
            RunSettings.Instance.MeasurementCount = 1;
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            RunSettings.Instance.MeasurementCount = m_OriginalCount;
        }

        [Test, Performance]
        public void MeasureMethod_With_NoArguments()
        {
            var callCount = 0;

            Measure.Method(() => { callCount++; }).Run();

            var test = PerformanceTest.Active;
            Assert.AreEqual(1, test.SampleGroups.Count);
            Assert.AreEqual(1, test.SampleGroups[0].Samples.Count);
            Assert.AreEqual(1, callCount);
        }

        [SerializeField] private int m_SerializedCount;
        [UnityTest, Performance]
        public IEnumerator MeasureMethod_Survives_EnterPlaymode()
        {
            yield return new EnterPlayMode();
            yield return new ExitPlayMode();
            m_SerializedCount = 0;

            Measure.Method(() => { m_SerializedCount++; }).Run();

            var test = PerformanceTest.Active;
            Assert.AreEqual(1, test.SampleGroups.Count);
            Assert.AreEqual(1, test.SampleGroups[0].Samples.Count);
            Assert.AreEqual(1, m_SerializedCount);
        }


        [Test, Performance]
        public void MeasureMethod_With_MeasurementCount()
        {
            var callCount = 0;

            Measure.Method(() => { callCount++; })
                .MeasurementCount(10)
                .Run();

            var test = PerformanceTest.Active;
            Assert.AreEqual(1, test.SampleGroups.Count);
            Assert.AreEqual(1, test.SampleGroups[0].Samples.Count);
            Assert.AreEqual(1, callCount);
        }
        
        [Test, Performance]
        public void MeasureMethod_With_MeasurementAndWarmupCount()
        {
            var callCount = 0;

            Measure.Method(() => { callCount++; })
                .WarmupCount(10)
                .MeasurementCount(10)
                .Run();

            var test = PerformanceTest.Active;
            Assert.AreEqual(1, test.SampleGroups.Count);
            Assert.AreEqual(1, test.SampleGroups[0].Samples.Count);
            Assert.AreEqual(2, callCount);
        }
        
        [Test, Performance]
        public void MeasureMethod_With_MeasurementAndWarmupAndIterationsCount()
        {
            var callCount = 0;

            Measure.Method(() => { callCount++; })
                .WarmupCount(10)
                .IterationsPerMeasurement(10)
                .MeasurementCount(10)
                .Run();

            var test = PerformanceTest.Active;
            Assert.AreEqual(1, test.SampleGroups.Count);
            Assert.AreEqual(1, test.SampleGroups[0].Samples.Count);
            Assert.AreEqual(20, callCount);
        }

        [Test, Performance]
        public void DynamicMethodMeasurement_IsOverridden_ByRunSettings()
        {
            var callCount = 0;
            var measurement = Measure.Method(() => { callCount++; })
                .DynamicMeasurementCount();

            measurement.Run();

            var test = PerformanceTest.Active;
            Assert.AreEqual(1, test.SampleGroups.Count, "Unexpected sample groups count.");
            Assert.AreEqual(1, test.SampleGroups[0].Samples.Count, "Unexpected sample count.");
            Assert.AreEqual(1, callCount);
            Assert.False(measurement.m_DynamicMeasurementCount, "DynamicMeasurementCount was not disabled.");
        }
    }
}