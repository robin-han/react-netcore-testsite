using GrapeCity.Documents.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrapeCity.DataVisualization.Chart.TestSite
{
    public class ImageDiff : DiffBase
    {
        /// <summary>
        /// Compares the two images.
        /// </summary>
        /// <param name="actualImage">The actual image.</param>
        /// <param name="expectImage">The expected image.</param>
        /// <returns>Compared result.</returns>
        public string Diff(GcBitmap actualImage, GcBitmap expectImage)
        {
            int width = actualImage.PixelWidth;
            int height = actualImage.PixelHeight;
            if (width != expectImage.PixelWidth && height != expectImage.PixelHeight)
            {
                return this.Error("Not same size", (width + "," + height), (expectImage.PixelWidth + "," + expectImage.PixelHeight));
            }

            bool flag = false;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (actualImage[i, j] != expectImage[i, j])
                    {
                        actualImage[i, j] = this.AddOpacity(actualImage[i, j]);
                        flag = true;
                    }
                }
            }
            return flag ? "Image different" : "";
        }

        /// <summary>
        ///  Add opacity to color.
        /// </summary>
        private uint AddOpacity(uint pixel, double opacity = 0.3)
        {
            uint a = System.Convert.ToUInt32((pixel >> 24) * 0.3);
            return (a << 24) | (pixel & 0x00ffffff);
        }

        /// <summary>
        /// Invert color.
        /// </summary>
        private uint InvertColor(uint pixel)
        {
            uint a = pixel >> 24;
            uint r = pixel << 8 >> 24;
            uint g = pixel << 16 >> 24;
            uint b = pixel << 24 >> 24;

            return ((255 - a) << 24) | ((255 - r) << 16) | ((255 - g) << 8) | (255 - b);
        }
    }
}

