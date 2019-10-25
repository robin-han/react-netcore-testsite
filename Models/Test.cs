using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrapeCity.DataVisualization.Chart.TestSite
{
    public class Test
    {
        public Test()
        {
            this.Id = 0;
            this.Name = "";
            this.Path = "";
            this.Content = "";
        }

        /// <summary>
        /// Gets or sets the test id.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the test name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the test path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the test content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Clone test.
        /// </summary>
        /// <returns></returns>
        public Test Clone()
        {
            return new Test()
            {
                Id = this.Id,
                Name = this.Name,
                Path = this.Path,
                Content = this.Content,
            };
        }
    }
}
