using System.Collections.Generic;
using Godot;
using System.Threading.Tasks;
using Robozzle.Primitives;

public partial class Player : Sprite2D
{
    private const float TileSize = 64f;
    private const float MoveSpeed = 1f;
    private const float RotationSpeed = .5f;

    public override void _Ready()
    {
        Position = new Vector2(32, 32);
    }

    public void ParseGraph()
    {
        var graph = GetParent<Node>().GetParent<Node>().GetNode<Node>("WorkflowBuilder")
            .GetNode<GraphEdit>("GraphEdit");
        var result = new MoveSequenceParser().ParseGraph(graph);
        ExecuteTree(result);
    }

    private static void ExecuteTree(NodeTree nodeTree, HashSet<string> visited = null)
    {
        if (nodeTree == null || (visited != null && visited.Contains(nodeTree.Name)))
            return;

        visited ??= new HashSet<string>();
        visited.Add(nodeTree.Name);

        switch (nodeTree.NodeType)
        {
            case NodeType.Action:
                GD.Print($"Performing action: {nodeTree.Action}");
                break;

            case NodeType.Condition:
                var conditionResult = EvaluateCondition(nodeTree.Condition);
                GD.Print($"Condition '{nodeTree.Condition}' evaluated to: {conditionResult}");
                ExecuteTree(conditionResult ? nodeTree.YesBranch : nodeTree.NoBranch, visited);
                break;
        }

        visited.Remove(nodeTree.Name);
    }

    private static bool EvaluateCondition(string condition)
    {
        return condition == "is tile green";
    }


    public async Task ExecuteSequence(string[] moves)
    {
        foreach (var move in moves)
        {
            switch (move)
            {
                case "forward":
                    await MoveForward();
                    break;
                case "right":
                    await Rotate(Rotation + Mathf.Pi / 2);
                    break;
                case "left":
                    await Rotate(Rotation - Mathf.Pi / 2);
                    break;
            }

            await ToSignal(GetTree().CreateTimer(MoveSpeed / 2), "timeout");
        }
    }

    private async Task MoveForward()
    {
        var startPosition = Position;
        var direction = new Vector2(Mathf.Cos(Rotation - Mathf.Pi / 2), Mathf.Sin(Rotation - Mathf.Pi / 2));
        var targetPosition = startPosition + direction * TileSize;

        var elapsedTime = 0f;

        while (elapsedTime < MoveSpeed)
        {
            elapsedTime += (float)GetProcessDeltaTime();
            Position = startPosition.Lerp(targetPosition, elapsedTime / MoveSpeed);
            await ToSignal(GetTree(), "process_frame");
        }

        Position = targetPosition;
    }

    private new async Task Rotate(float targetRotation)
    {
        var startRotation = Rotation;

        var elapsedTime = 0f;

        while (elapsedTime < RotationSpeed)
        {
            elapsedTime += (float)GetProcessDeltaTime();
            Rotation = Mathf.Lerp(startRotation, targetRotation, elapsedTime / RotationSpeed);
            await ToSignal(GetTree(), "process_frame");
        }

        Rotation = targetRotation;
    }
}