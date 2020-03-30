using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrapeCity.DataVisualization.Chart.TestSite
{
    public class DiffBase
    {
        /// <summary>
        /// Format error message.
        /// </summary>
        /// <param name="actual">The actual message.</param>
        /// <param name="expected">The </param>
        /// <returns></returns>
        protected string Error(string message, string actual, string expected)
        {
            return string.Format("{0}\r\n\r\nActual:\r\n{1}\r\n\r\nExpected:\r\n{2}\r\n------", message, actual, expected);
        }
    }
}
