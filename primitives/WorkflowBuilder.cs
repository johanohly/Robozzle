using Godot;
using System;
using Godot.Collections;
using Robozzle.primitives;

public partial class WorkflowBuilder : GraphEdit
{
	private Dictionary<Node, bool> _selectedNodes = new Dictionary<Node, bool>();

	public override void _Ready()
	{
		Parts.Instance.Hide();
		
		AddButton("Forward", () =>
		{
			var forwardNode = Parts.Instance.GetNode("Forward").Duplicate() as GraphNode;
			AddChild(forwardNode);
		});
		
		AddButton("Right", () =>
		{
			var rightNode = Parts.Instance.GetNode("Right").Duplicate() as GraphNode;
			AddChild(rightNode);
		});
		
		AddButton("Left", () =>
		{
			var leftNode = Parts.Instance.GetNode("Left").Duplicate() as GraphNode;
			AddChild(leftNode);
		});
		
		AddButton("Is green", () =>
		{
			var conditionNode = Parts.Instance.GetNode("Green").Duplicate() as GraphNode;
			AddChild(conditionNode);
		});
		
		AddButton("Evaluate", () =>
		{
			var level = GetParent().GetParent<Level>();
			Visible = false;
			level.Start();
			level.GetNode<Player>("Player/Player").ParseGraph();
		});
		
		AddButton("To level", () =>
		{
			GetParent().GetParent<Level>().ToLevel();
		});

		var startNode = Parts.Instance.GetNode("Start").Duplicate() as GraphNode;
		AddChild(startNode);

		var previousNode = startNode;
		for (var i = 0; i < 4; i++)
		{
			var forwardNode = Parts.Instance.GetNode("Forward").Duplicate() as GraphNode;
			AddChild(forwardNode);
			ConnectNode(previousNode.Name, 0, forwardNode.Name, 0);
			previousNode = forwardNode;
		}

		this.NodeSelected += OnNodeSelected;
		this.NodeDeselected += OnNodeDeselected;
		this.DeleteNodesRequest += OnDeleteNodesRequest;

		this.ConnectionRequest += OnConnectionRequest;
		this.DisconnectionRequest += OnDisconnectRequest;
	}

	private void AddButton(string title, Action action)
	{
		var button = new Button();
		button.Text = title;
		button.Pressed += action;
		GetZoomHBox().AddChild(button);
	}

	private void OnNodeSelected(Node node)
	{
		var graphNode = node as GraphNode;
		if (graphNode.Title == "Start")
		{
			return;
		}

		_selectedNodes.Add(node, true);
	}

	private void OnNodeDeselected(Node node)
	{
		_selectedNodes.Remove(node);
	}

	private void _RemoveConnectionsToNode(Node node)
	{
		foreach (var con in this.GetConnectionList())
		{
			if ((string)con["from_node"] == node.Name || (string)con["to_node"] == node.Name)
			{
				this.DisconnectNode((string)con["from_node"], (int)con["from_port"], (string)con["to_node"],
					(int)con["to_port"]);
			}
		}
	}

	private void OnDeleteNodesRequest(Godot.Collections.Array array)
	{
		foreach (var node in _selectedNodes.Keys)
		{
			if (_selectedNodes[node])
			{
				_RemoveConnectionsToNode(node);
				node.QueueFree();
			}
		}

		_selectedNodes.Clear();
	}

	private void OnConnectionRequest(StringName fromNode, long fromPort, StringName toNode, long toPort)
	{
		this.ConnectNode(fromNode, (int)fromPort, toNode, (int)toPort);
	}

	private void OnDisconnectRequest(StringName fromNode, long fromPort, StringName toNode, long toPort)
	{
		this.DisconnectNode(fromNode, (int)fromPort, toNode, (int)toPort);
	}
}
