using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class GraphNode : Sprite2D
{

	[Export] int id;
	static readonly int[][] connections=[
		[1,10],
		[2],
		[3,8],
		[4,8,10],
		[5,6,7], // Operator
		[10],
		[8],
		[8,10],
		[9],
		[10],
		[]
		];
	List<GraphNode> myConnections=[];
	public List<GraphNode> neededForUnlock = [];

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		id = this.GetIndex();
		// Iterate through indices of connected nodes
		foreach(int i in connections[id])
		{
			// Get connected node
			GraphNode connected = this.GetParent().GetChild<GraphNode>(i);
			myConnections.Add(connected);
			connected.neededForUnlock.Add(this);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	public override void _EnterTree()
	{
		Debug.WriteLine("GraphNode ", id, " entered tree.");
	}

	public override void _Draw()
	{
		foreach(GraphNode connected in myConnections)
		{
    		// DrawLine(from, to, color, width, antialiasing)
    		DrawLine(Vector2.Zero, connected.Position-this.Position, Colors.Cyan, 2.0f);
		}

	}
}
