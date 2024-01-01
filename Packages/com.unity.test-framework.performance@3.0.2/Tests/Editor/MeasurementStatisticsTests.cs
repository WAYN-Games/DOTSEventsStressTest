using System;
using System.Linq;
using NUnit.Framework;
using Unity.PerformanceTesting.Statistics;

namespace Unity.PerformanceTesting.Tests.Editor
{
    public class MeasurementStatisticsTests
    {
        [Test]
        public void Calculate_FromEmptySamplesList_Throws()
        {
            var samples = new double[0];

            Assert.Throws<InvalidOperationException>(
                () => MeasurementsStatistics.Calculate(samples.ToList(), OutlierMode.Remove, ConfidenceLevel.L90));
        }

        [TestCase(new double[] { 1, 2, 3, 4 }, (1d + 2 + 3 + 4) / 4)]
        [TestCase(new double[] { 1, 2, 3, 4, 5 }, (1d + 2 + 3 + 4 + 5) / 5)]
        [TestCase(new double[] { 0, 0, 0, 0, 0 }, 0d)]
        public void Calculate_MeanValue(double[] samples, double expectedMean)
        {
            var measurements = MeasurementsStatistics.Calculate(samples.ToList(), OutlierMode.DontRemove, ConfidenceLevel.L90);

            Assert.AreEqual(expectedMean, measurements.Mean, 1e-6, "Unexpected mean value.");
        }

        [Test]
        public void MeanValue_WhenOutlierModeIsRemove_ExcludesOutliers()
        {
            var samples = new double[] { 1, 1, 1, 1, 1, 10, 1 };

            var measurements = MeasurementsStatistics.Calculate(samples.ToList(), OutlierMode.Remove, ConfidenceLevel.L90);

            Assert.AreEqual(1, measurements.Mean, 1e-6, "Unexpected mean value.");
        }

        [TestCase(ConfidenceLevel.L95, new double[] { 1, 1, 1, 5 }, 3.182446)]
        [TestCase(ConfidenceLevel.L95, new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 10 }, 2.306004)]
        [TestCase(ConfidenceLevel.L99, new double[] { 1, 1, 1, 5 }, 5.840909)]
        [TestCase(ConfidenceLevel.L99, new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 10 }, 3.355387)]
        [TestCase(ConfidenceLevel.L999, new double[] { 1, 1, 1, 5 }, 12.923979)]
        [TestCase(ConfidenceLevel.L999, new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 10 }, 5.041305)]
        public void Calculate_MarginOfError(ConfidenceLevel confidenceLevel, double[] samples, double expected)
        {
            var stats = MeasurementsStatistics.Calculate(samples.ToList(), OutlierMode.DontRemove, confidenceLevel);

            Assert.AreEqual(expected, stats.MarginOfError, 1e-6, "Unexpected margin of error value.");
        }

        [TestCase(OutlierMode.Remove, ConfidenceLevel.L90)]
        [TestCase(OutlierMode.DontRemove, ConfidenceLevel.L999)]
        public void MarginOfError_ForStableMeasurement_IsZero(OutlierMode outlierMode, ConfidenceLevel confidenceLevel)
        {
            var samples = new double[] { 2, 2, 2, 2 };

            var measurements = MeasurementsStatistics.Calculate(samples.ToList(), outlierMode, confidenceLevel);

            Assert.AreEqual(0, measurements.MarginOfError, 1e-6, "Unexpected margin of error value.");
        }

        [Test]
        public void MarginOfError_WhenOutlierModeIsRemove_ExcludesOutliers()
        {
            var samples = new double[] { 1, 1, 1, 1, 1, 10, 1 };

            var measurements = MeasurementsStatistics.Calculate(samples.ToList(), OutlierMode.Remove, ConfidenceLevel.L90);

            Assert.AreEqual(0, measurements.MarginOfError, 1e-6, "Unexpected margin of error value.");
        }
    }
}
