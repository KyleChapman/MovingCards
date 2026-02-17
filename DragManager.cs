// Author:  Kyle Chapman
// Created: February 6, 2026
// Updated: February 17, 2026
// Description:
// Handles the dragging of objects that implement the IDraggable interface.
// Note that several specific methods were generated using Copilot, and those are noted in block comments below.

// Note the format of this using statement for accessing our new Interface(s).
using MovingCards.Exceptions;
using MovingCards.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MovingCards
{
    /// <summary>
    /// Handles mouse capture & movement on a surface objects that implement IDraggable.
    /// </summary>
    public sealed class DragManager : IDisposable
    {
        // Teaching note: this is sealed. Why?
        // Underscore notation for fully private elements in the class.
        private readonly UIElement _surface;
        // When elements are added to the DragManager, add them to this Dictionary.
        private readonly Dictionary<UIElement, IDraggable> _map = new();

        private bool _isDragging;
        private Point _lastSurfacePoint;
        private UIElement? _activeElement;
        private IDraggable? _activeModel;
        private int _zCounter = 1;

        /// <summary>
        /// Constructor for the DragManager. This can't exist until the UI exists, so it's called in the form's OnLoaded event handler.
        /// </summary>
        /// <param name="surface">The surface for objects to be dragged on; intended as a Canvas object.</param>
        public DragManager(UIElement surface)
        {
            _surface = surface;
            // Add new event handlers to the surface.
            _surface.PreviewMouseLeftButtonDown += OnMouseDown;
            _surface.PreviewMouseMove += OnMouseMove;
            _surface.PreviewMouseLeftButtonUp += OnMouseUp;
            _surface.LostMouseCapture += OnLostCapture;
        }

        /// <summary>
        /// Registers an object as draggable, mapping a UI element to a draggable object.
        /// </summary>
        /// <param name="element">The UI element to act as the key</param>
        /// <param name="model">The draggable object as the value</param>
        public void Register(UIElement element, IDraggable model)
        {
            _map[element] = model;
        }

        /// <summary>
        /// Removes a draggable object from the map.
        /// </summary>
        /// <param name="element">UI element to remove</param>
        public void Unregister(UIElement element)
        {
            _map.Remove(element);
        }

        /// <summary>
        /// When the mouse goes down, start dragging the object.
        /// </summary>
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            // If we're already dragging the object, ignore the rest.
            if (_isDragging) return;

            // Using the DragManager, access the registered, clicked object. Immediately exit if this fails.
            var (element, model) = FindRegisteredFromVisual(e.OriginalSource as DependencyObject);
            if (element == null || model == null) return;

            // Set the start location relative to the app and the canvas based on the mouse click location.
            var localStart = e.GetPosition(element);
            var surfaceStart = e.GetPosition(_surface);
            
            // If the start location is unusable, exit.
            if (!model.CanDrag(localStart)) return;

            // Begin the drag!
            _activeElement = element;
            _activeModel = model;
            _lastSurfacePoint = surfaceStart;
            model.BeginDrag(localStart, surfaceStart);

            // This line is basically a final check, since we already know we're dragging from the first line. This could technically matter if mouse were on a new surface didn't support dragging.
            _isDragging = _surface.CaptureMouse();
            // We are now dragging the object. It moves to the front.
            if (_isDragging)
            {
                BringToFront(element);
                e.Handled = true;
            }
            else
            {
                _activeElement = null;
                _activeModel = null;
            }
        }

        /// <summary>
        /// When the mouse moves, if we're dragging, reposition the dragged object.
        /// </summary>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            // If we're not dragging, get out of here.
            if (!_isDragging || _activeModel == null || _activeElement == null) return;

            // Calculate the change in position.
            var now = e.GetPosition(_surface);
            var delta = now - _lastSurfacePoint;
            if (delta.X == 0 && delta.Y == 0) return;
            // Drag the object.
            _activeModel.DragBy(delta);
            _lastSurfacePoint = now;

            // Reflect the object's new position on the UI element.
            Canvas.SetLeft(_activeElement, _activeModel.Position.X);
            Canvas.SetTop(_activeElement, _activeModel.Position.Y);
        }

        /// <summary>
        /// When the mouse button goes down, stop dragging.
        /// </summary>
        private void OnMouseUp(object sender, MouseButtonEventArgs e) => EndDrag();
        /// <summary>
        /// If we lose capture/focus, stop dragging.
        /// </summary>
        private void OnLostCapture(object sender, MouseEventArgs e) => EndDrag();

        /// <summary>
        /// Ends dragging if the cursor focus is lost or the button goes up.
        /// </summary>
        private void EndDrag()
        {
            try
            {
                // This just calls EndDrag from the IDraggable interface.
                if (_activeModel != null)
                    _activeModel.EndDrag();

                _activeElement = null;
                _activeModel = null;

                // End the drag.
                if (_isDragging)
                {
                    _surface.ReleaseMouseCapture();
                    _isDragging = false;
                }
            }
            catch (InvalidMoveException ex)
            {
                if (_activeModel != null)
                {
                    MessageBox.Show("Invalid move. " + ex.Message +
                    "\nX: " + _activeModel.Position.X +
                    "\nY: " + _activeModel.Position.Y);

                    _isDragging = false; 
                    _activeModel.Position = _lastSurfacePoint;
                }
            }


        }

        /// <summary>
        /// Any dragged object is moved to the front of the visual tree.
        /// This method was largely informed by Copilot.
        /// </summary>
        /// <param name="element">The UI element being moved.</param>
        private void BringToFront(UIElement element)
        {
            if (VisualTreeHelper.GetParent(element) is Panel panel)
                Panel.SetZIndex(element, ++_zCounter);
        }

        /// <summary>
        /// Accessed when the mouse button goes down, this retrieves a clicked object.
        /// This method was largely informed by Copilot.
        /// </summary>
        /// <param name="start">A starting position</param>
        /// <returns>A nullable, draggable UI element</returns>
        private (UIElement? element, IDraggable? model) FindRegisteredFromVisual(DependencyObject? start)
        {
            DependencyObject? current = start;
            while (current != null)
            {
                if (current is UIElement el && _map.TryGetValue(el, out var model))
                    return (el, model);
                current = VisualTreeHelper.GetParent(current);
            }
            return (null, null);
        }

        /// <summary>
        /// Removes unneeded event handlers when the DragManager is no longer needed.
        /// </summary>
        public void Dispose()
        {
            _surface.PreviewMouseLeftButtonDown -= OnMouseDown;
            _surface.PreviewMouseMove -= OnMouseMove;
            _surface.PreviewMouseLeftButtonUp -= OnMouseUp;
            _surface.LostMouseCapture -= OnLostCapture;
        }

        /// <summary>
        /// // Makes sure the objects in the local map have the same position on the drawing surface.
        /// This method was suggested by and written by Copilot, other than the comments.
        /// </summary>
        /// <param name="element">A draggable UI element</param>
        public void SyncPosition(IDraggable element)
        {
            foreach (var kvp in _map)
            {
                if (ReferenceEquals(kvp.Value, element))
                {
                    Canvas.SetLeft(kvp.Key, element.Position.X);
                    Canvas.SetTop(kvp.Key, element.Position.Y);
                    // Oooh, you know Kyle hates this.
                    // But there's a reason for it, right? What is the reason?
                    // Can it work without this?
                    break;
                }
            }
        }

    }
}
