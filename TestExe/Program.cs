using cspotcode.SlopCrewClient;
using cspotcode.SlopCrewClient.SlopCrewAPI;

Console.WriteLine("Hello, World!");
APIManager.OnAPIRegistered += api => Console.WriteLine("registered");

Console.WriteLine(APIManager.API.Latency);
Console.WriteLine(APIManager.API.ServerAddress);
