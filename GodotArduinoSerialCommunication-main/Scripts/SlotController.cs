using Godot;
using System;
using System.IO.Ports;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace SlotController
{
    public partial class SlotController : Node2D
    {
        SerialPort serialPort;
        [Export] RichTextLabel text;
        bool hasHeardFromArduino = false;
        float timer;

        private static ObjectId _documentId; // Class-level variable to store the document _id

        [Signal] public delegate void SerialDataReceiverEventHandler(string data);

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            serialPort = new SerialPort
            {
                DtrEnable = true,
                RtsEnable = true,
                PortName = "COM4",
                BaudRate = 9600 // make sure this is the same in Arduino as it is in Godot.
            };
            serialPort.Open();
            serialPort.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            SerialDataReceiver += Recieved;
        }

        public override void _Process(double delta)
        {
            // Optional: you can put any repeated logic here
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
            Task.Run(async () => await InsertOrUpdate(data));
            text.Text = data;
        }

        static async Task InsertOrUpdate(string data)
{
    string connectionString = "mongodb+srv://jacobmarshall9608:h20uaJnIIg6dpnSF@cluster0.7osdg5a.mongodb.net/";
    string databaseName = "SlotMachine";
    string collectionName = "data";

    var client = new MongoClient(connectionString);

    // Get a reference to the database
    var database = client.GetDatabase(databaseName);

    // Get a reference to the collection
    var collection = database.GetCollection<BsonDocument>(collectionName);

    // Check if the document ID has been set
    if (_documentId == ObjectId.Empty)
    {
        // Create a new document with the initial data
        var document = new BsonDocument { { "user_data", data } };

        // Insert the document into the collection and get the generated _id
        try
        {
            await collection.InsertOneAsync(document);
            _documentId = document["_id"].AsObjectId;
            GD.Print("Document inserted successfully with _id: " + _documentId);
        }
        catch (Exception ex)
        {
            GD.Print($"Error inserting document: {ex.Message}");
        }
    }
    else
    {
        // Update the existing document by appending new data
        var filter = Builders<BsonDocument>.Filter.Eq("_id", _documentId);
        
        // Fetch the current document
        var currentDocument = await collection.Find(filter).FirstOrDefaultAsync();

        if (currentDocument != null)
        {
            // Get the current user data and append the new data
            var existingData = currentDocument["user_data"].AsString;
            var updatedData = existingData + "\n" + data; // Use "\n" to separate entries, adjust as needed

            // Create the update definition
            var update = Builders<BsonDocument>.Update.Set("user_data", updatedData);

            try
            {
                var result = await collection.UpdateOneAsync(filter, update);
                if (result.ModifiedCount > 0)
                {
                    GD.Print("Document updated successfully.");
                }
                else
                {
                    GD.Print("No document was updated.");
                }
            }
            catch (Exception ex)
            {
                GD.Print($"Error updating document: {ex.Message}");
            }
        }
        else
        {
            GD.Print("Document not found.");
        }
    }
}

    }
}
