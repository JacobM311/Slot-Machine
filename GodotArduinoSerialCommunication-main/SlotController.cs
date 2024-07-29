using Godot;
using System;
using System.IO.Ports;

public partial class SlotController : Node2D
{

	SerialPort serialPort;
	[Export] RichTextLabel text;
	bool hasHeardFromArduino = false;
	float timer;

	[Signal] public delegate void SerialDataReceiverEventHandler(string data);


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		serialPort = new SerialPort();
		serialPort.DtrEnable = true;
		serialPort.RtsEnable = true;
		serialPort.PortName = "COM4";
		serialPort.BaudRate = 9600; //make sure this is the same in Arduino as it is in Godot.
		serialPort.Open();
		serialPort.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
		SerialDataReceiver += Recieved;
	}

	public override void _Process(double delta)
	{

	}

	private async void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
	{
		// Show all the incoming data in the port's buffer in the output window
		String data = (String)serialPort.ReadExisting();
		await ToSignal(GetTree().CreateTimer(.1), "timeout");

		GD.Print("data : " + (String)serialPort.ReadExisting());
		EmitSignal("SerialDataReceiver", data);
	}

	void Recieved(string data)
	{
		text.Text = data;
	}
}
