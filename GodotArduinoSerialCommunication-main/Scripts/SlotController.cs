using Godot;
using System;
using System.IO.Ports;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace SlotController
{
public partial class SlotController : Node2D
{
	SerialPort serialPort;
	[Export] RichTextLabel winningsText;
	bool hasHeardFromArduino = false;
	float timer;
	private static ObjectId _documentId = ObjectId.Empty; // Initialize with an empty ObjectId
	private int winnings;

	[Signal] public delegate void SerialDataReceiverEventHandler(string data);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		winnings = 0;
		winningsText.Text = winnings.ToString();

		string portNameString = "";

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			portNameString = "COM4";
		}
		else
		{
			portNameString = "/dev/cu.usbmodem34B7DA66137C2";
		}

		serialPort = new SerialPort
		{
			DtrEnable = true,
			RtsEnable = true,
			PortName = portNameString,
			BaudRate = 9600 // make sure this is the same in Arduino as it is in Godot.
		};
		serialPort.Open();
		serialPort.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
		SerialDataReceiver += Recieved;

		// Load the _id from the config file
		var config = new ConfigFile();
		var filePath = "res://UserData/UserData.cfg";

		if (config.Load(filePath) == Error.Ok)
		{
			var documentIdString = config.GetValue("UserData", "document_id", "").ToString();
			if (ObjectId.TryParse(documentIdString, out var documentId))
			{
				_documentId = documentId;
			}
		}
	}

	public override void _Process(double delta)
	{
		
	}

	private async void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
	{
		// Show all the incoming data in the port's buffer in the output window
		String data = serialPort.ReadExisting();
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

		GD.Print("data : " + data);
		EmitSignal("SerialDataReceiver", data);
	}

	void Recieved(string data)
	{
		// pass data to MongoDB
		Task.Run(async () => await InsertOrUpdate(data));


		// execute game logic for UI stuff here
		List<string> values = data.Split(",").ToList();
		// checks if the reels are all matching
		bool allSame = values.All(str => str == values[0]);
		// if the reels all match do this switch case
		if (allSame)
		{
			switch (values[1])
			{
				case "1":
					winnings += 10;
					winningsText.Text = winnings.ToString();
					break;

				case "2":
					winnings += 20;
					winningsText.Text = winnings.ToString();
					break;

				case "3":
					winnings += 30;
					winningsText.Text = winnings.ToString();
					break;
				
				case "4":
					winnings += 40;
					winningsText.Text = winnings.ToString();
					break;

				case "5":
					winnings += 50;
					winningsText.Text = winnings.ToString();
					break;

				case "6":
					winnings += 60;
					winningsText.Text = winnings.ToString();
					break;

				case "7":
					winnings += 70;
					winningsText.Text = winnings.ToString();
					break;

				case "8":
					winnings += 80;
					winningsText.Text = winnings.ToString();
					break;

				case "9":
					winnings += 90;
					winningsText.Text = winnings.ToString();
					break;

				case "10":
					winnings += 100;
					winningsText.Text = winnings.ToString();
					break;

				case "11":
					winnings += 110;
					winningsText.Text = winnings.ToString();
					break;

				case "12":
					winnings += 120;
					winningsText.Text = winnings.ToString();
					break;
			}
		}
		else
		{
			winnings -= 5;
			winningsText.Text = winnings.ToString();
		}

		foreach(string str in values)
		{
			GD.Print(str);
		}
	}

	static async Task InsertOrUpdate(string data)
	{
		string connectionString = "mongodb+srv://jacobmarshall9608:h20uaJnIIg6dpnSF@cluster0.7osdg5a.mongodb.net/";
		string databaseName = "SlotMachine";
		string collectionName = "data";

		var client = new MongoClient(connectionString);
		var database = client.GetDatabase(databaseName);
		var collection = database.GetCollection<BsonDocument>(collectionName);

		if (_documentId == ObjectId.Empty)
		{
			// Create a new document with the initial data
			var document = new BsonDocument { { "user_data", data } };

			// Insert the document into the collection and get the generated _id
			await collection.InsertOneAsync(document);
			_documentId = document["_id"].AsObjectId;

			// Save _id to config file
			var config = new ConfigFile();
			var filePath = "res://UserData/UserData.cfg";
			config.SetValue("UserData", "document_id", _documentId.ToString());
			config.Save(filePath);

			GD.Print("doc added and _id saved to config: " + _documentId);
		}
		else
		{
			// create filter to find the correct doc with the _id
			var filter = Builders<BsonDocument>.Filter.Eq("_id", _documentId);

			// search the collection for the document with _id
			var currentDocument = await collection.Find(filter).FirstOrDefaultAsync();

			if (currentDocument != null)
			{
				// get the current user data as a string
				var existingData = currentDocument["user_data"].AsString;

				//  append the new user_data to the existing
				var updatedData = existingData + "\n" + data;

				// create the update thing for mongo to update user_data with
				var update = Builders<BsonDocument>.Update.Set("user_data", updatedData);

				// update the collection with the updated user_data at the correct doc using the _id filter
				await collection.UpdateOneAsync(filter, update);

			}
			else
			{
				// document wasn't found, create a new one with the existing _id
				var newDocument = new BsonDocument
				{
					{ "_id", _documentId },
					{ "user_data", data }
				};

				await collection.InsertOneAsync(newDocument);
				GD.Print("created doc with _id on file: " + _documentId);
			}
		}
	}
}
}
