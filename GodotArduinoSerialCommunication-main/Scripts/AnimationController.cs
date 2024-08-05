using Godot;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]

public partial class AnimationController : AnimationPlayer
{
	[Export] SlotController.SlotController slotController;
	[Export] CpuParticles2D cpuParticles2D;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		slotController.SerialDataReceiver += DataReceived;
		Play("TitleCardFade");
	}

	void DataReceived(string data)
	{
		List<string> values = data.Split(",").ToList();
		bool allSame = values.All(str => str == values[0]);
		
		if (allSame)
		{
			Play("WinAimation");
		}
		else
		{
			GD.Print("loseanim");
			Play("LoseAnimation");
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
