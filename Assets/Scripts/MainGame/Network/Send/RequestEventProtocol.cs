using System.Collections.Generic;
using MainGame.Network.Util;

namespace MainGame.Network.Send
{
    public class RequestEventProtocol
    {
        private const short ProtocolId = 4;
        private readonly ISocket _socket;

        public RequestEventProtocol(ISocket socket)
        {
            _socket = socket;
        }

        public void Send(int playerId)
        {
            var packet = new List<byte>();
            
            packet.AddRange(ToByteList.Convert(ProtocolId));
            packet.AddRange(ToByteList.Convert(playerId));
            
            _socket.Send(packet.ToArray());
        }
    }
}