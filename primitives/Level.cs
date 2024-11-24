using System;
using System.Collections.Generic;
using Godot;

namespace Robozzle.primitives;

public partial class Level : Node
{
	private Player _player = null;
	private WorkflowBuilder _workflowBuilder = null;
	private Vector2 _startPosition = new Vector2();
	private List<Vector2I> _initialStarPositions = new List<Vector2I>();
	private Button _workflowBuilderButton = null;

	public TileMapLayer ForegroundTileMap = null;
	public TileMapLayer BackgroundTileMap = null;

	public int CollectedStars { get; private set; } = 0;
	public int Stars { get; private set; } = 0;

	public static readonly Vector2I StartAtlasPosition = new Vector2I(16, 3);
	public static readonly Vector2I StarAtlasPosition = new Vector2I(14, 1);
	public static readonly Vector2I OutOfBoundsAtlasPosition = new Vector2I(0, 0);

	public override void _Ready()
	{
		_player = GetNode<Player>("Player/Player");
		_workflowBuilder = GetNode<WorkflowBuilder>("WorkflowBuilder/GraphEdit");
		_workflowBuilder.Visible = false;

		var btn = new Button();
		btn.Name = "WorkflowBuilderButton";
		btn.Text = "Workflow Builder";
		btn.Pressed += ToWorkflowBuilder;
		AddChild(btn);
		_workflowBuilderButton = GetNode<Button>("WorkflowBuilderButton");

		ForegroundTileMap = GetNode<TileMapLayer>("Foreground");
		BackgroundTileMap = GetNode<TileMapLayer>("Background");

		var tiles = ForegroundTileMap.GetUsedCells();
		foreach (var tile in tiles)
		{
			if (ForegroundTileMap.GetCellAtlasCoords(tile) == StartAtlasPosition)
			{
				_startPosition = tile;
			}
			else if (ForegroundTileMap.GetCellAtlasCoords(tile) == StarAtlasPosition)
			{
				Stars++;
				_initialStarPositions.Add(tile);
			}
		}

		Start();
	}

	public void Start()
	{
		_player.Rotation = Mathf.Pi / 2;
		_player.Position = new Vector2(_startPosition.X * 64 + 32, _startPosition.Y * 64 + 32);
		_player.Visible = true;

		CollectedStars = 0;

		ForegroundTileMap.Visible = true;
		BackgroundTileMap.Visible = true;
	}

	public void Reset()
	{
		_player.Visible = false;
		ForegroundTileMap.Visible = false;
		BackgroundTileMap.Visible = false;

		_workflowBuilder.Visible = true;

		foreach (var position in _initialStarPositions)
		{
			ForegroundTileMap.SetCell(position, 0, StarAtlasPosition);
		}
	}
	
	public void ToWorkflowBuilder()
	{
		_player.Visible = false;
		ForegroundTileMap.Visible = false;
		BackgroundTileMap.Visible = false;
		
		_workflowBuilder.Visible = true;
		_workflowBuilderButton.Visible = false;
		GD.Print("ToWorkflowBuilder");
	}
	
	public void ToLevel()
	{
		_player.Visible = true;
		ForegroundTileMap.Visible = true;
		BackgroundTileMap.Visible = true;
		
		_workflowBuilder.Visible = false;
		_workflowBuilderButton.Visible = true;
	}

	public bool CheckTile()
	{
		var cancel = CheckStar();
		if (cancel)
		{
			Reset();
			return true;
		}

		var outOfBounds = CheckOB();
		if (outOfBounds)
		{
			Reset();
			return true;
		}

		return false;
	}

	private bool CheckStar()
	{
		var tilePosition = ForegroundTileMap.LocalToMap(_player.Position);
		var atlasPosition = ForegroundTileMap.GetCellAtlasCoords(tilePosition);
		if (atlasPosition != StarAtlasPosition) return false;

		ForegroundTileMap.SetCell(tilePosition, -1);

		CollectedStars++;
		if (CollectedStars == Stars)
		{
			Reset();
			return true;
		}

		return false;
	}

	private bool CheckOB()
	{
		var tilePosition = BackgroundTileMap.LocalToMap(_player.Position);
		var atlasPosition = BackgroundTileMap.GetCellAtlasCoords(tilePosition);
		if (atlasPosition != OutOfBoundsAtlasPosition) return false;

		return true;
	}
}
