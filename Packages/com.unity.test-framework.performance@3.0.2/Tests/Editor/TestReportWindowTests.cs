using System;
using System.Collections;
using NUnit.Framework;
using Unity.PerformanceTesting;
using Unity.PerformanceTesting.Editor;
using Unity.PerformanceTesting.Runtime;
using UnityEngine.TestTools;
using UnityEngine;

namespace Unity.PerformanceTesting.Tests.Editor
{
    public class TestReportWindowTests
    {
        [Test]
        public void TestReportWindow_SetupMaterial_ThrowsNoExceptions()
        {
            // Create new instance of test report window
            TestReportWindow m_testReportWindow = new TestReportWindow();
            // Check if SetupMaterial doesn't create exception, if it does, fail the test
            Assert.That(() => m_testReportWindow.SetupMaterial(), 
                  !Throws.TypeOf<Exception>());
        }
    }
}
