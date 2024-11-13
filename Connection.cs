using Godot;
using System;
using Godot.Collections;

public partial class Connection : GraphEdit
{
	private Dictionary<Node, bool> _selectedNodes = new Dictionary<Node, bool>();

	public override void _Ready()
	{
		Parts.Instance.Hide();
		
		var button = new Button();
		button.Text = "Add Node";
		button.Pressed += () =>
		{
			var node = Parts.Instance.GetNode("Forward").Duplicate() as GraphNode;
			AddChild(node);
		};
		this.GetZoomHBox().AddChild(button);

		var startNode = Parts.Instance.GetNode("Start").Duplicate() as GraphNode;
		AddChild(startNode);

		var forwardNode = Parts.Instance.GetNode("Forward").Duplicate() as GraphNode;
		AddChild(forwardNode);

		this.NodeSelected += OnNodeSelected;
		this.NodeDeselected += OnNodeDeselected;
		this.DeleteNodesRequest += OnDeleteNodesRequest;

		this.ConnectionRequest += OnConnectionRequest;
		this.DisconnectionRequest += OnDisconnectRequest;
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
