using System;
using System.Collections.Generic;
using Godot;
using System.Threading.Tasks;
using Robozzle.primitives;
using Robozzle.Primitives;

public partial class Player : Sprite2D
{
	private const float TileSize = 64f;
	private const float MoveSpeed = 1f;
	private const float RotationSpeed = .5f;
	private Level _level = null;

	public override void _Ready()
	{
		Position = new Vector2(32, 32);
		_level = GetParent<Node>().GetParent<Level>();
	}

	public void ParseGraph()
	{
		var graph = GetParent<Node>().GetParent<Node>().GetNode<Node>("WorkflowBuilder")
			.GetNode<GraphEdit>("GraphEdit");
		var result = new MoveSequenceParser().ParseGraph(graph);
		ExecuteTree(result);
	}

	private async Task ExecuteTree(NodeTree nodeTree)
	{
		var currentNode = nodeTree;

		while (currentNode != null)
		{
			switch (currentNode.NodeType)
			{
				case NodeType.Action:
					await ExecuteAction(currentNode.Action);
					currentNode = currentNode.NextNode;
					break;

				case NodeType.Condition:
					var conditionMet = EvaluateCondition(currentNode.Condition);
					currentNode = conditionMet ? currentNode.YesBranch : currentNode.NoBranch;
					break;
			}

			var cancel = _level.CheckTile();
			if (cancel)
			{
				return;
			}

			await ToSignal(GetTree().CreateTimer(MoveSpeed / 2), "timeout");
		}
	}

	private bool EvaluateCondition(string condition)
	{
		switch (condition)
		{
			case "green":
				return _level.IsGreenTile();
		}

		return false;
	}

	private async Task ExecuteAction(string action)
	{
		switch (action)
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
