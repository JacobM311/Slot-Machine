using Godot;
using System;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace MongoDB
{
    public partial class MongoDB : Node
    {
        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
			Task.Run(async () => await Insert());
        }

		static async Task Insert()
		{
			string connectionString = "mongodb+srv://jacobmarshall9608:h20uaJnIIg6dpnSF@cluster0.7osdg5a.mongodb.net/";
			string databaseName = "SlotMachine";
			string collectionName = "data";

			var client = new MongoClient(connectionString);

            // Get a reference to the database
            var database = client.GetDatabase(databaseName);

            // Get a reference to the collection
            var collection = database.GetCollection<BsonDocument>(collectionName);

            // Create a new document
            var document = new BsonDocument { { "_id", 2m }, { "x", 4 } };

            // Insert the document into the collection
            try
            {
                await collection.InsertOneAsync(document);
                GD.Print("Document inserted successfully.");
            }
            catch (Exception ex)
            {
                GD.Print($"Error inserting document: {ex.Message}");
            }
		}

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }
    }
}
