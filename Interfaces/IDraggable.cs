// Author:  Kyle Chapman
// Created: February 6, 2026
// Updated: February 6, 2026
// Description:
// This interface describes behaviours for an object that can be clicked and dragged around
// a container in WPF. Meant to be used with a WPF Canvas.

using System.Windows;

// Note the namespace.
namespace MovingCards.Interfaces
{
    /// <summary>
    /// Interface for an object that can be dragged around a container in WPF.
    /// </summary>
    public interface IDraggable
    {
        /// <summary>
        /// Top-left position on the drag surface (Canvas) in pixels.
        /// </summary>
        Point Position { get; set; }

        /// <summary>
        /// A width and height for the object to drag; not sure it's actually needed.
        /// </summary>
        Size Size { get; }

        /// <summary>
        /// Whether dragging can start based on mouse cursor coordinates. The main purpose of this is if you only wanted the object to be draggable based on clicking a certain part of that thing, or if you wanted to allow a bit of "spillover space" to still make that object draggable.
        /// </summary>
        /// <param name="localStart">An x + y coordinate.</param>
        /// <returns>whether dragging can take place</returns>
        bool CanDrag(Point localStart);

        /// <summary>
        /// A thing the class needs to call when drag starts.
        /// </summary>
        void BeginDrag(Point localStart, Point surfaceStart);

        /// <summary>
        /// During a drag, this uses the "delta" - how much the thing has moved since the dragging began.
        /// </summary>
        void DragBy(Vector surfaceDelta);

        /// <summary>
        /// Called when drag ends - generally, when the mouse button goes up.
        /// </summary>
        void EndDrag();
    }
}