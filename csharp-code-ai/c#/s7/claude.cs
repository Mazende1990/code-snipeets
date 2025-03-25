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
        // Property declarations remain the same
        [DefaultValue(""), Editor(typeof(ImageFileNameEditor), typeof(UITypeEditor))]
        public string ImageLocation { get; set; }

        [DefaultValue(ContentAlignment.TopLeft), TypeConverter(typeof(EnumProperNameConverter))]
        public ContentAlignment Placement { get; set; }

        [DefaultValue(typeof(Point), "0, 0")]
        public Point Offset { get; set; }

        [DefaultValue(DrawImageSizeMode.DontResize), Description("How the image watermark should be rescaled, if at all."), TypeConverter(typeof(EnumDescriptionConverter))]
        public DrawImageSizeMode SizeMode { get; set; }

        [DefaultValue(typeof(Size), "0, 0")]
        public Size Size { get; set; }

        [DefaultValue(ImageRotateFlipType.None), TypeConverter(typeof(EnumProperNameKeepCaseConverter))]
        public ImageRotateFlipType RotateFlip { get; set; }

        [DefaultValue(false)]
        public bool Tile { get; set; }

        [DefaultValue(false), Description("If image watermark size bigger than source image then don't draw it.")]
        public bool AutoHide { get; set; }

        [DefaultValue(ImageInterpolationMode.HighQualityBicubic), TypeConverter(typeof(EnumProperNameConverter))]
        public ImageInterpolationMode InterpolationMode { get; set; }

        [DefaultValue(CompositingMode.SourceOver), TypeConverter(typeof(EnumProperNameConverter))]
        public CompositingMode CompositingMode { get; set; }

        private int opacity;

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

        public override Bitmap Apply(Bitmap bmp)
        {
            // Early exit conditions
            if (ShouldSkipDrawing())
            {
                return bmp;
            }

            // Expand and validate image file path
            string imageFilePath = Helpers.ExpandFolderVariables(ImageLocation, true);
            if (string.IsNullOrEmpty(imageFilePath) || !File.Exists(imageFilePath))
            {
                return bmp;
            }

            using (Bitmap bmpWatermark = ImageHelpers.LoadImage(imageFilePath))
            {
                if (bmpWatermark == null)
                {
                    return bmp;
                }

                return ApplyWatermark(bmp, bmpWatermark);
            }
        }

        private bool ShouldSkipDrawing()
        {
            return Opacity < 1 || 
                   (SizeMode != DrawImageSizeMode.DontResize && 
                    Size.Width <= 0 && 
                    Size.Height <= 0);
        }

        private Bitmap ApplyWatermark(Bitmap originalBitmap, Bitmap watermarkBitmap)
        {
            // Apply rotate/flip if specified
            if (RotateFlip != ImageRotateFlipType.None)
            {
                watermarkBitmap.RotateFlip((RotateFlipType)RotateFlip);
            }

            // Calculate watermark size based on size mode
            Size watermarkSize = CalculateWatermarkSize(originalBitmap, watermarkBitmap);

            // Determine watermark position
            Point watermarkPosition = Helpers.GetPosition(Placement, Offset, originalBitmap.Size, watermarkSize);
            Rectangle watermarkRectangle = new Rectangle(watermarkPosition, watermarkSize);

            // Check if watermark should be hidden
            if (AutoHide && !new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height).Contains(watermarkRectangle))
            {
                return originalBitmap;
            }

            // Draw watermark
            return DrawWatermarkOnBitmap(originalBitmap, watermarkBitmap, watermarkRectangle);
        }

        private Size CalculateWatermarkSize(Bitmap originalBitmap, Bitmap watermarkBitmap)
        {
            return SizeMode switch
            {
                DrawImageSizeMode.AbsoluteSize => 
                    ImageHelpers.ApplyAspectRatio(
                        Size.Width == -1 ? originalBitmap.Width : Size.Width, 
                        Size.Height == -1 ? originalBitmap.Height : Size.Height, 
                        watermarkBitmap
                    ),
                DrawImageSizeMode.PercentageOfWatermark => 
                    ImageHelpers.ApplyAspectRatio(
                        (int)Math.Round(Size.Width / 100f * watermarkBitmap.Width),
                        (int)Math.Round(Size.Height / 100f * watermarkBitmap.Height), 
                        watermarkBitmap
                    ),
                DrawImageSizeMode.PercentageOfCanvas => 
                    ImageHelpers.ApplyAspectRatio(
                        (int)Math.Round(Size.Width / 100f * originalBitmap.Width),
                        (int)Math.Round(Size.Height / 100f * originalBitmap.Height), 
                        watermarkBitmap
                    ),
                _ => watermarkBitmap.Size
            };
        }

        private Bitmap DrawWatermarkOnBitmap(Bitmap originalBitmap, Bitmap watermarkBitmap, Rectangle watermarkRectangle)
        {
            using (Graphics g = Graphics.FromImage(originalBitmap))
            {
                ConfigureGraphics(g);

                if (Tile)
                {
                    DrawTiledWatermark(g, watermarkBitmap, watermarkRectangle);
                }
                else if (Opacity < 100)
                {
                    DrawTransparentWatermark(g, watermarkBitmap, watermarkRectangle);
                }
                else
                {
                    g.DrawImage(watermarkBitmap, watermarkRectangle);
                }
            }

            return originalBitmap;
        }

        private void ConfigureGraphics(Graphics graphics)
        {
            graphics.InterpolationMode = ImageHelpers.GetInterpolationMode(InterpolationMode);
            graphics.PixelOffsetMode = PixelOffsetMode.Half;
            graphics.CompositingMode = CompositingMode;
        }

        private void DrawTiledWatermark(Graphics graphics, Bitmap watermarkBitmap, Rectangle watermarkRectangle)
        {
            using (TextureBrush brush = new TextureBrush(watermarkBitmap, WrapMode.Tile))
            {
                brush.TranslateTransform(watermarkRectangle.X, watermarkRectangle.Y);
                graphics.FillRectangle(brush, watermarkRectangle);
            }
        }

        private void DrawTransparentWatermark(Graphics graphics, Bitmap watermarkBitmap, Rectangle watermarkRectangle)
        {
            using (ImageAttributes imageAttributes = new ImageAttributes())
            {
                ColorMatrix matrix = ColorMatrixManager.Alpha(Opacity / 100f);
                imageAttributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                
                graphics.DrawImage(
                    watermarkBitmap, 
                    watermarkRectangle, 
                    0, 0, 
                    watermarkBitmap.Width, watermarkBitmap.Height, 
                    GraphicsUnit.Pixel, 
                    imageAttributes
                );
            }
        }
    }
}