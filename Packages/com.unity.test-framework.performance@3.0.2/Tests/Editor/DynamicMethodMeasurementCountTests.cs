using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using NUnit.Framework;
using Unity.PerformanceTesting.Measurements;
using Unity.PerformanceTesting.Meters;
using Unity.PerformanceTesting.Statistics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools;

namespace Unity.PerformanceTesting.Tests.Editor
{
    class DynamicMethodMeasurementCountTests
    {
        [Test, Performance]
        public void DynamicMethodMeasurement_IsIgnored_WhenExplicitMeasurementCountIsProvided()
        {
            const int expectedMeasurementCount = 5;
            var fakeStopWatch = new FakeStopWatch();
            var measurement = Measure.Method(() => { fakeStopWatch.SimulateNextSample(); })
                .DynamicMeasurementCount()
                .MeasurementCount(expectedMeasurementCount)
                .StopWatch(fakeStopWatch);

            LogAssert.Expect(LogType.Warning, new Regex("DynamicMeasurementCount will be ignored.*"));
            measurement.Run();

            var test = PerformanceTest.Active;
            Assert.AreEqual(1, test.SampleGroups.Count, "Unexpected sample groups count.");
            Assert.AreEqual(expectedMeasurementCount, test.SampleGroups[0].Samples.Count, "Unexpected sample count.");
            Assert.AreEqual(expectedMeasurementCount, fakeStopWatch.MeasurementsCount, "Unexpected measurement count.");
            Assert.False(measurement.m_DynamicMeasurementCount, "DynamicMeasurementCount was not disabled.");
        }

        [Test, Performance]
        public void DynamicMethodMeasurement_RespectsMinimumMeasurementCount()
        {
            const int minMeasurementCount = MethodMeasurement.k_MeasurementCount;
            var fakeStopWatch = new FakeStopWatch();
            var measurement = Measure.Method(() => { fakeStopWatch.SimulateNextSample(); })
                .DynamicMeasurementCount()
                .StopWatch(fakeStopWatch);

            measurement.Run();

            var test = PerformanceTest.Active;
            Assert.AreEqual(1, test.SampleGroups.Count, "Unexpected sample groups count.");
            Assert.AreEqual(minMeasurementCount, test.SampleGroups[0].Samples.Count, "Unexpected sample count.");
            Assert.AreEqual(minMeasurementCount, fakeStopWatch.MeasurementsCount, "Unexpected measurement count.");
            Assert.True(measurement.m_DynamicMeasurementCount, "DynamicMeasurementCount was not enabled.");
        }

        [Test, Performance]
        public void DynamicMethodMeasurement_RespectsWarmupCount()
        {
            const int minMeasurementCount = MethodMeasurement.k_MeasurementCount;
            const int warmupCount = 5;
            var fakeStopWatch = new FakeStopWatch();
            var measurement = Measure.Method(() => { fakeStopWatch.SimulateNextSample(); })
                .DynamicMeasurementCount()
                .WarmupCount(warmupCount)
                .StopWatch(fakeStopWatch);

            measurement.Run();

            var test = PerformanceTest.Active;
            Assert.AreEqual(1, test.SampleGroups.Count, "Unexpected sample groups count.");
            Assert.AreEqual(minMeasurementCount, test.SampleGroups[0].Samples.Count, "Unexpected sample count.");
            Assert.AreEqual(minMeasurementCount + warmupCount, fakeStopWatch.MeasurementsCount, "Unexpected measurement count.");
            Assert.True(measurement.m_DynamicMeasurementCount, "DynamicMeasurementCount was not enabled.");
        }

        [Test, Performance]
        public void DynamicMethodMeasurement_RespectsMaximumMeasurementCount()
        {
            const int maxMeasurementCount = MethodMeasurement.k_MaxDynamicMeasurements;
            var fakeStopWatch = new FakeStopWatch(measurementIndex => measurementIndex % 2 == 0 ? 10d : 0d);
            var measurement = Measure.Method(() => { fakeStopWatch.SimulateNextSample(); })
                .DynamicMeasurementCount()
                .StopWatch(fakeStopWatch);

            measurement.Run();

            var test = PerformanceTest.Active;
            Assert.AreEqual(1, test.SampleGroups.Count, "Unexpected sample groups count.");
            Assert.AreEqual(maxMeasurementCount, test.SampleGroups[0].Samples.Count, "Unexpected sample count!");
            Assert.AreEqual(maxMeasurementCount, fakeStopWatch.MeasurementsCount, "Unexpected measurement count!");
            Assert.True(measurement.m_DynamicMeasurementCount, "DynamicMeasurementCount was not enabled.");
        }

        [Test, Performance]
        public void DynamicMethodMeasurement_RespectsIterationCount()
        {
            const int iterationCount = 5;
            var fakeStopWatch = new FakeStopWatch();
            var measurement = Measure.Method(() => { fakeStopWatch.SimulateNextSample(); })
                .DynamicMeasurementCount()
                .IterationsPerMeasurement(iterationCount)
                .StopWatch(fakeStopWatch);

            measurement.Run();

            var test = PerformanceTest.Active;
            Assert.AreEqual(1, test.SampleGroups.Count, "Unexpected sample groups count.");
            Assert.AreEqual(iterationCount, test.SampleGroups[0].Samples[0], 1e-6, "Unexpected sample count.");
            Assert.True(measurement.m_DynamicMeasurementCount, "DynamicMeasurementCount was not enabled.");
        }

        [Test, Performance]
        public void DynamicMethodMeasurement_WithProfilerMarkers_RecordsMarkers()
        {
            const string profilerMarkerName = "TEST_MARKER";
            var measurement = Measure.Method(() => { ProfilerMarkerMethod(profilerMarkerName); })
                .DynamicMeasurementCount(double.MaxValue, ConfidenceLevel.L90, OutlierMode.Remove)
                .ProfilerMarkers(profilerMarkerName);

            measurement.Run();

            var test = PerformanceTest.Active;
            var sampleGroups = test.SampleGroups;
            var markerSampleGroup = sampleGroups.SingleOrDefault(sg => sg.Name == profilerMarkerName);
            Assert.AreEqual(2, sampleGroups.Count, "Unexpected sample groups count.");
            Assert.NotNull(markerSampleGroup, "Profiler marker sample group not found.");
            Assert.True(measurement.m_DynamicMeasurementCount, "DynamicMeasurementCount was not enabled.");
        }

        static void ProfilerMarkerMethod(string name)
        {
            var marker = CustomSampler.Create(name);
            marker.Begin();
            Thread.Sleep(1);
            marker.End();
        }

        class FakeStopWatch : IStopWatch
        {
            public delegate double SampleDurationProvider(int measurementIndex);

            readonly SampleDurationProvider m_SampleDurationProvider;
            double m_CurrentTime;
            int m_MeasurementIndex = -1;

            public int MeasurementsCount => m_MeasurementIndex + 1;

            public FakeStopWatch(SampleDurationProvider sampleDurationProvider = null)
            {
                m_SampleDurationProvider = sampleDurationProvider ?? ConstantSampleDurationProvider;
            }

            public void SimulateNextSample()
            {
                m_MeasurementIndex++;
                m_CurrentTime += m_SampleDurationProvider(m_MeasurementIndex);
            }

            public void Start()
            {
                throw new NotSupportedException("FakeStopWatch.Start is not supported.");
            }

            public double Split()
            {
                return m_CurrentTime;
            }

            static double ConstantSampleDurationProvider(int measurementIndex)
            {
                return 1;
            }
        }
    }
}
