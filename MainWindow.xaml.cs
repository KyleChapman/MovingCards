// Author:  Kyle Chapman
// Created: February 6, 2026
// Updated: February 17, 2026
// Description:
// This interface describes behaviours for an object that can be clicked and dragged around
// a container in WPF. Meant to be used with a WPF Canvas.

using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MovingCards
{

    public partial class MainWindow : Window
    {
        // Declaring, but not instantiating the DragManager - this is so it can be addressed in other methods.
        private DragManager? _drag;
        List<DraggableRectangle> draggableElements = new List<DraggableRectangle>();

        /// <summary>
        /// Constructor for the window. Adds event handlers for Loaded/Unloaded just to keep the UI a little more separated from functionality.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        /// <summary>
        /// When the form is fully loaded, initialize the DragManager.
        /// It also creates default DraggableRectangles.
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // This is the instantiation of the DragManager object.
            _drag = new DragManager(Surface);

            // Default cards!
            // If we were taking this seriously, these would be defined in a JSON
            // or something - or at least its own module - instead of a bunch of
            // verbatim definitions.
            var textCard1 = new DraggableRectangle("Discuss progress relative to intended milestones.", Brushes.LightSalmon, new Point(40, 50));
            var textCard2 = new DraggableRectangle("Get the work done.", Brushes.PaleGoldenrod, new Point(260, 120));
            var textCard3 = new DraggableRectangle("Gather student feedback", Brushes.LightSeaGreen, new Point(120, 260));
            var imageCard1 = new DraggableRectangle(new BitmapImage(new Uri("pack://application:,,,/Assets/Zebra.jpg")), Brushes.LightSkyBlue, new Point(300, 120));
            var textCard4 = new DraggableRectangle("This is the fifth card.");
            var imageCard2 = new DraggableRectangle(new BitmapImage(new Uri("pack://application:,,,/Assets/ExpoTube.jpg")), Brushes.Maroon, new Point(500, 20));

            // Add the DraggableRectangles into the map stored in the DragManager.
            AddDraggable(textCard1);
            AddDraggable(textCard2);
            AddDraggable(imageCard1);
            AddDraggable(imageCard2);
            AddDraggable(textCard3);
            AddDraggable(textCard4);
        }

        /// <summary>
        /// Dispose of the DragManager object when the window is closed.
        /// </summary>
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // Intentional Dispose for the drag manager object.
            _drag?.Dispose();
            _drag = null;
        }

        /// <summary>
        /// Add DraggableRectangles into the map stored in the DragManager.
        /// </summary>
        /// <param name="rectangle">A draggable rectangle you'd like to be displayed and dragged</param>
        private void AddDraggable(DraggableRectangle rectangle)
        {
            // This part actually draws the rectangle.
            var element = CreateRectangleElement(rectangle);

            Canvas.SetLeft(element, rectangle.Position.X);
            Canvas.SetTop(element, rectangle.Position.Y);

            Surface.Children.Add(element);
            // Add an element into the list of IDraggable objects using the DragManager object.
            _drag!.Register(element, rectangle);

            draggableElements.Add(rectangle);
        }

        /// <summary>
        /// Creates the rectangle as a WPF-displayable UIElement.
        /// </summary>
        /// <param name="rectangleObject">An instantiated DraggableRectangle to display</param>
        /// <returns>A displayable UIElement</returns>
        private UIElement CreateRectangleElement(DraggableRectangle rectangleObject)
        {
            // Generate the Border for the UIElement.
            var border = new Border
            {
                Width = rectangleObject.Size.Width,
                Padding = new Thickness(12, 8, 12, 12),
                CornerRadius = new CornerRadius(10),
                Background = rectangleObject.Background,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 18,
                    ShadowDepth = 2,
                    Opacity = 0.35,
                    Color = Colors.Black
                }
            };

            // Use a Grid to add different elements to the rectangle.
            // This Grid could theoretically also be used to make cards with a
            // header, image, and text block in rows... (e.g. Pokemon, MTG)
            // or a neat playing card with a rank text and suit symbols.
            var grid = new Grid();

            // The image to be rendered onto the card, which will be neatly centered.
            var img = new Image
            {
                Source = rectangleObject.Content,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                SnapsToDevicePixels = true
            };
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);

            // Create the container for the elements of the card.
            var contentContainer = new Grid
            {
                // This is the actual thickness of margin for objects in the card.
                Margin = new Thickness(2, 2, 2, 2)
            };
            contentContainer.Children.Add(img);
            Grid.SetRow(contentContainer, 0);
            grid.Children.Add(contentContainer);
            border.Child = grid;

            return border;
        }

        /// <summary>
        /// Attempts to sort the cards into neat rows.
        /// </summary>
        private async void SortClick(object sender, RoutedEventArgs e)
        {
            // This iterates through cards, keeping track of the X/Y position of each card, but it starts at 5,5.
            var currentX = 5;
            var currentY = 5;
            // This is the minimum space between sorted cards.
            var sortPadding = 5;

            // This creates a list of "destinations" - slots that the cards will be relocated to.
            var destinations = new List<Point>();
            foreach (DraggableRectangle element in draggableElements)
            {
                // Set location to the current X,Y coordinates.
                destinations.Add(new Point(currentX, currentY));

                // Increment X. If X goes off the screen, instead set X back to 5 (yucky hardcode) and increment Y.
                currentX = currentX + (int)element.Size.Width + sortPadding;
                if (currentX > Surface.ActualWidth - (DraggableRectangle.DefaultWidth + sortPadding))
                {
                    currentX = 5;
                    currentY = currentY + DraggableRectangle.DefaultHeight + sortPadding;
                }
            }

            // Go through the list of cards, and set each card's coordinates to its matching destination.
            var isDone = false;
            // This loop continues until there are no more DraggableRectangles left to be sorted.
            while (!isDone)
            {
                isDone = true;

                // This loop handles "animation"!
                // Basically for each card, we gradually change its X and Y coordinates until it reaches its destination.
                for (int i = 0; i < destinations.Count; i++)
                {
                    var currentCard = draggableElements[i];
                    
                    // This is the final destination X,Y of the DraggableRectangle.
                    var dest = destinations[i];
                    // This is the current X,Y of the DraggableRectangle.
                    var renderedX = currentCard.Position.X;
                    var renderedY = currentCard.Position.Y;
                    // This is the new, iterative X,Y of the DraggableRectangle.
                    var newX = renderedX;
                    var newY = renderedY;

                    // If it's too far to the right, move it left. Or vice-versa.
                    if (renderedX > dest.X) newX = renderedX - 1;
                    else if (renderedX < dest.X) newX = renderedX + 1;

                    // If it's too far to the up, move it down. Or vice-versa.
                    if (renderedY > dest.Y) newY = renderedY - 1;
                    else if (renderedY < dest.Y) newY = renderedY + 1;

                    // If it's within 3 pixels, snap it. This prevents jitters and overshoots.
                    if (Math.Abs(renderedX - dest.X) <= 3) renderedX = dest.X;
                    if (Math.Abs(renderedY - dest.Y) <= 3) renderedY = dest.Y;

                    // If it's not at the destination, keep looping and render the card.
                    if (renderedX != dest.X || renderedY != dest.Y)
                    {
                        isDone = false;
                        currentCard.Position = new Point(newX, newY);
                        _drag?.SyncPosition(currentCard);
                    }
                }

                // Wait 1 millisecond before looping for the sake of animation.
                // Or... try messing with this value. It's fun.
                await Task.Delay(5);
            }
        }

    }
}
