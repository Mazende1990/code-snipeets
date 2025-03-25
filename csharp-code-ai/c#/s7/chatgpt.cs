using ShareX.HelpersLib;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace ShareX.ImageEffectsLib
{
    [Description("Image")]
    public class DrawImage : ImageEffect
    {
        // File & Position
        [DefaultValue(""), Editor(typeof(ImageFileNameEditor), typeof(UITypeEditor))]
        public string ImageLocation { get; set; }

        [DefaultValue(ContentAlignment.TopLeft), TypeConverter(typeof(EnumProperNameConverter))]
        public ContentAlignment Placement { get; set; }

        [DefaultValue(typeof(Point), "0, 0")]
        public Point Offset { get; set; }

        // Size & Scaling
        [DefaultValue(DrawImageSizeMode.DontResize), Description("How the image watermark should be rescaled, if at all."), TypeConverter(typeof(EnumDescriptionConverter))]
        public DrawImageSizeMode SizeMode { get; set; }

        [DefaultValue(typeof(Size), "0, 0")]
        public Size Size { get; set; }

        // Transformation
        [DefaultValue(ImageRotateFlipType.None), TypeConverter(typeof(EnumProperNameKeepCaseConverter))]
        public ImageRotateFlipType RotateFlip { get; set; }

        [DefaultValue(false)]
        public bool Tile { get; set; }

        [DefaultValue(false), Description("If image watermark size bigger than source image then don't draw it.")]
        public bool AutoHide { get; set; }

        // Rendering
        [DefaultValue(ImageInterpolationMode.HighQualityBicubic), TypeConverter(typeof(EnumProperNameConverter))]
        public ImageInterpolationMode InterpolationMode { get; set; }

        [DefaultValue(CompositingMode.SourceOver), TypeConverter(typeof(EnumProperNameConverter))]
        public CompositingMode CompositingMode { get; set; }

        // Opacity
        private int opacity = 100;

        [DefaultValue(100)]
        public int Opacity
        {
            get => opacity;
            set => opacity = value.Clamp(0, 100);
        }

        public DrawImage()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap sourceBitmap)
        {
            if (!ShouldDraw())
                return sourceBitmap;

            string imagePath = Helpers.ExpandFolderVariables(ImageLocation, true);

            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                return sourceBitmap;

            using (Bitmap watermarkBitmap = ImageHelpers.LoadImage(imagePath))
            {
                if (watermarkBitmap == null)
                    return sourceBitmap;

                ApplyRotation(watermarkBitmap);

                Size finalSize = CalculateWatermarkSize(sourceBitmap, watermarkBitmap);
                Point finalPosition = Helpers.GetPosition(Placement, Offset, sourceBitmap.Size, finalSize);
                Rectangle drawRect = new Rectangle(finalPosition, finalSize);

                if (AutoHide && !new Rectangle(Point.Empty, sourceBitmap.Size).Contains(drawRect))
                    return sourceBitmap;

                using (Graphics g = Graphics.FromImage(sourceBitmap))
                {
                    g.InterpolationMode = ImageHelpers.GetInterpolationMode(InterpolationMode);
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.CompositingMode = CompositingMode;

                    if (Tile)
                    {
                        using (TextureBrush brush = new TextureBrush(watermarkBitmap, WrapMode.Tile))
                        {
                            brush.TranslateTransform(finalPosition.X, finalPosition.Y);
                            g.FillRectangle(brush, drawRect);
                        }
                    }
                    else
                    {
                        DrawWithOpacity(g, watermarkBitmap, drawRect);
                    }
                }
            }

            return sourceBitmap;
        }

        private bool ShouldDraw()
        {
            if (Opacity < 1)
                return false;

            if (SizeMode != DrawImageSizeMode.DontResize && Size.Width <= 0 && Size.Height <= 0)
                return false;

            return true;
        }

        private void ApplyRotation(Bitmap bmp)
        {
            if (RotateFlip != ImageRotateFlipType.None)
            {
                bmp.RotateFlip((RotateFlipType)RotateFlip);
            }
        }

        private Size CalculateWatermarkSize(Bitmap canvas, Bitmap watermark)
        {
            switch (SizeMode)
            {
                case DrawImageSizeMode.AbsoluteSize:
                    int width = Size.Width == -1 ? canvas.Width : Size.Width;
                    int height = Size.Height == -1 ? canvas.Height : Size.Height;
                    return ImageHelpers.ApplyAspectRatio(width, height, watermark);

                case DrawImageSizeMode.PercentageOfWatermark:
                    int w1 = (int)Math.Round(Size.Width / 100f * watermark.Width);
                    int h1 = (int)Math.Round(Size.Height / 100f * watermark.Height);
                    return ImageHelpers.ApplyAspectRatio(w1, h1, watermark);

                case DrawImageSizeMode.PercentageOfCanvas:
                    int w2 = (int)Math.Round(Size.Width / 100f * canvas.Width);
                    int h2 = (int)Math.Round(Size.Height / 100f * canvas.Height);
                    return ImageHelpers.ApplyAspectRatio(w2, h2, watermark);

                default:
                    return watermark.Size;
            }
        }

        private void DrawWithOpacity(Graphics g, Bitmap bmp, Rectangle rect)
        {
            if (Opacity < 100)
            {
                using (ImageAttributes attributes = new ImageAttributes())
                {
                    ColorMatrix matrix = ColorMatrixManager.Alpha(Opacity / 100f);
                    attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    g.DrawImage(bmp, rect, 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            else
            {
                g.DrawImage(bmp, rect);
            }
        }
    }
}
