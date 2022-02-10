﻿using System.Collections.Generic;
using System.Linq;
using MainGame.Basic;
using MainGame.Network.Event;
using MainGame.Network.Util;
using UnityEngine;

namespace MainGame.Network.Receive
{
    public class ReceiveChunkDataProtocol : IAnalysisPacket
    {
        private readonly ChunkUpdateEvent _chunkUpdateEvent;

        public ReceiveChunkDataProtocol(ChunkUpdateEvent chunkUpdateEvent)
        {
            _chunkUpdateEvent = chunkUpdateEvent;
        }

        public void Analysis(List<byte> data)
        {
            var bits = new BitListEnumerator(data.ToList());
            //packet id
            bits.MoveNextToShort();
            var x = bits.MoveNextToInt();
            var y = bits.MoveNextToInt();
            var chunkPos = new Vector2Int(x, y);

            var chunkBlocks = new int[ChunkConstant.ChunkSize,ChunkConstant.ChunkSize];
            
            //analysis block data
            for (int i = 0; i < ChunkConstant.ChunkSize; i++)
            {
                for (int j = 0; j < ChunkConstant.ChunkSize; j++)
                {
                    chunkBlocks[i, j] = GetBlockId(bits);
                }
            }
            
            //chunk data event
            _chunkUpdateEvent.InvokeChunkUpdateEvent(new OnChunkUpdateEventProperties(chunkPos, chunkBlocks));
        }

        private int GetBlockId(BitListEnumerator bits)
        {
            var isBlock = bits.MoveNextToBit();
            if (isBlock)
            {
                var isInt = bits.MoveNextToBit();
                if (isInt)
                {
                    //block id type is int
                    bits.MoveNextToBit();
                    return bits.MoveNextToInt();
                }
                var isShort = bits.MoveNextToBit();
                if (isShort)
                {
                    //block id type is short
                    return bits.MoveNextToShort();
                }
                //block id type is byte
                return bits.MoveNextToByte();
            }
            //none block
            return BlockConstant.NullBlockId;
        }
    }
}