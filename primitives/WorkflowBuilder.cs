using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Robozzle.primitives;

public partial class WorkflowBuilder : GraphEdit
{
    private Dictionary<Node, bool> _selectedNodes = new();
    private Dictionary<string, int> _usedBlocks = new();

    private Level _level = null;

    public override void _Ready()
    {
        _level = GetParent<Node>().GetParent<Level>();
        Parts.Instance.Hide();

        this.NodeSelected += OnNodeSelected;
        this.NodeDeselected += OnNodeDeselected;
        this.DeleteNodesRequest += OnDeleteNodesRequest;

        this.ConnectionRequest += OnConnectionRequest;
        this.DisconnectionRequest += OnDisconnectRequest;
    }

    public void InitControls()
    {
        _level.AllowedBlocks.Keys.ToList().ForEach(key => _usedBlocks[key] = 0);

        AddLimitedButton("forward", "Forward");
        AddLimitedButton("left", "Left");
        AddLimitedButton("right", "Right");
        AddLimitedButton("green", "Is green");

        AddButton("Evaluate", () =>
        {
            var level = GetParent().GetParent<Level>();
            Visible = false;
            level.Start();
            level.GetNode<Player>("Player/Player").ParseGraph();
        });

        AddButton("To level", () => { GetParent().GetParent<Level>().ToLevel(); });

        var startNode = Parts.Instance.GetNode("Start").Duplicate() as GraphNode;
        AddChild(startNode);
    }

    private void AddLimitedButton(string key, string name)
    {
        if (!_level.AllowedBlocks.ContainsKey(key))
        {
            return;
        }

        var button = new Button();
        button.Name = key;
        button.Text = $"{name} ({_usedBlocks[key]}/{_level.AllowedBlocks[key]})";
        button.Pressed += () => AddBlock(key, name);
        GetZoomHBox().AddChild(button);
    }

    private void AddButton(string title, Action action)
    {
        var button = new Button();
        button.Text = title;
        button.Pressed += action;
        GetZoomHBox().AddChild(button);
    }

    private void AddBlock(string key, string name)
    {
        if (_usedBlocks[key] >= _level.AllowedBlocks[key])
        {
            return;
        }

        var node = Parts.Instance.GetNode(name).Duplicate() as GraphNode;
        AddChild(node);
        _usedBlocks[key]++;
        GetZoomHBox().GetNode<Button>(key).Text = $"{name} ({_usedBlocks[key]}/{_level.AllowedBlocks[key]})";
    }

    private void RemoveBlock(Node node)
    {
        var action = (string)node.Get("metadata/action");
        var condition = (string)node.Get("metadata/condition");
        var key = action != "" ? action : condition;
        if (key == "") return;

        if (!_usedBlocks.ContainsKey(key)) return;
        _usedBlocks[key]--;
        var btn = GetZoomHBox().GetNode<Button>(key);
        btn.Text = Regex.Replace(btn.Text, @"\(.*\)", $"({_usedBlocks[key]}/{_level.AllowedBlocks[key]})");
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
        foreach (var con in GetConnectionList())
        {
            if ((string)con["from_node"] == node.Name || (string)con["to_node"] == node.Name)
            {
                DisconnectNode((string)con["from_node"], (int)con["from_port"], (string)con["to_node"],
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
                RemoveBlock(node);
                node.QueueFree();
            }
        }

        _selectedNodes.Clear();
    }

    private void OnConnectionRequest(StringName fromNode, long fromPort, StringName toNode, long toPort)
    {
        ConnectNode(fromNode, (int)fromPort, toNode, (int)toPort);
    }

    private void OnDisconnectRequest(StringName fromNode, long fromPort, StringName toNode, long toPort)
    {
        DisconnectNode(fromNode, (int)fromPort, toNode, (int)toPort);
    }
}