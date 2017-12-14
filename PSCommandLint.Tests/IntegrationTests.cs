using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PSCommandLint.Analysis;
using System.Management.Automation.Language;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using PSCommandLint.Model;

namespace PSCommandLint.Tests
{
    [TestClass]
    public class IntegrationTests
    {
        [DataTestMethod]
        [DataRow("TestCases/DotSourcedMissingFunction/Main.ps1")]
        [DataRow("TestCases/MissingFunction/Main.ps1")]
        [DataRow("TestCases/ValidDotSourced/Main.ps1")]
        [DataRow("TestCases/ValidDotSourcedDeepHierarchy/Main.ps1")]
        [DataRow("TestCases/ValidScript/Main.ps1")]
        public void RunIntegrationTest(string filename)
        {
            ILookup<string, object> expectations = GetExpectedFields(filename);
            Assert.IsTrue(expectations.Count() > 0);

            AnalysisResult result = CommandAnalyzer.AnalyzeFile(filename);

            AssertSingle(expectations["ParseErrors"], () => result.ParseErrors.Count);
            AssertSingle(expectations["UnsupportedErrors"], () => result.UnsupportedErrors.Count);
            AssertSingle(expectations["LocalCommands"], () => result.LocalCommands.Count);
            AssertSingle(expectations["ValidationErrors"], () => result.ValidationErrors.Count);
            AssertMultiple(expectations["Message"], () => result.ValidationErrors.Select(e => e.Message));
        }

        /// <summary>
        /// Read expected values from the header comments of the file.
        /// </summary>
        /// <param name="file">filename of the powershell test case</param>
        /// <returns>key/value of expected fields and their values</returns>
        private static ILookup<string, object> GetExpectedFields(string file)
        {
            return File.ReadAllLines(file)
                .TakeWhile(line => line.StartsWith("#"))
                .Select(line => line.Split(new[] { '#', '=' }, StringSplitOptions.RemoveEmptyEntries))
                .ToLookup(
                    pair => pair[0].Trim(),
                    pair =>
                    {
                        string value = pair[1].Trim();
                        return int.TryParse(value, out int parsed)
                            ? parsed as object
                            : value;
                    });
        }

        private static void AssertSingle(IEnumerable<object> expectations, Func<object> actualFunc)
        {
            var expected = expectations.Single();
            if (expected != null)
            {
                Assert.AreEqual(expected, actualFunc());
            }
        }

        private static void AssertMultiple(IEnumerable<object> expectations, Func<IEnumerable<object>> actualFunc)
        {
            var expected = expectations.ToList();
            var actual = actualFunc().ToList();
            Assert.AreEqual(expected.Count(), actual.Count());

            var testCases = expected
                .Zip(actual, (expectedElement, actualElement) => (expectedElement, actualElement));
            foreach (var test in testCases)
            {
                Assert.AreEqual(test.expectedElement, test.actualElement);
            }
        }
    }
}
