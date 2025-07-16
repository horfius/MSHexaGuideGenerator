using MSHexaGuideGen.Models;
using MSHexaGuideGen.Util;
using MSHexaGuideGenerator.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSHexaGuideGen.Processors
{
    public class GuideImageProcessor
    {
        private GuideCanvas GuideCanvas { get; set; }

        private SkillOrder[] SkillOrders { get; set; }

        public GuideImageProcessor(GuideCanvas guideCanvas, SkillOrder[] skillOrders) 
        {
            GuideCanvas = guideCanvas ?? throw new ArgumentNullException(nameof(guideCanvas));
            SkillOrders = skillOrders ?? throw new ArgumentNullException(nameof(skillOrders));
        }

        private FontFamily CustomFontFamily
        {
            get
            {
                //PrivateFontCollection fontCollection = new PrivateFontCollection();
                //fontCollection.AddFontFile(Path.Combine(Environment.CurrentDirectory, "INKFREE.TTF"));
                //return new FontFamily("Ink Free", fontCollection);
                return new FontFamily(GenericFontFamilies.SansSerif);
            }
        }

        public async Task<Image> Process()
        {
            var legendAnchor = GuideCanvas.Legend.Anchor.ToLegendAnchor();
            var isLegendPresent = legendAnchor != LegendAnchor.Invalid;
            var skillImageWidth = 128;
            var skillImageHeight = 128;
            var cyclesX = 8;
            var rows = (int)Math.Ceiling((SkillOrders.Length + 1) / (double)cyclesX);
            var cyclesY = rows + 1;
            var cycleOffsetX = 220;
            var cycleOffsetY = 190;
            var imageWidth = 2000;
            var legendWidth = imageWidth / 2; // Included in the imageWidth
            var legendHeight = GuideCanvas.Legend.Contents.Length * cycleOffsetY + 64; // Padding
            var baseX = (imageWidth - cyclesX * cycleOffsetX) / 2 + (cycleOffsetX - skillImageWidth) / 2;
            var legendBaseX = baseX + (legendAnchor.IsLeftAnchored() ? 0 : legendWidth);
            var baseY = 200;
            var legendBaseY = baseY + SkillOrders.Length / cyclesX * cycleOffsetY + 64;
            var imageHeight = baseY + cyclesY * cycleOffsetY - 47 + (isLegendPresent ? legendHeight - cycleOffsetY : 0);
            Color smokyBlack = Color.FromArgb(0x1A, 0x1A, 0x1A);

            var canvas = new Bitmap(imageWidth, imageHeight);
            using var fontFamily = CustomFontFamily;
            using var fontSmall = new Font(fontFamily, 20, FontStyle.Regular, GraphicsUnit.Pixel);
            using var fontMedium = new Font(fontFamily, 30, FontStyle.Regular, GraphicsUnit.Pixel);
            using var fontLarge = new Font(fontFamily, 40, FontStyle.Regular, GraphicsUnit.Pixel);
            using var fontXLarge = new Font(fontFamily, 60, FontStyle.Regular, GraphicsUnit.Pixel);

            using var textBrush = new HatchBrush(HatchStyle.Percent50, Color.White, Color.Black);

            using Graphics g = Graphics.FromImage(canvas);

            g.Clear(Color.Black);

            var bgImage = await GuideCanvas.Background.GetImage();
            var blurredBgImage = Blur((Bitmap)bgImage, 4);

            g.DrawImage(blurredBgImage, new Rectangle(0, 0, canvas.Width, canvas.Height));

            using var headerTextFormat = new StringFormat();
            headerTextFormat.Alignment = StringAlignment.Center;
            headerTextFormat.LineAlignment = StringAlignment.Center;
            Rectangle headerRect = new Rectangle(0, 0, imageWidth, baseY);
            DrawOutlinedText(g, GuideCanvas.HeaderText, headerTextFormat, fontXLarge, headerRect, Color.White, smokyBlack);

            using var versionTextFormat = new StringFormat();
            versionTextFormat.LineAlignment = StringAlignment.Far;
            Rectangle versionRect = new Rectangle(0, imageHeight - baseY, imageWidth, baseY);
            if (legendAnchor != LegendAnchor.BottomRight)
            {
                versionTextFormat.Alignment = StringAlignment.Far;
            }
            else
            {
                versionTextFormat.Alignment = StringAlignment.Near;
            }
            DrawOutlinedText(g, $"Patch {GuideCanvas.Version}", versionTextFormat, fontLarge, versionRect, Color.White, Color.Black);

            using var disposables = new CompositeDisposable();
            disposables.AddDisposable(GuideCanvas.Background);
            disposables.AddRangeDisposables(GuideCanvas.SkillImages);
            foreach(var skillImage in GuideCanvas.SkillImages)
            {
                await skillImage.GetImage();
            }

            for (var i = 0; i < SkillOrders.Length; i++)
            {
                var position = i + 1;
                // Vertical alignment
                var rowOffset = position / cyclesX;
                // Horizontal alignment
                var columnOffset = position % cyclesX;

                var imageRect = new Rectangle(baseX + columnOffset * cycleOffsetX, baseY + rowOffset * cycleOffsetY, skillImageWidth, skillImageHeight);
                var imageTextRect = new Rectangle(baseX + columnOffset * cycleOffsetX, baseY + rowOffset * cycleOffsetY + skillImageHeight, skillImageHeight, 48);
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.Low;
                g.DrawImage(await GuideCanvas.GetImage(SkillOrders[i].SkillName).GetImage(), imageRect);

                using var levelTextFormat = new StringFormat();
                levelTextFormat.Alignment = StringAlignment.Center;
                DrawOutlinedText(g, $"Lv {SkillOrders[i].Level}", levelTextFormat, fontLarge, imageTextRect, Color.White, smokyBlack);
            }

            if (isLegendPresent)
            {
                using var legendTextFormat = new StringFormat();
                legendTextFormat.Alignment = StringAlignment.Center;
                legendTextFormat.LineAlignment = StringAlignment.Center;
                Rectangle legendTextRect = new Rectangle(legendBaseX - baseX, legendBaseY, legendWidth, legendHeight);
                DrawOutlinedText(g, GuideCanvas.Legend.Header, legendTextFormat, fontXLarge, legendTextRect, Color.White, Color.Black);

                for (var i = 0; i < GuideCanvas.Legend.Contents.Length; i++)
                {
                    if (!string.IsNullOrEmpty(GuideCanvas.Legend.Contents[i].Icon))
                    {
                        var imageRect = new Rectangle(legendBaseX, legendBaseY + (i + 1) * cycleOffsetY, skillImageWidth, skillImageHeight);
                        var textRect = new Rectangle(legendBaseX + skillImageWidth + 16,
                            legendBaseY + (i + 1) * cycleOffsetY, // Padding from header
                            legendWidth - skillImageWidth - baseX - 64,
                            skillImageHeight);
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.InterpolationMode = InterpolationMode.Low;
                        g.DrawImage(await GuideCanvas.GetImage(GuideCanvas.Legend.Contents[i].Icon).GetImage(), imageRect);

                        using var textFormat = new StringFormat();
                        textFormat.Alignment = StringAlignment.Near;
                        textFormat.LineAlignment = StringAlignment.Center;
                        DrawOutlinedText(g, GuideCanvas.Legend.Contents[i].Text, textFormat, fontMedium, textRect, Color.White, Color.Black);
                    }
                }
            }

            return canvas!;
        }

        private static void DrawOutlinedText(Graphics graphics, string text, StringFormat stringFormat, Font font, Rectangle textRect, Color textColor, Color backgroundColor)
        {
            using Pen pen = new Pen(backgroundColor, 8);
            pen.LineJoin = LineJoin.Round;

            Rectangle fr = new Rectangle(textRect.X, textRect.Y - font.Height, textRect.Width, font.Height);
            using LinearGradientBrush brush = new LinearGradientBrush(fr, textColor, textColor, 90);

            using GraphicsPath path = new GraphicsPath();
            path.AddString(text, font.FontFamily, (int)FontStyle.Regular, font.Size, textRect, stringFormat);

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.InterpolationMode = InterpolationMode.High;

            graphics.DrawPath(pen, path);
            graphics.FillPath(brush, path);
        }

        private static Bitmap Blur(Bitmap image, Int32 blurSize)
        {
            return Blur(image, new Rectangle(0, 0, image.Width, image.Height), blurSize);
        }

        private unsafe static Bitmap Blur(Bitmap image, Rectangle rectangle, Int32 blurSize)
        {
            Bitmap blurred = new Bitmap(image.Width, image.Height);

            // make an exact copy of the bitmap provided
            using (Graphics graphics = Graphics.FromImage(blurred))
                graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                    new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);

            // Lock the bitmap's bits
            BitmapData blurredData = blurred.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, blurred.PixelFormat);

            // Get bits per pixel for current PixelFormat
            int bitsPerPixel = Image.GetPixelFormatSize(blurred.PixelFormat);

            // Get pointer to first line
            byte* scan0 = (byte*)blurredData.Scan0.ToPointer();

            // look at every pixel in the blur rectangle
            for (int xx = rectangle.X; xx < rectangle.X + rectangle.Width; xx++)
            {
                for (int yy = rectangle.Y; yy < rectangle.Y + rectangle.Height; yy++)
                {
                    int avgR = 0, avgG = 0, avgB = 0;
                    int blurPixelCount = 0;

                    // average the color of the red, green and blue for each pixel in the
                    // blur size while making sure you don't go outside the image bounds
                    for (int x = xx; (x < xx + blurSize && x < image.Width); x++)
                    {
                        for (int y = yy; (y < yy + blurSize && y < image.Height); y++)
                        {
                            // Get pointer to RGB
                            byte* data = scan0 + y * blurredData.Stride + x * bitsPerPixel / 8;

                            avgB += data[0]; // Blue
                            avgG += data[1]; // Green
                            avgR += data[2]; // Red

                            blurPixelCount++;
                        }
                    }

                    avgR = avgR / blurPixelCount;
                    avgG = avgG / blurPixelCount;
                    avgB = avgB / blurPixelCount;

                    // now that we know the average for the blur size, set each pixel to that color
                    for (int x = xx; x < xx + blurSize && x < image.Width && x < rectangle.Width; x++)
                    {
                        for (int y = yy; y < yy + blurSize && y < image.Height && y < rectangle.Height; y++)
                        {
                            // Get pointer to RGB
                            byte* data = scan0 + y * blurredData.Stride + x * bitsPerPixel / 8;

                            // Change values
                            data[0] = (byte)avgB;
                            data[1] = (byte)avgG;
                            data[2] = (byte)avgR;
                        }
                    }
                }
            }

            // Unlock the bits
            blurred.UnlockBits(blurredData);

            return blurred;
        }
    }
}
