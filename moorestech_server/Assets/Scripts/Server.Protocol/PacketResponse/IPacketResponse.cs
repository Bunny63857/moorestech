﻿using System.Collections.Generic;

namespace Server.Protocol.PacketResponse
{
    public interface IPacketResponse
    {
        public List<List<byte>> GetResponse(List<byte> payload);
    }
}