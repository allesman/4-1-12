using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

public partial class GraphNode : Sprite2D
{

	[Export] public int id;
	static readonly int[][] connections = [
		[1,10],
		[2],
		[3,8],
		[4,8,10],
		[5,6,7], // Operator
		[10],
		[8],
		[8,10],
		[9], // Reporter
		[], // Radio Host
		[] // Dog
		];
	public static bool[] unlocked = [
		true,  // 0
		false, // 1
		false, // 2
		false, // 3
		false, // 4
		false, // 5
		false, // 6
		false, // 7
		false, // 8
		false, // 9
		false  // 10
	];
	static int lastPlayedId = 0;
	public List<GraphNode> myConnections = [];
	public List<GraphNode> neededForUnlock = [];
	GraphMovement graphMovement;
	bool selected;
	Node2D lineDrawer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		graphMovement = this.GetParent<GraphMovement>();
		// VisibilityChanged += OnVisibilityChanged;
		id = this.GetIndex();
		// if unlocked, show me
		if (unlocked[id])
		{
			this.Show();
			// If I'm the last played node, center on me but like a cool zoom out thing
			if (id == lastPlayedId)
			{
				graphMovement.CenterOnNode(this, 1);
			}
		}
		else
		{
			this.Hide();
		}
		// Iterate through indices of connected nodes
		foreach (int i in connections[id])
		{
			// Get connected node
			GraphNode connected = this.GetParent().GetChild<GraphNode>(i);
			myConnections.Add(connected);
			if (!unlocked[id])
			{
				connected.neededForUnlock.Add(this);
			}
		}
		// Create line drawer child node
		lineDrawer = new Node2D();
		lineDrawer.Name = "LineDrawer";
		AddChild(lineDrawer);
		MoveChild(lineDrawer, 0);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// if (neededForUnlock.Count == 0 && !unlocked[id])
		// {
		// this.Show(); // TODO fade in instead of just show
		// unlocked[id] = true;
		// }

	}

	private async void OnComplete()
	{
		if (!this.IsVisibleInTree())
		{ return; }

		// change sprite to green color
		this.Modulate = Colors.LimeGreen;

		// Wait 1 second for the previous zoom out to finish
		// await Task.Delay(1000);

		List<GraphNode> unlockedByMe = new List<GraphNode>();
		// the name for this needs to be something like unlocked, but signify it wasnt completely unlocked but just a step towards it
		List<GraphNode> unlockedByMePartially = new List<GraphNode>();
		foreach (GraphNode connected in myConnections)
		{
			connected.neededForUnlock.Remove(this);
			if (connected.neededForUnlock.Count == 0)
			{
				connected.Show();
				unlocked[connected.id] = true;
				unlockedByMe.Add(connected);
			}
			else
			{
				unlockedByMePartially.Add(connected);
			}
		}
		// if unlocking, zoom out to show newly unlocked nodes, and me
		if (unlockedByMe.Count > 0)
		{
			unlockedByMe.Add(this);
			graphMovement.ZoomOutToShowNodes(unlockedByMe, 1f);
		}
		// if no unlocking, but partially unlocking, zoom out a bit to show progress
		else if (unlockedByMePartially.Count > 0)
		{
			unlockedByMePartially.Add(this);
			graphMovement.ZoomOutToShowNodes(unlockedByMePartially, 0.1f);
		}
		lineDrawer.QueueRedraw();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
			{
				if (GetRect().HasPoint(ToLocal(mouseEvent.Position)))
				{
					GetViewport().SetInputAsHandled();
					if (!selected)
					{
						graphMovement.CenterOnNode(this);
						selected = true;
					}
					else
					{
						// graphMovement.CenterOnNode(this, 2);
						// In Reality: enter level
						// In this demo: mark level as complete after quick delay
						// TODO delay
						// graphMovement.CenterOnNode(this, 1);
						OnComplete();
						selected = false;
					}
				}
			}
		}
	}
}
