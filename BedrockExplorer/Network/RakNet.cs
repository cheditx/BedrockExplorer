using System.Net;
using System.Net.Sockets;
using System.Reflection;
using BedrockExplorer.Network.Packets;

namespace BedrockExplorer.Network;

public class RakNet {

    private readonly Socket _socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

    public class Edition(string edition) {
        
        private string EditionName { get; } = edition;

        public string GetEditionName() {
            return EditionName;
        }
        
        public bool IsEducationEdition() {
            return GetEditionName().Contains("MCEE");
        }

        public override string ToString() {
            return GetEditionName();
        }
    }

    public class Motd(string firstLine, string secondLine) {
        
        private string FirstLine { get; } = firstLine;

        private string SecondLine { get; } = secondLine;

        public string GetFirstLine() {
            return FirstLine;
        }
        
        public string GetSecondLine() {
            return SecondLine;
        }

        public override string ToString() {
            return $"\n- FirstLine: {GetFirstLine()}\n- SecondLine: {GetSecondLine()}";
        }
    }

    public class GameMode(string name, int number) {
        
        private string Name { get; } = name;

        private int Number { get; } = number;

        public string GetName() {
            return Name;
        }

        public int GetNumber() {
            return Number;
        }
        
        public override string ToString() {
            return $"\n- Name: {GetName()}\n- Number: {GetNumber()}";
        }
    }

    /// <summary>
    /// TThis class contains all the information that the server sends.
    /// </summary>
    public class PingResponse {
        
        public int Protocol { get; init; }
        
        public required string Version { get; init; }
        
        public int OnlinePlayers { get; init; }
        
        public int MaxOnlinePlayers { get; init; }
        
        public required string ServerUniqueId { get; init; }
        
        public required Edition Edition { get; init; }
        
        public required Motd Motd { get; init; }
        
        public required GameMode GameMode { get;  init; }
        
        public override string ToString() {
            
            var fields = typeof(PingResponse).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Select(x => x.Name);
            const string queryMessage = "Query Results:\n\n";
            
            return queryMessage + string.Join("\n", fields.Select(x => $"{x}: {GetType().GetProperty(x)!.GetValue(this)}"));
        }
    }

    public async Task<PingResponse> SendUnconnectedPingAsync(IPEndPoint ipEndPoint, int timeout) {

        var packet = new UnconnectedPing();
        await _socket.SendToAsync(packet.PacketArray, ipEndPoint);

        var responseBuffer = new byte[1024];
        var receiveTask = _socket.ReceiveFromAsync(responseBuffer, SocketFlags.None, ipEndPoint);
        
        var timeoutTask = Task.Delay(timeout);
        var completedTask = await Task.WhenAny(receiveTask, timeoutTask);

        if (completedTask == timeoutTask) {
            return null!;
        }

        var receivedBytes = await receiveTask;
        var unconnectedPong = new UnconnectedPong(responseBuffer.AsSpan(0, receivedBytes.ReceivedBytes));
        
        var splittedPayload = unconnectedPong.Payload.Split(";");
        var pingResponse = new PingResponse {
            Edition = new Edition(splittedPayload[0]),
            Motd = new Motd(splittedPayload[1], splittedPayload[7]),
            Protocol = Convert.ToInt32(splittedPayload[2]),
            Version = splittedPayload[3],
            OnlinePlayers = Convert.ToInt32(splittedPayload[4]),
            MaxOnlinePlayers = Convert.ToInt32(splittedPayload[5]),
            ServerUniqueId = splittedPayload[6],
            GameMode = new GameMode(splittedPayload[8], Convert.ToInt32(splittedPayload[9] == "" ? "0" : splittedPayload[9]))
        };

        return pingResponse;
    }
}