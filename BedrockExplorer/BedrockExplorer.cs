using System.Net;
using System.Net.Sockets;
using BedrockExplorer.Network;
using static BedrockExplorer.Network.RakNet;

namespace BedrockExplorer;

public class BedrockExplorer {

    private readonly RakNet _rakNet = new();

    public class Query(PingResponse pingResponse, Exception exception) {

        public bool IsOk() {
            return pingResponse != null! || exception == null!;
        }

        public PingResponse GetResponse() {
            return pingResponse;
        }

        public Exception GetError() {
            return exception;
        }
    }

    private static async Task<IPAddress?> GetAddressFromDomainAsync(string address) {

        try {
            var results = await Dns.GetHostAddressesAsync(address);
            return results[0];
        } catch (SocketException) {
            return null;
        }
    }

    /// <summary>
    /// This function executes a Query to the specified server.
    /// </summary>
    /// <param name="address">Server address.</param>
    /// <param name="port">Server port.</param>
    /// <param name="timeout">Query timeout.</param>
    /// <returns>Get server information, it returns the <see cref="RakNet.PingResponse"/> class.</returns>
    public async Task<Query> QueryServer(string address, int port, int timeout) {

        var numericalAddress = await GetAddressFromDomainAsync(address);
        if (numericalAddress == null!)
            return new Query(null!, new Exception("Invalid address, unable to get numeric address from domain."));
        
        var response = await _rakNet.SendUnconnectedPingAsync(new IPEndPoint(numericalAddress, port), timeout);
        return response == null! ? new Query(null!, new Exception($"No response received from server, elapsed time: {timeout}ms")) : new Query(response, null!);
    }
}