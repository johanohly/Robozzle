using Godot;
using System;
using System.Linq;
using System.Text.RegularExpressions;

public partial class Main : ColorRect
{
    public override void _Ready()
    {
        foreach (var node in GetNode<VBoxContainer>("CenterContainer/VBoxContainer/VBoxContainer").GetChildren())
        {
            if (node is Button button)
            {
                var levelNum = Regex.Match(button.Name.ToString(), @"\d+").Value;
                if (ResourceLoader.Exists($"res://levels/level_{levelNum}.tscn"))
                {
                    button.Pressed += () =>
                    {
                        GetNode<SceneLoader>("/root/SceneLoader")
                            .GotoScene($"levels/level_{levelNum}");
                    };
                }
                else
                {
                    button.Disabled = true;
                }
            }
        }
    }
}