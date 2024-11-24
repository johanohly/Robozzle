using Godot;
using System;

public partial class SceneLoader : Node
{
	public Node CurrentScene { get; set; }

	public override void _Ready()
	{
		Viewport root = GetTree().Root;
		CurrentScene = root.GetChild(root.GetChildCount() - 1);
	}

	public void GotoScene(string path)
	{
		CallDeferred(MethodName.DeferredGotoScene, path);
	}

	private void DeferredGotoScene(string path)
	{
		CurrentScene.Free();

		var nextScene = GD.Load<PackedScene>($"res://{path}.tscn");

		CurrentScene = nextScene.Instantiate();

		GetTree().Root.AddChild(CurrentScene);

		GetTree().CurrentScene = CurrentScene;
	}
}
