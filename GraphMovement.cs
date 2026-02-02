using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

public partial class GraphMovement : Node2D
{
    private bool _isDragging = false;
    private bool panningInProgress;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SetProcessInput(true);
        // Option 2 (commented): Hide and capture mouse for free pan mode
        // Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // Option 2 (commented): Press Escape to toggle mouse capture mode
        // if (@event is InputEventKey keyEvent && keyEvent.Keycode == Key.Escape && keyEvent.Pressed && !keyEvent.Echo)
        // {
        // 	if (Input.MouseMode == Input.MouseModeEnum.Captured)
        // 	{
        // 		Input.MouseMode = Input.MouseModeEnum.Visible;
        // 	}
        // 	else
        // 	{
        // 		Input.MouseMode = Input.MouseModeEnum.Captured;
        // 	}
        // 	GetViewport().SetInputAsHandled();
        // 	return;
        // }

        // Handle mouse wheel for zooming and mouse move for panning
        if (@event is InputEventMouseButton mouseEvent)
        {
            // Option 1 (active): Left-click drag to pan
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                _isDragging = mouseEvent.Pressed;
            }
            else if (mouseEvent.Pressed)
            {
                switch (mouseEvent.ButtonIndex)
                {
                    case MouseButton.WheelUp:
                        if (Scale.X >= 3.0f)
                        {
                            break; // Prevent zooming in too far
                        }
                        ZoomAtPoint(1.1f, mouseEvent.Position); // Option 1 (active): Zoom at mouse position
                                                                // ZoomAtPoint(1.1f, GetViewportRect().GetCenter()); // Option 2 (commented): zoom at viewport center
                        break;
                    case MouseButton.WheelDown:
                        if (Scale.X <= 0.7f)
                        {
                            break; // Prevent zooming out too far
                        }
                        ZoomAtPoint(0.9f, mouseEvent.Position); // Option 1: Zoom at mouse position
                                                                // ZoomAtPoint(0.9f, GetViewportRect().GetCenter()); // Option 2: zoom at viewport center
                        break;
                }
                HandleZoomOutOfBounds();
            }
        }
        else if (@event is InputEventMouseMotion motionEvent)
        {
            // Option 1 (active): Pan only when dragging with left button
            if (_isDragging)
            {
                Position += motionEvent.Relative;
            }
            // Option 2 (commented): Pan with any mouse movement (no button needed, mouse captured)
            // Position += motionEvent.Relative;
        }
    }

    private void HandleZoomOutOfBounds()
    {
        // TODO: Check if zoom out lead to the viewport crossing a border of the graph
    }

    private void ZoomAtPoint(float zoomFactor, Vector2 point)
    {
        // Convert viewport point to local coordinates before scaling
        Vector2 localPoint = ToLocal(point);

        // Apply scale
        Scale *= zoomFactor;

        // Convert the same local point back to global coordinates after scaling
        Vector2 newGlobalPoint = ToGlobal(localPoint);

        // Adjust position to keep the point in the same place
        Position += point - newGlobalPoint;
    }

    public void CenterOnNode(GraphNode node, int zoomInMode = 0) // zoomInMode: 0 = normal, 1 = zoom out from inside, 2 = zoom in from outside
    {
        Viewport viewport = GetViewport();
        Vector2 viewportCenter = viewport.GetVisibleRect().GetCenter();

        if (zoomInMode == 1)
        {
            Scale = new Vector2(15f, 15f);
            Position = viewportCenter - node.Position * Scale.X;
        }
        float newScale = zoomInMode == 2 ? 15f : 2.2f;

        Tween tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(this, "scale", new Vector2(newScale, newScale), 0.8).SetTrans(Tween.TransitionType.Sine);
        tween.TweenProperty(this, "position", viewportCenter - node.Position * newScale, 0.8).SetTrans(Tween.TransitionType.Sine);
        tween.SetEase(Tween.EaseType.InOut);
        panningInProgress = false;
    }

    internal void ZoomOutToShowNodes(List<GraphNode> nodesToInclude, float multiplier = 1f) // multiplier, 1f means normal zoom out, 0f means no zoom out
    {
        Viewport viewport = GetViewport();
        Vector2 viewportCenter = viewport.GetVisibleRect().GetCenter();

        // Calculate bounding box of all nodes to show
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (GraphNode node in nodesToInclude)
        {
            Vector2 pos = node.Position;
            if (pos.X < minX) minX = pos.X;
            if (pos.X > maxX) maxX = pos.X;
            if (pos.Y < minY) minY = pos.Y;
            if (pos.Y > maxY) maxY = pos.Y;
        }

        // Determine required scale to fit all nodes in viewport
        float nodesWidth = maxX - minX;
        float nodesHeight = maxY - minY;
        float scaleX = viewport.GetVisibleRect().Size.X / (nodesWidth + 200); // Add some padding
        float scaleY = viewport.GetVisibleRect().Size.Y / (nodesHeight + 200); // Add some padding
        float newScale = Math.Min(scaleX, scaleY);


        newScale = Scale.X + (newScale - Scale.X) * multiplier;

        // dont zoom in, only out
        if (newScale > Scale.X)
        {
            newScale = Scale.X;
        }

        // Calculate new position to center the bounding box
        Vector2 nodesCenter = new((minX + maxX) / 2, (minY + maxY) / 2);
        Vector2 newPosition = viewportCenter - nodesCenter * newScale;

        // Animate to new scale and position
        Tween tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(this, "scale", new Vector2(newScale, newScale), 1.2).SetTrans(Tween.TransitionType.Sine);
        tween.TweenProperty(this, "position", newPosition, 1.2).SetTrans(Tween.TransitionType.Sine);
        tween.SetEase(Tween.EaseType.InOut);
        panningInProgress = false;
    }
}