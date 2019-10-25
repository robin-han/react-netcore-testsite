using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;

namespace GrapeCity.DataVisualization.Chart.TestSite
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        public TestController()
        {
        }

        /// <summary>
        /// Gets all tests.
        /// </summary>
        /// <returns>All tests.</returns>
        // GET api/test
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Test>>> Get()
        {
            return await Task.FromResult(Tests);
        }

        /// <summary>
        /// Get test by id.
        /// </summary>
        /// <param name="id">The test id.</param>
        /// <returns>The test.</returns>
        // GET api/test/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Test>> Get(long id)
        {
            Test test = Tests.Find((item) => item.Id == id);
            if (test == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(test.Content))
            {
                Test ret = test.Clone();
                ret.Content = await this.ReadTestContent(ret.Path);
                return ret;
            }
            else
            {
                return test;
            }
        }

        //PUT api/test/batch/5
        [HttpPut("batch/{id}")]
        public async Task<ActionResult<TestResult>> BatchRunTest(long id)
        {
            Test test = Tests.Find(item => item.Id == id);
            if (test == null)
            {
                return BadRequest();
            }

            string json = test.Content;
            if (string.IsNullOrEmpty(json))
            {
                json = await this.ReadTestContent(test.Path);
            }

            return await Task.Run<ActionResult<TestResult>>((Func<Task<ActionResult<TestResult>>>)(async () =>
            {
                string expectedSvgText = "";
                try
                {
                    XElement componentSvg = this.RenderComponent(json);
                    string diff = "No Expected";
                    expectedSvgText = await this.ReadRenderResult(test.Path);
                    if (!string.IsNullOrEmpty(expectedSvgText))
                    {
                        using (StringReader reader = new StringReader(expectedSvgText))
                        {
                            XElement expectedSvg = XElement.Load(reader);
                            diff = new SvgDiff().Diff(componentSvg, expectedSvg);
                        }
                    }

                    if (!string.IsNullOrEmpty(diff))
                    {
                        return new TestResult()
                        {
                            TestId = test.Id,
                            TestContent = json,
                            State = TestResultState.Failure,
                            Result = componentSvg.ToString(),
                            Expected = expectedSvgText,
                            Message = diff,
                        };
                    }

                    return new TestResult()
                    {
                        TestId = test.Id,
                        State = TestResultState.Success,
                        Result = componentSvg.ToString(),
                    };
                }
                catch (Exception ex)
                {
                    return new TestResult()
                    {
                        TestId = test.Id,
                        TestContent = json,
                        Expected = expectedSvgText,
                        Message = string.Format("{0}:\r\n{1}", test.Path, this.GetExceptionMessage(ex)),
                        State = TestResultState.Error
                    };
                }
            }));
        }

        /// <summary>
        /// Run test and save test result.
        /// </summary>
        /// <param name="id">The test id.</param>
        /// <returns>The test result.</returns>
        //PUT api/test/save/5
        [HttpPut("save/{id}")]
        public async Task<ActionResult<TestResult>> RunAndSaveTest(long id)
        {
            TestResult testResult = (await this.RunTest(id)).Value;
            if (testResult.State == TestResultState.Success)
            {
                Test test = Tests.Find(item => item.Id == id);
                this.WriteRenderResult(test.Path, testResult.Result);
            }

            return testResult;
        }

        /// <summary>
        /// Run test by id, if batch test result will contains test content.
        /// </summary>
        /// <param name="id">The test id.</param>
        /// <param name="batch">Indicates whether batch running.</param>
        /// <returns>The test result.</returns>
        // PUT api/test/5
        [HttpPut("{id}")]
        public async Task<ActionResult<TestResult>> RunTest(long id)
        {
            Test test = Tests.Find(item => item.Id == id);
            if (test == null)
            {
                return BadRequest();
            }

            string json = test.Content;
            if (string.IsNullOrEmpty(json))
            {
                json = await this.ReadTestContent(test.Path);
            }

            return await Task.Run<ActionResult<TestResult>>((Func<ActionResult<TestResult>>)(() =>
            {
                try
                {
                    XElement componentSvg = this.RenderComponent(json);
                    return new TestResult()
                    {
                        TestId = test.Id,
                        TestContent = json,
                        Result = componentSvg.ToString(),
                        State = TestResultState.Success
                    };
                }
                catch (Exception ex)
                {
                    return new TestResult()
                    {
                        TestId = test.Id,
                        TestContent = json,
                        Message = string.Format("{0}:\r\n{1}", test.Path, this.GetExceptionMessage(ex)),
                        State = TestResultState.Error
                    };
                }
            }));
        }

        /// <summary>
        /// Runs test by request's content.
        /// </summary>
        /// <param name="test">The test contains test content</param>
        /// <returns>The test result.</returns>
        // PUT api/test
        [HttpPut()]
        public async Task<ActionResult<TestResult>> RefreshTest([FromBody] Test test)
        {
            string json = test.Content;
            if (string.IsNullOrEmpty(json))
            {
                return BadRequest();
            }

            return await Task.Run<ActionResult<TestResult>>((Func<ActionResult<TestResult>>)(() =>
            {
                try
                {
                    XElement componentSvg = this.RenderComponent(json);
                    return new TestResult()
                    {
                        TestId = test.Id,
                        TestContent = test.Content,
                        Result = componentSvg.ToString(),
                        State = TestResultState.Success
                    };
                }
                catch (Exception ex)
                {
                    return new TestResult()
                    {
                        TestId = test.Id,
                        TestContent = test.Content,
                        State = TestResultState.Error,
                        Message = this.GetExceptionMessage(ex)
                    };
                }
            }));
        }

        /// <summary>
        /// Reads test content.
        /// </summary>
        /// <param name="path">The test path</param>
        /// <returns>The test content.</returns>
        private async Task<string> ReadTestContent(string path)
        {
            path = Path.Combine("App_Data", path);
            if (System.IO.File.Exists(path))
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    return await reader.ReadToEndAsync();
                }
            }

            return "";
        }

        /// <summary>
        /// Read expected test result.
        /// </summary>
        /// <param name="path">The test path.</param>
        /// <returns></returns>
        private async Task<string> ReadRenderResult(string path)
        {
            path = Path.Combine("App_Result", path.Replace(".json", ".svg"));
            if (System.IO.File.Exists(path))
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    return await reader.ReadToEndAsync();
                }
            }

            return "";
        }

        /// <summary>
        ///  Write expected test result.
        /// </summary>
        /// <param name="path">The test path.</param>
        /// <param name="content">The test result.</param>
        private void WriteRenderResult(string path, string content)
        {
            string resultPath = Path.Combine("App_Result", path.Replace(".json", ".svg"));
            if (!Directory.Exists(Path.GetDirectoryName(resultPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(resultPath));
            }

            using (StreamWriter writer = new StreamWriter(resultPath))
            {
                writer.Write(content);
            }
        }

        /// <summary>
        /// Render componnet.
        /// </summary>
        /// <param name="json">The component json</param>
        /// <returns>Component svg element</returns>
        private XElement RenderComponent(string json)
        {
            // TODO:
            string str =
            @"<svg width=""600"" height=""300"" version=""1.1"" xmlns=""http://www.w3.org/2000/svg"">
                <g stroke=""black"">
                    <line x1=""75"" y1=""160"" x2=""525"" y2=""160"" stroke=""lightgreen"" stroke-width=""10""/>
                </g>
            </svg>";
            XDocument doc = XDocument.Parse(str);
            return doc.Root;
        }

        /// <summary>
        /// Gets the exception message.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>The exception message</returns>
        private string GetExceptionMessage(Exception ex)
        {
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }
            return string.Format("Message:\r\n{0}StackTrace:\r\n{1}", ex.Message, ex.StackTrace);
        }

        private static List<Test> tests;
        /// <summary>
        /// Gets all tests.
        /// </summary>
        private static List<Test> Tests
        {
            get
            {
                if (tests != null)
                {
                    return tests;
                }

                tests = new List<Test>();
                string casesDir = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
                string[] files = Directory.GetFiles(casesDir, "*.json", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    Test testCase = new Test()
                    {
                        Id = i + 1,
                        Name = Path.GetFileNameWithoutExtension(files[i]),
                        Path = Path.GetRelativePath(casesDir, files[i]).Replace("\\", "/"),
                        //Content = System.IO.File.ReadAllText(files[i])
                    };
                    tests.Add(testCase);
                }

                return tests;
            }
        }

    }
}
