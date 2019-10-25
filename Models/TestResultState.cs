using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrapeCity.DataVisualization.Chart.TestSite
{
    public enum TestResultState
    {
        /// <summary>
        /// The test was not run now.
        /// </summary>
        NotRun,

        /// <summary>
        /// The test succeeded.
        /// </summary>
        Success,

        /// <summary>
        /// The test failed.
        /// </summary>
        Failure,

        /// <summary>
        /// The test throw exception
        /// </summary>
        Error
    }
}
