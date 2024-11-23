using Godot;

namespace Robozzle.primitives;

public partial class Level : Node
{
    public override void _Ready()
    {
        GetNode<TileMapLayer>("Background").Visible = false;
        GetNode<TileMapLayer>("Foreground").Visible = false;
        GetNode("Player").GetNode<Player>("Player").Visible = false;
    }

    public void Start()
    {
        GetNode<TileMapLayer>("Background").Visible = true;
        GetNode<TileMapLayer>("Foreground").Visible = true;
        GetNode("Player").GetNode<Player>("Player").Visible = true;
    }
}