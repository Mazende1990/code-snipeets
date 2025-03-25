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
        [DefaultValue(""), Editor(typeof(ImageFileNameEditor), typeof(UITypeEditor))]
        public string ImageLocation { get; set; }

        [DefaultValue(ContentAlignment.TopLeft), TypeConverter(typeof(EnumProperNameConverter))]
        public ContentAlignment Placement { get; set; }

        [DefaultValue(typeof(Point), "0, 0")]
        public Point Offset { get; set; }

        [DefaultValue(DrawImageSizeMode.DontResize), 
         Description("How the image watermark should be rescaled, if at all."), 
         TypeConverter(typeof(EnumDescriptionConverter))]
        public DrawImageSizeMode SizeMode { get; set; }

        [DefaultValue(typeof(Size), "0, 0")]
        public Size Size { get; set; }

        [DefaultValue(ImageRotateFlipType.None), TypeConverter(typeof(EnumProperNameKeepCaseConverter))]
        public ImageRotateFlipType RotateFlip { get; set; }

        [DefaultValue(false)]
        public bool Tile { get; set; }

        [DefaultValue(false), 
         Description("If image watermark size bigger than source image then don't draw it.")]
        public bool AutoHide { get; set; }

        [DefaultValue(ImageInterpolationMode.HighQualityBicubic), 
         TypeConverter(typeof(EnumProperNameConverter))]
        public ImageInterpolationMode InterpolationMode { get; set; }

        [DefaultValue(CompositingMode.SourceOver), 
         TypeConverter(typeof(EnumProperNameConverter))]
        public CompositingMode CompositingMode { get; set; }

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

        public override Bitmap Apply(Bitmap bmp)
        {
            if (ShouldSkipProcessing())
            {
                return bmp;
            }

            string imageFilePath = Helpers.ExpandFolderVariables(ImageLocation, true);

            if (!IsValidImagePath(imageFilePath))
            {
                return bmp;
            }

            using (Bitmap watermarkImage = LoadAndPrepareWatermark(imageFilePath))
            {
                if (watermarkImage == null)
                {
                    return bmp;
                }

                Rectangle destinationRect = CalculateDestinationRectangle(bmp, watermarkImage);

                if (ShouldSkipDrawing(bmp, destinationRect))
                {
                    return bmp;
                }

                DrawWatermark(bmp, watermarkImage, destinationRect);
            }

            return bmp;
        }

        private bool ShouldSkipProcessing()
        {
            return Opacity < 1 || 
                  (SizeMode != DrawImageSizeMode.DontResize && Size.Width <= 0 && Size.Height <= 0);
        }

        private bool IsValidImagePath(string imagePath)
        {
            return !string.IsNullOrEmpty(imagePath) && File.Exists(imagePath);
        }

        private Bitmap LoadAndPrepareWatermark(string imagePath)
        {
            Bitmap watermark = ImageHelpers.LoadImage(imagePath);

            if (watermark != null && RotateFlip != ImageRotateFlipType.None)
            {
                watermark.RotateFlip((RotateFlipType)RotateFlip);
            }

            return watermark;
        }

        private Rectangle CalculateDestinationRectangle(Bitmap sourceImage, Bitmap watermark)
        {
            Size imageSize = CalculateWatermarkSize(sourceImage, watermark);
            Point position = Helpers.GetPosition(Placement, Offset, sourceImage.Size, imageSize);
            
            return new Rectangle(position, imageSize);
        }

        private Size CalculateWatermarkSize(Bitmap sourceImage, Bitmap watermark)
        {
            switch (SizeMode)
            {
                case DrawImageSizeMode.AbsoluteSize:
                    int width = Size.Width == -1 ? sourceImage.Width : Size.Width;
                    int height = Size.Height == -1 ? sourceImage.Height : Size.Height;
                    return ImageHelpers.ApplyAspectRatio(width, height, watermark);

                case DrawImageSizeMode.PercentageOfWatermark:
                    int wmWidth = (int)Math.Round(Size.Width / 100f * watermark.Width);
                    int wmHeight = (int)Math.Round(Size.Height / 100f * watermark.Height);
                    return ImageHelpers.ApplyAspectRatio(wmWidth, wmHeight, watermark);

                case DrawImageSizeMode.PercentageOfCanvas:
                    int canvasWidth = (int)Math.Round(Size.Width / 100f * sourceImage.Width);
                    int canvasHeight = (int)Math.Round(Size.Height / 100f * sourceImage.Height);
                    return ImageHelpers.ApplyAspectRatio(canvasWidth, canvasHeight, watermark);

                default:
                    return watermark.Size;
            }
        }

        private bool ShouldSkipDrawing(Bitmap sourceImage, Rectangle destinationRect)
        {
            return AutoHide && !new Rectangle(0, 0, sourceImage.Width, sourceImage.Height).Contains(destinationRect);
        }

        private void DrawWatermark(Bitmap sourceImage, Bitmap watermark, Rectangle destinationRect)
        {
            using (Graphics g = Graphics.FromImage(sourceImage))
            {
                ConfigureGraphics(g);

                if (Tile)
                {
                    DrawTiledWatermark(g, watermark, destinationRect);
                }
                else if (Opacity < 100)
                {
                    DrawTransparentWatermark(g, watermark, destinationRect);
                }
                else
                {
                    g.DrawImage(watermark, destinationRect);
                }
            }
        }

        private void ConfigureGraphics(Graphics g)
        {
            g.InterpolationMode = ImageHelpers.GetInterpolationMode(InterpolationMode);
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.CompositingMode = CompositingMode;
        }

        private void DrawTiledWatermark(Graphics g, Bitmap watermark, Rectangle destinationRect)
        {
            using (TextureBrush brush = new TextureBrush(watermark, WrapMode.Tile))
            {
                brush.TranslateTransform(destinationRect.X, destinationRect.Y);
                g.FillRectangle(brush, destinationRect);
            }
        }

        private void DrawTransparentWatermark(Graphics g, Bitmap watermark, Rectangle destinationRect)
        {
            using (ImageAttributes attributes = new ImageAttributes())
            {
                ColorMatrix matrix = ColorMatrixManager.Alpha(Opacity / 100f);
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                g.DrawImage(
                    watermark, 
                    destinationRect, 
                    0, 0, watermark.Width, watermark.Height, 
                    GraphicsUnit.Pixel, 
                    attributes);
            }
        }
    }
}