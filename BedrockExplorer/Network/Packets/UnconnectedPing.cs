using System.Buffers.Binary;
using System.Security.Cryptography;

namespace BedrockExplorer.Network.Packets;

public sealed class UnconnectedPing : Packet {

    public override byte PacketId => 0x01;
    
    public readonly byte[] PacketArray = new byte[33];
    private static DateTimeOffset SentTime => DateTimeOffset.UtcNow;
    
    public UnconnectedPing() {

        Span<byte> span = PacketArray;
        span[0] = PacketId;
        
        BinaryPrimitives.WriteInt64LittleEndian(span.Slice(1, 8), SentTime.ToUnixTimeMilliseconds());
        Magic.AsSpan().CopyTo(span.Slice(9, 16));

        var randomBytes = new byte[8];
        RandomNumberGenerator.Fill(randomBytes);
        
        randomBytes.AsSpan().CopyTo(span.Slice(25, 8));
    }
}