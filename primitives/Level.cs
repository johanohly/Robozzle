using System;
using Godot;

namespace Robozzle.primitives;

public partial class Level : Node
{
	private Player _player = null;
	private WorkflowBuilder _workflowBuilder = null;
	private Vector2 _startPosition = new Vector2();

	public TileMapLayer ForegroundTileMap = null;
	public TileMapLayer BackgroundTileMap = null;
	public int Stars { get; private set; } = 0;

	public static readonly Vector2 StartAtlasPosition = new Vector2(16, 3);
	public static readonly Vector2 StarAtlasPosition = new Vector2(14, 1);
	public static readonly Vector2 OutOfBoundsAtlasPosition = new Vector2(0, 0);

	public override void _Ready()
	{
		_player = GetNode("Player").GetNode<Player>("Player");
		_workflowBuilder = GetNode<Node>("WorkflowBuilder").GetNode<WorkflowBuilder>("GraphEdit");

		ForegroundTileMap = GetNode<TileMapLayer>("Foreground");
		BackgroundTileMap = GetNode<TileMapLayer>("Background");

		var tiles = ForegroundTileMap.GetUsedCells();
		foreach (var tile in tiles)
		{
			if (ForegroundTileMap.GetCellAtlasCoords(tile) != StartAtlasPosition) continue;
			_startPosition = tile;
			break;
		}

		foreach (var tile in tiles)
		{
			if (ForegroundTileMap.GetCellAtlasCoords(tile) != StarAtlasPosition) continue;
			Stars++;
		}

		ForegroundTileMap.Visible = false;
		BackgroundTileMap.Visible = false;
		_player.Visible = false;
	}

	public void Start()
	{
		_player.Rotation = Mathf.Pi / 2;
		_player.Position = new Vector2(_startPosition.X * 64 + 32, _startPosition.Y * 64 + 32);
		_player.Visible = true;

		ForegroundTileMap.Visible = true;
		BackgroundTileMap.Visible = true;
	}

	public void Reset()
	{
		_player.Visible = false;
		ForegroundTileMap.Visible = false;
		BackgroundTileMap.Visible = false;

		_workflowBuilder.Visible = true;
	}
}
