using System.Buffers.Binary;
using System.Text;

namespace BedrockExplorer.Network.Packets;

public sealed class UnconnectedPong : Packet {

    public override byte PacketId => 0x1c;

    public string Payload { get; } = null!;

    public long? TimeStamp { get; } = null!;

    public long? ServerGuid { get; } = null!;

    public UnconnectedPong(Span<byte> packet) {

        if (packet.Length < 33) {
            return;
        }

        var packetId = packet[0];
        if (packetId != PacketId) {
            return;
        }

        ServerGuid = BinaryPrimitives.ReadInt64LittleEndian(packet.Slice(1, 8));
        var magic = packet.Slice(17, 16);

        if (!magic.SequenceEqual(Magic)) {
            return;
        }

        Payload = Encoding.UTF8.GetString(packet[35..]);
        ServerGuid = BinaryPrimitives.ReadInt64LittleEndian(packet.Slice(9, 8));
    }
}