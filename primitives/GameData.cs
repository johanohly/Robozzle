using Godot;
using System;

public partial class GameData : Node
{
    private ConfigFile _config = new ConfigFile();

    public int CurrentLevel { get; private set; } = 1;

    public override void _Ready()
    {
        var err = _config.Load("user://config.cfg");
        if (err != Error.Ok) return;

        var value = (string)_config.GetValue("Level", "Level");
        if (int.TryParse(value, out var level))
        {
            CurrentLevel = Math.Max(CurrentLevel, level);
        }
    }
    
    public void SetLevel(int level)
    {
        CurrentLevel = level;
        _config.SetValue("Level", "Level", level.ToString());
        _config.Save("user://config.cfg");
    }
}