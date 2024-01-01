using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using NUnit.Framework;
using Unity.PerformanceTesting.Measurements;
using Unity.PerformanceTesting.Statistics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Unity.PerformanceTesting.Tests.Runtime
{
    class DynamicFramesMeasurementCountTests
    {
        [UnityTest, Performance]
        public IEnumerator DynamicFramesMeasurement_IsIgnored_WhenFrametimeRecordingIsDisabled()
        {
            var measurement = Measure.Frames()
                .DynamicMeasurementCount()
                .DontRecordFrametime();

            LogAssert.Expect(LogType.Warning, new Regex("DynamicMeasurementCount will be ignored.*"));
            yield return measurement.Run();

            var test = PerformanceTest.Active;
            Assert.AreEqual(0, test.SampleGroups.Count, "Unexpected sample groups count.");
            Assert.False(measurement.m_DynamicMeasurementCount, "DynamicMeasurementCount was not disabled.");
        }

        [UnityTest, Performance]
        public IEnumerator DynamicFramesMeasurement_IsIgnored_WhenExplicitMeasurementCountIsProvided()
        {
            const int expectedMeasurementCount = 5;
            var measurement = Measure.Frames()
                .DynamicMeasurementCount()
                .MeasurementCount(expectedMeasurementCount);

            LogAssert.Expect(LogType.Warning, new Regex("DynamicMeasurementCount will be ignored.*"));
            yield return measurement.Run();

            var test = PerformanceTest.Active;
            Assert.AreEqual(1, test.SampleGroups.Count, "Unexpected sample groups count.");
            Assert.AreEqual(expectedMeasurementCount, test.SampleGroups[0].Samples.Count, "Unexpected sample count.");
            Assert.False(measurement.m_DynamicMeasurementCount, "DynamicMeasurementCount was not disabled.");
        }

        [UnityTest, Performance]
        public IEnumerator DynamicFramesMeasurement_RespectsMinimumMeasurementCount()
        {
            const int minMeasurementCount = FramesMeasurement.k_MinIterations;
            var measurement = Measure.Frames()
                .DynamicMeasurementCount(double.MaxValue, ConfidenceLevel.L90, OutlierMode.Remove);

            yield return measurement.Run();

            var test = PerformanceTest.Active;
            Assert.AreEqual(1, test.SampleGroups.Count, "Unexpected sample groups count.");
            Assert.AreEqual(minMeasurementCount, test.SampleGroups[0].Samples.Count, "Unexpected sample count.");
            Assert.True(measurement.m_DynamicMeasurementCount, "DynamicMeasurementCount was not enabled.");
        }

        [UnityTest, Performance]
        public IEnumerator DynamicFramesMeasurement_RespectsWarmupCount()
        {
            const int minMeasurementCount = FramesMeasurement.k_MinIterations;
            const int warmupCount = 5;
            var measurement = Measure.Frames()
                .DynamicMeasurementCount(double.MaxValue, ConfidenceLevel.L90, OutlierMode.Remove)
                .WarmupCount(warmupCount);

            var frameCountBefore = Time.frameCount;
            yield return measurement.Run();

            var test = PerformanceTest.Active;
            var frameTime = Time.frameCount - frameCountBefore;
            Assert.AreEqual(1, test.SampleGroups.Count, "Unexpected sample groups count.");
            Assert.AreEqual(minMeasurementCount, test.SampleGroups[0].Samples.Count, "Unexpected sample count.");
            Assert.AreEqual(minMeasurementCount + warmupCount, frameTime, "Unexpected frame time.");
            Assert.True(measurement.m_DynamicMeasurementCount, "DynamicMeasurementCount was not enabled.");
        }

        [UnityTest, Performance]
        public IEnumerator DynamicFramesMeasurement_RespectsMaximumMeasurementCount()
        {
            const int maxMeasurementCount = FramesMeasurement.k_MaxDynamicMeasurements;
            var measurement = Measure.Frames()
                .DynamicMeasurementCount(0d, ConfidenceLevel.L999, OutlierMode.DontRemove);

            yield return measurement.Run();

            var test = PerformanceTest.Active;
            Assert.AreEqual(1, test.SampleGroups.Count, "Unexpected sample groups count.");
            Assert.AreEqual(maxMeasurementCount, test.SampleGroups[0].Samples.Count, "Unexpected sample count.");
            Assert.True(measurement.m_DynamicMeasurementCount, "DynamicMeasurementCount was not enabled.");
        }

        [UnityTest, Performance]
        public IEnumerator DynamicFramesMeasurement_WithProfilerMarkers_RecordsMarkers()
        {
            const string profilerMarkerName = "TEST_MARKER";
            ProfilerMarkerComponent.profilerMarkerName = profilerMarkerName;
            var obj = new GameObject("DynamicFramesMeasurement_WithProfilerMarkers_RecordsMarkers", typeof(ProfilerMarkerComponent));
            var measurement = Measure.Frames()
                .DynamicMeasurementCount(1d, ConfidenceLevel.L90, OutlierMode.Remove)
                .ProfilerMarkers(profilerMarkerName);

            yield return measurement.Run();

            var test = PerformanceTest.Active;
            var sampleGroups = test.SampleGroups;
            var markerSampleGroup = sampleGroups.SingleOrDefault(sg => sg.Name == profilerMarkerName);
            Assert.AreEqual(2, sampleGroups.Count, "Unexpected sample groups count.");
            Assert.NotNull(markerSampleGroup, "Profiler marker sample group not found.");
            Assert.True(measurement.m_DynamicMeasurementCount, "DynamicMeasurementCount was not enabled.");
            Object.Destroy(obj);
        }

        class ProfilerMarkerComponent : MonoBehaviour
        {
            CustomSampler m_CustomSampler;
            public static string profilerMarkerName;

            void OnEnable()
            {
                m_CustomSampler = CustomSampler.Create(profilerMarkerName);
            }

            void Update()
            {
                m_CustomSampler.Begin();
                Thread.Sleep(1);
                m_CustomSampler.End();
            }
        }
    }
}
