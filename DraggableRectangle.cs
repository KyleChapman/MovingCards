// Author:  Kyle Chapman
// Created: February 6, 2026
// Updated: February 17, 2026
// Description:
// This is an approach to implementing cards or other draggable elements
// around a user interface. While originally I wanted to support generated
// cards, I thought it would be more interesting to allow for text-based
// cards or image-based cards. So this class allows both using different
// constructors.

// Note the format of this using statement for accessing our new Interface(s).
using MovingCards.Interfaces;
using MovingCards.Exceptions;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace MovingCards
{
    public class DraggableRectangle : IDraggable
    {
        // Constants for the sizes of the cards.
        // Originally I wanted them to have different default and minimum sizes
        // but right now they're the same, so ㄟ( ▔, ▔ )ㄏ.
        public const int DefaultWidth = 180;
        public const int DefaultHeight = 252;
        public const int MinWidth = 180; 
        public const int MinHeight = 252;

        /// <summary>
        /// The Content property of a card is always contained as an ImageSource (nullable), even when the card has text.
        /// </summary>
        public ImageSource? Content { get; private set; }
        /// <summary>
        /// The background colour of a card.
        /// </summary>
        public Brush Background { get; }

        /// <summary>
        /// The Size of a card as a built-in Size. (Although it's always the same.)
        /// </summary>
        public Size Size => new Size(DefaultWidth, DefaultHeight);
        /// <summary>
        /// The Position of a card as a built-in Point.
        /// </summary>
        public Point Position { get; set; }

        /// <summary>
        /// Parametrized constructor for DraggableRectangle with an image.
        /// </summary>
        /// <param name="image">An image source - e.g. a URI</param>
        /// <param name="background">A background colour Brush</param>
        /// <param name="initialPosition">A initial position</param>
        public DraggableRectangle(ImageSource image, Brush? background = null, Point? initialPosition = null)
        {
            Content = image;
            // There's a default background colour.
            Background = background ?? Brushes.Gainsboro;
            // Rather arbitrary default position.
            Position = initialPosition ?? new Point(40, 40);
        }

        /// <summary>
        /// Parametrized constructor for DraggableRectangle with text.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="background">A background colour Brush</param>
        /// <param name="initialPosition">A initial position</param>
        /// <param name="textBrush">A text colour Brush</param>
        public DraggableRectangle(string text, Brush? background = null, Point? initialPosition = null, Brush? textBrush = null, Thickness? textPadding = null)
        {
            // There's a default background colour.
            Background = background ?? Brushes.Gainsboro;
            // Rather arbitrary default position.
            Position = initialPosition ?? new Point(40, 40);

            Content = RenderTextToImage(text, foreground: textBrush ?? Brushes.Black, new Thickness(0));
        }


        /// <summary>
        /// From IDraggable, although for Card objects they are always draggable. If this were, say, lightboxes with headers or cards with grip indicators, you might want them to only be movable when the header is clicked.
        /// </summary>
        /// <param name="localStart">The start point that is clicked.</param>
        /// <returns>true</returns>
        public bool CanDrag(Point localStart)
        {
            return true;
        }

        /// <summary>
        /// From IDraggable, although we don't really need to do anything with this. This COULD be used to indicate that a UI element is "selected" or something.
        /// </summary>
        /// <param name="localStart">Start location on the application</param>
        /// <param name="surfaceStart">Start location on the surface</param>
        public void BeginDrag(Point localStart, Point surfaceStart)
        {
        }

        /// <summary>
        /// From IDraggable, indicate how far something needs to be dragged.
        /// </summary>
        /// <param name="surfaceDelta">How far to drag the object.</param>
        public void DragBy(Vector surfaceDelta)
        {
            Position = new Point(Position.X + surfaceDelta.X, Position.Y + surfaceDelta.Y);
        }

        /// <summary>
        /// From IDraggable, although we don't really need to do anything with this. This COULD be used to snap a piece to a grid, or check the validity of a move.
        /// </summary>
        public void EndDrag()
        {
            if (this.Position.X < 0 || this.Position.Y < 0)
            {
                throw new InvalidMoveException("Rectangle cannot be partly off-screen.");
            }
        }

        /// <summary>
        /// Renders text into an ImageSource using a TextBlock visual. Renders a TextBlock to treat it like an image so that these different cards can be treated the same way.
        /// Mostly generated using Copilot.
        /// </summary>
        private static ImageSource RenderTextToImage(string text, Brush foreground, Thickness padding)
        {   
            // Create a TextBlock as the visual to render.
            var renderBox = new System.Windows.Controls.TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Foreground = foreground
            };

            // This supposedly makes the text appear more "crisp".
            TextOptions.SetTextFormattingMode(renderBox, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(renderBox, TextRenderingMode.Auto);

            // Wrap the TextBlock in a Border to add internal padding if requested.
            FrameworkElement visual = renderBox;
            if (padding != default && (padding.Left != 0 || padding.Top != 0 || padding.Right != 0 || padding.Bottom != 0))
            {
                visual = new System.Windows.Controls.Border
                {
                    Background = Brushes.Transparent, // keep transparent
                    Padding = padding,
                    Child = renderBox
                };
            }

            // Measure/arrange to get desired size.
            visual.Measure(new Size(DefaultWidth, DefaultHeight));
            visual.Arrange(new Rect(0, 0, Math.Max(MinWidth,Math.Min(DefaultWidth, visual.DesiredSize.Width)), Math.Max(MinHeight,visual.DesiredSize.Height)));
            visual.UpdateLayout();

            var width = Math.Ceiling(visual.RenderSize.Width);
            var height = Math.Ceiling(visual.RenderSize.Height);
            if (width < 1) width = MinWidth;
            if (height < 1) height = MinHeight;

            // Implements DPI-aware rendering. You can query system DPI from
            // a PresentationSource if needed.
            // Using 96x96 keeps device independent pixels = pixels in RTB.
            const double dpiX = 96.0;
            const double dpiY = 96.0;

            var rtb = new RenderTargetBitmap((int)width, (int)height, dpiX, dpiY, PixelFormats.Pbgra32);

            // Render and return the Bitmap format image. This will be treated as an ImageSource.
            rtb.Render(visual);
            return rtb;
        }
    }
}