using Godot;

public partial class LineDrawer : Node2D
{
    GraphNode parent;

    public override void _Ready()
    {
        parent = this.GetParent<GraphNode>();
    }

    public override void _Process(double delta)
    {
    }


    public override void _Draw()
    {
        foreach (GraphNode connected in parent.myConnections)
        {
            Vector2 targetPosition = connected.Position - parent.Position;
            Vector2 lineEnd = GraphNode.unlocked[connected.id] ? targetPosition : targetPosition * 0.5f;
            if (GraphNode.unlocked[connected.id])
            {
                DrawCircle(lineEnd, 10, Colors.LimeGreen);
            }
            DrawLine(Vector2.Zero, lineEnd, Colors.Cyan, 7, true);
        }
    }
}
