using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GrapeCity.DataVisualization.Chart.TestSite
{
    public class SvgDiff
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SvgDiff()
        {
        }

        /// <summary>
        /// Diff the two elements.
        /// </summary>
        /// <param name="expected">The element1.</param>
        /// <param name="actual">The element2.</param>
        /// <returns>The different text.</returns>
        public string Diff(XElement actual, XElement expected)
        {
            if (actual.Name.ToString() != expected.Name.ToString())
            {
                return this.Error(
                    string.Format(
                        "Name\r\nactual: {0}\r\nexpect: {1}",
                        actual.Name.ToString(),
                        expected.Name.ToString()),
                    actual.OuterXml(),
                    expected.OuterXml());
            }

            List<XAttribute> actualAttrs = this.FilterAttributes(actual.Attributes().ToList());
            List<XAttribute> expectedAttrs = this.FilterAttributes(expected.Attributes().ToList());
            string diffMessage = this.DiffAttributes(actualAttrs, expectedAttrs);
            if (!string.IsNullOrEmpty(diffMessage))
            {
                return this.Error(diffMessage, actual.OuterXml(), expected.OuterXml());
            }

            List<XElement> actualChildren = this.FilterElements(actual.Elements().ToList());
            List<XElement> expectedChildren = this.FilterElements(expected.Elements().ToList());
            if (actualChildren.Count != expectedChildren.Count)
            {
                return this.Error(
                    string.Format(
                        "Child Count\r\nactual: {0}\r\nexpect: {1}",
                        actualChildren.Count,
                        expectedChildren.Count),
                    actual.OuterXml(),
                    expected.OuterXml());
            }

            for (int i = 0; i < actualChildren.Count; i++)
            {
                string diff = this.Diff(actualChildren[i], expectedChildren[i]);
                if (!string.IsNullOrEmpty(diff))
                {
                    return diff;
                }
            }

            return "";
        }

        /// <summary>
        /// Format error message.
        /// </summary>
        /// <param name="actual">The actual message.</param>
        /// <param name="expected">The </param>
        /// <returns></returns>
        private string Error(string message, string actual, string expected)
        {
            return string.Format(
                "{0}\r\n\r\nActual:\r\n{1}\r\n\r\nExpected:\r\n{2}\r\n------",
                message,
                actual,
                expected
            );
        }

        /// <summary>
        /// Diff attributes.
        /// </summary>
        /// <param name="attr1">The attribute1.</param>
        /// <param name="attr2">The attribute2.</param>
        /// <returns>The diff string.</returns>
        private string DiffAttributes(List<XAttribute> actualAttrs, List<XAttribute> expectedAttrs)
        {
            if (actualAttrs.Count != expectedAttrs.Count)
            {
                return string.Format(
                    "Attribute Count\r\nactual: {0}\r\nexpect: {1}",
                    actualAttrs.Count,
                    expectedAttrs.Count
                );
            }

            for (int i = 0; i < actualAttrs.Count; i++)
            {
                XAttribute actual = actualAttrs[i];
                XAttribute expected = expectedAttrs[i];
                if (actual.Name.ToString() != expected.Name.ToString())
                {
                    return string.Format(
                        "Attribute Name\r\nactual: {0}\r\nexpect: {1}",
                        actual.Name.ToString(),
                        expected.Name.ToString()
                    );
                }

                if (actual.Value != expected.Value)
                {
                    return string.Format(
                        "Attribute Value\r\nactual: {0}\r\nexpect: {1}",
                        actual.Value,
                        expected.Value
                    );
                }
            }

            return string.Empty;
        }

        private List<XAttribute> FilterAttributes(List<XAttribute> attrs)
        {
            return attrs.Where((att) =>
            {
                string name = att.Name.ToString();
                return name != "xmlns" && name != "clip-path";
            }).ToList();
        }

        private List<XElement> FilterElements(List<XElement> elems)
        {
            return elems.Where((att) =>
            {
                string name = att.Name.LocalName;
                return name != "clipPath";
            }).ToList();
        }
    }


    internal static class XElementExtensions
    {
        public static string OuterXml(this XElement thiz)
        {
            var xReader = thiz.CreateReader();
            xReader.MoveToContent();
            return xReader.ReadOuterXml();
        }

        public static string InnerXml(this XElement thiz)
        {
            var xReader = thiz.CreateReader();
            xReader.MoveToContent();
            return xReader.ReadInnerXml();
        }
    }

}
