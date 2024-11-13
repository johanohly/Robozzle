using Godot;
using System;

public partial class Parts : VBoxContainer
{
	public static Parts Instance { get; private set; }

	public override void _Ready()
	{
		Instance = this;
	}
}
