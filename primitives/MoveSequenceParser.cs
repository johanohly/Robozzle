using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Robozzle.Primitives
{
    public class MoveSequenceParser
    {
        public Dictionary<string, GraphNode> Nodes = new Dictionary<string, GraphNode>();
        public Dictionary<string, List<string>> Connections = new Dictionary<string, List<string>>();

        public NodeTree ParseGraph(GraphEdit graphEdit)
        {
            foreach (var child in graphEdit.GetChildren())
            {
                if (child is not GraphNode node) continue;
                string nodeName = node.Name;
                Nodes[nodeName] = node;
                Connections[nodeName] = new List<string>();
            }

            foreach (var connection in graphEdit.GetConnectionList())
            {
                var from = (string)connection["from_node"];
                var to = (string)connection["to_node"];
                Connections[from].Add(to);
            }

            if (Nodes.Count <= 0) return null;
            var visited = new Dictionary<string, NodeTree>();
            var startNode = Connections["Start"].First();
            return BuildNodeTree(startNode, visited);
        }

        private NodeTree BuildNodeTree(string nodeName, Dictionary<string, NodeTree> visited)
        {
            if (visited.ContainsKey(nodeName))
            {
                return visited[nodeName];
            }

            if (!Nodes.TryGetValue(nodeName, out var node))
                return null;

            var nodeTree = new NodeTree { Name = nodeName };
            visited[nodeName] = nodeTree;

            var nodeType = (string)node.Get("metadata/type");
            switch (nodeType)
            {
                case "condition":
                    ParseConditionNode(node, nodeTree, visited);
                    break;
                case "action":
                    ParseActionNode(node, nodeTree);
                    if (Connections[nodeName].Count > 0)
                    {
                        var nextNodeName = Connections[nodeName].First();
                        nodeTree.NextNode = BuildNodeTree(nextNodeName, visited);
                    }

                    break;
                default:
                    GD.PrintErr($"Unknown node type: {nodeType}");
                    break;
            }

            return nodeTree;
        }

        private void ParseConditionNode(GraphNode node, NodeTree nodeTree, Dictionary<string, NodeTree> visited)
        {
            var condition = (string)node.Get("metadata/condition");
            nodeTree.NodeType = NodeType.Condition;
            nodeTree.Condition = condition;

            if (Connections[node.Name].Count < 2)
                throw new System.Exception($"Condition node '{node.Name}' must have two connections.");

            var yesNode = Connections[node.Name][0];
            var noNode = Connections[node.Name][1];

            nodeTree.YesBranch = BuildNodeTree(yesNode, visited);
            nodeTree.NoBranch = BuildNodeTree(noNode, visited);
        }

        private void ParseActionNode(GraphNode node, NodeTree nodeTree)
        {
            var action = (string)node.Get("metadata/action");
            nodeTree.NodeType = NodeType.Action;
            nodeTree.Action = action;
        }
    }

    public class NodeTree
    {
        public string Name { get; set; }
        public NodeType NodeType { get; set; }
        public string Condition { get; set; }
        public string Action { get; set; }
        public NodeTree YesBranch { get; set; }
        public NodeTree NoBranch { get; set; }
        public NodeTree NextNode { get; set; }
    }

    public enum NodeType
    {
        Action,
        Condition
    }
}