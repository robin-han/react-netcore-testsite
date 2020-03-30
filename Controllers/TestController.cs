using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using GrapeCity.Documents.Imaging;
using Microsoft.AspNetCore.Mvc;

namespace GrapeCity.DataVisualization.Chart.TestSite
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private RenderEngineType _renderType = RenderEngineType.Svg;

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

            string dvJson = test.Content;
            if (string.IsNullOrEmpty(dvJson))
            {
                dvJson = await this.ReadTestContent(test.Path);
            }

            return await Task.Run<ActionResult<TestResult>>(() =>
            {
                try
                {
                    var result = this.RenderAndDiff(dvJson);
                    if (!string.IsNullOrEmpty(result.Diff))
                    {
                        return new TestResult()
                        {
                            TestId = test.Id,
                            TestContent = dvJson,
                            State = TestResultState.Failure,
                            Result = result.Actual,
                            Expected = result.Expected,
                            Message = result.Diff
                        };
                    }

                    return new TestResult()
                    {
                        TestId = test.Id,
                        State = TestResultState.Success,
                        Result = result.Actual
                    };
                }
                catch (Exception ex)
                {
                    return new TestResult()
                    {
                        TestId = test.Id,
                        TestContent = dvJson,
                        Message = string.Format("{0}:\r\n{1}", test.Path, this.GetExceptionMessage(ex)),
                        State = TestResultState.Error
                    };
                }
            });
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

            string dvJson = test.Content;
            if (string.IsNullOrEmpty(dvJson))
            {
                dvJson = await this.ReadTestContent(test.Path);
            }

            return await Task.Run<ActionResult<TestResult>>(() =>
            {
                try
                {
                    string result = this.Render(dvJson);
                    return new TestResult()
                    {
                        TestId = test.Id,
                        TestContent = dvJson,
                        Result = result,
                        State = TestResultState.Success
                    };
                }
                catch (Exception ex)
                {
                    return new TestResult()
                    {
                        TestId = test.Id,
                        TestContent = dvJson,
                        Message = string.Format("{0}:\r\n{1}", test.Path, this.GetExceptionMessage(ex)),
                        State = TestResultState.Error
                    };
                }
            });
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
            string dvJson = test.Content;
            if (string.IsNullOrEmpty(dvJson))
            {
                return BadRequest();
            }

            return await Task.Run<ActionResult<TestResult>>(() =>
            {
                try
                {
                    string result = this.Render(dvJson);
                    return new TestResult()
                    {
                        TestId = test.Id,
                        TestContent = test.Content,
                        Result = result,
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
            });
        }

        /// <summary>
        /// Reads test content.
        /// </summary>
        /// <param name="path">The test path</param>
        /// <returns>The test content.</returns>
        private async Task<string> ReadTestContent(string path)
        {
            path = System.IO.Path.Combine("App_Data", path);
            if (System.IO.File.Exists(path))
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            return "";
        }

        private string Render(string dvJson)
        {
            string result = string.Empty;
            switch (this._renderType)
            {
                case RenderEngineType.Svg:
                    //SvgRender svgRender = new SvgRender();
                    //XElement dvSvg = svgRender.Draw(dvJson);
                     // TODO:
                    XElement dvSvg = this.RenderToSvg("", null);
                    result = dvSvg.ToString();
                    break;

                case RenderEngineType.Imaging:
                    //GcImageRender imgRender = new GcImageRender();
                    //GcBitmap dvImg = imgRender.Draw(dvJson);
                    //using (MemoryStream memory = new MemoryStream())
                    //{
                    //    dvImg.SaveAsPng(memory);
                    //    result = "data:image/png;base64," + System.Convert.ToBase64String(memory.ToArray());
                    //    dvImg.Dispose();
                    //}
                    break;
            }
            return result;
        }

        /// <summary>
        /// Render dv chart and compare with its prev version.
        /// </summary>
        /// <param name="dvJson"></param>
        /// <returns></returns>
        private (string Actual, string Expected, string Diff) RenderAndDiff(string dvJson)
        {
            (string Actual, string Expected, string Diff) result = ("", "", "");
            switch (this._renderType)
            {
                case RenderEngineType.Svg:
                    XElement expectedSvg = this.RenderToSvg(dvJson, LastVersion);
                    XElement actualSvg = this.RenderToSvg(dvJson, CurrentVersion);
                    string svgDiffResult = new SvgDiff().Diff(actualSvg, expectedSvg);

                    result.Actual = actualSvg.ToString();
                    if (!string.IsNullOrEmpty(svgDiffResult))
                    {
                        result.Expected = expectedSvg.ToString();
                        result.Diff = svgDiffResult;
                    }
                    break;

                case RenderEngineType.Imaging:
                    GcBitmap expectedImg = this.RenderToImage(dvJson, LastVersion);
                    GcBitmap actualImg = this.RenderToImage(dvJson, CurrentVersion);
                    string imgDiffResult = new ImageDiff().Diff(actualImg, expectedImg);

                    result.Actual = this.GetBase64String(actualImg);
                    if (!string.IsNullOrEmpty(imgDiffResult))
                    {
                        result.Expected = this.GetBase64String(expectedImg);
                        result.Diff = imgDiffResult;
                    }
                    break;
            }
            return result;
        }

        /// <summary>
        /// Get GcBitmap's base64 string.
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        private string GetBase64String(GcBitmap img)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                img.SaveAsPng(memory);
                return "data:image/png;base64," + System.Convert.ToBase64String(memory.ToArray());
            }
        }

        /// <summary>
        /// Render dv.
        /// </summary>
        /// <param name="dvJson">The dv chart json.</param>
        /// <param name="renderAssembly">The dv chart render assembly.</param>
        /// <returns>Rendered dv chart on img element</returns>
        private GcBitmap RenderToImage(string dvJson, Assembly renderAssembly)
        {
            var type = renderAssembly.GetType("GrapeCity.DataVisualization.Render.Imaging.GcImageRender");
            var instance = Activator.CreateInstance(type);
            var drawMethod = type.GetMethod("Draw", new Type[] { typeof(string) });
            var result = drawMethod.Invoke(instance, new object[] { dvJson });
            return (GcBitmap)result;
        }

        /// <summary>
        /// Render dv.
        /// </summary>
        /// <param name="dvJson">The dv chart json.</param>
        /// <param name="renderAssembly">The dv chart render assembly.</param>
        /// <returns>Rendered dv chart on svg element</returns>
        private XElement RenderToSvg(string dvJson, Assembly renderAssembly)
        {
            //var type = renderAssembly.GetType("GrapeCity.DataVisualization.Render.Svg.SvgRender");
            //var instance = Activator.CreateInstance(type);
            //var drawMethod = type.GetMethod("Draw", new Type[] { typeof(string) });
            //var result = drawMethod.Invoke(instance, new object[] { dvJson });
            //return (XElement)result;
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

        #region Static Methods
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
                string casesDir = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
                string[] files = Directory.GetFiles(casesDir, "*.json", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    Test testCase = new Test()
                    {
                        Id = i + 1,
                        Name = System.IO.Path.GetFileNameWithoutExtension(files[i]),
                        Path = System.IO.Path.GetRelativePath(casesDir, files[i]).Replace("\\", "/"),
                        //Content = System.IO.File.ReadAllText(files[i])
                    };
                    tests.Add(testCase);
                }
                return tests;
            }
        }


        private static Assembly lastVersion;
        /// <summary>
        /// Get dv chart render's last version assembly.
        /// </summary>
        private static Assembly LastVersion
        {
            get
            {
                if (lastVersion == null)
                {
                    string assemblyPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), @"Chart_LastBuild/bin/Debug/netstandard2.0/GrapeCity.DataVisualization.Render.dll");
                    AssemblyLoader assemblyLoader = new AssemblyLoader(assemblyPath);
                    lastVersion = assemblyLoader.Load();
                }
                return lastVersion;
            }
        }

        private static Assembly currentVersion;
        /// <summary>
        /// Get dv chart render's current version assembly.
        /// </summary>
        private static Assembly CurrentVersion
        {
            get
            {
                if (currentVersion == null)
                {
                    string assemblyPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), @"Chart_Build/bin/Debug/netstandard2.0/GrapeCity.DataVisualization.Render.dll");
                    AssemblyLoader assemblyLoader = new AssemblyLoader(assemblyPath);
                    currentVersion = assemblyLoader.Load();
                }
                return currentVersion;
            }
        }
        #endregion


    }
}
