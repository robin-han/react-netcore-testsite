using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrapeCity.DataVisualization.Chart.TestSite
{
    public class TestResult
    {
        public TestResult()
        {
            this.TestId = 0;
            this.TestContent = "";
            this.Result = "";
            this.Expected = "";
            this.State = TestResultState.NotRun;
            this.Message = "";
        }

        /// <summary>
        /// Gets or sets the test id.
        /// </summary>
        public long TestId { get; set; }

        /// <summary>
        /// Gets or sets the test content.
        /// </summary>
        public string TestContent { get; set; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// Gets or sets the expected result.
        /// </summary>
        public string Expected { get; set; }

        /// <summary>
        /// Gets or sets the test status
        /// </summary>
        public TestResultState State { get; set; }

        /// <summary>
        /// Gets or sets the test message
        /// </summary>
        public string Message { get; set; }
    }
}
