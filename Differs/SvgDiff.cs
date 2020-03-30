using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GrapeCity.DataVisualization.Chart.TestSite
{
    public class SvgDiff : DiffBase
    {
        private static readonly List<string> RANDOM_VALUE_ATTR_NAME = new List<string>() { "id", "clip-path", "fill" };

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
                return this.Error(string.Format("Name\r\nactual: {0}\r\nexpect: {1}", actual.Name.ToString(), expected.Name.ToString()), actual.OuterXml(), expected.OuterXml());
            }

            List<XAttribute> actualAttrs = actual.Attributes().ToList();
            List<XAttribute> expectedAttrs = expected.Attributes().ToList();
            if (!this.DiffAttributes(actualAttrs, expectedAttrs, out string message))
            {
                return this.Error(message, actual.OuterXml(), expected.OuterXml());
            }

            List<XElement> actualChildren = actual.Elements().ToList();
            List<XElement> expectedChildren = expected.Elements().ToList();
            if (actualChildren.Count != expectedChildren.Count)
            {
                return this.Error(string.Format("Child Count\r\nactual: {0}\r\nexpect: {1}", actualChildren.Count, expectedChildren.Count), actual.OuterXml(), expected.OuterXml());
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
        /// Diff attributes.
        /// </summary>
        /// <param name="attr1">The attribute1.</param>
        /// <param name="attr2">The attribute2.</param>
        /// <returns></returns>
        private bool DiffAttributes(List<XAttribute> actualAttrs, List<XAttribute> expectedAttrs, out string message)
        {
            message = string.Empty;
            if (actualAttrs.Count != expectedAttrs.Count)
            {
                message = string.Format("Attribute Count\r\nactual: {0}\r\nexpect: {1}", actualAttrs.Count, expectedAttrs.Count);
                return false;
            }
            Regex numRegex = new Regex("[0-9]");
            for (int i = 0; i < actualAttrs.Count; i++)
            {
                XAttribute actual = actualAttrs[i];
                XAttribute expected = expectedAttrs[i];

                if (actual.Name.ToString() != expected.Name.ToString())
                {
                    message = string.Format("Attribute Name\r\nactual: {0}\r\nexpect: {1}", actual.Name.ToString(), expected.Name.ToString());
                    return false;
                }

                string attrName = actual.Name.ToString();
                string actualAttrValue = actual.Value;
                string expectedAttrValue = expected.Value;
                if (RANDOM_VALUE_ATTR_NAME.Contains(attrName))
                {
                    actualAttrValue = numRegex.Replace(actualAttrValue, "");
                    expectedAttrValue = numRegex.Replace(expectedAttrValue, "");
                }
                if (actualAttrValue != expectedAttrValue)
                {
                    int pos = this.GetDiffPosition(actual.Value, expected.Value);
                    message = string.Format("Attribute Value\r\nactual: {0}\r\nexpect: {1}", actual.Value.Substring(pos), expected.Value.Substring(pos));
                    return false;
                }
            }
            return true;
        }

        private int GetDiffPosition(string text1, string text2)
        {
            int length = Math.Min(text1.Length, text2.Length);
            for (int index = 0; index < length; index++)
            {
                if (text1[index] != text2[index])
                {
                    return index;
                }
            }

            if (length < text1.Length || length < text2.Length)
            {
                return length;
            }
            return -1;
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
