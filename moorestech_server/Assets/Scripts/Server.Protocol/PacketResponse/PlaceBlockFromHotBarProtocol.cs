using System;
using System.Collections.Generic;
using Game.Block.Interface;
using Game.Block.Interface.BlockConfig;
using Game.PlayerInventory.Interface;
using Game.World.Interface.DataStore;
using Game.World.Interface.Util;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;

namespace Server.Protocol.PacketResponse
{
    public class SendPlaceHotBarBlockProtocol : IPacketResponse
    {
        public const string Tag = "va:palceHotbarBlock";
        private readonly IBlockFactory _blockFactory;

        private readonly IPlayerInventoryDataStore _playerInventoryDataStore;
        private readonly IWorldBlockDatastore _worldBlockDatastore;
        private readonly IBlockConfig _blockConfig;

        public SendPlaceHotBarBlockProtocol(ServiceProvider serviceProvider)
        {
            _blockConfig = serviceProvider.GetService<IBlockConfig>();
            _playerInventoryDataStore = serviceProvider.GetService<IPlayerInventoryDataStore>();
            _worldBlockDatastore = serviceProvider.GetService<IWorldBlockDatastore>();
            _blockFactory = serviceProvider.GetService<IBlockFactory>();
        }

        public List<List<byte>> GetResponse(List<byte> payload)
        {
            var data = MessagePackSerializer.Deserialize<SendPlaceHotBarBlockProtocolMessagePack>(payload.ToArray());


            var inventorySlot = PlayerInventoryConst.HotBarSlotToInventorySlot(data.Slot);
            var item = _playerInventoryDataStore.GetInventoryData(data.PlayerId).MainOpenableInventory
                .GetItem(inventorySlot);

            //アイテムIDがブロックIDに変換できない場合はそもまま処理を終了
            if (!_blockConfig.IsBlock(item.Id)) return new List<List<byte>>();
            //すでにブロックがある場合はそもまま処理を終了
            if (_worldBlockDatastore.Exists(data.X, data.Y)) return new List<List<byte>>();


            var blockDirection = data.Direction switch
            {
                0 => BlockDirection.North,
                1 => BlockDirection.East,
                2 => BlockDirection.South,
                3 => BlockDirection.West,
                _ => BlockDirection.North
            };


            //ブロックの作成
            var block = _blockFactory.Create(_blockConfig.ItemIdToBlockId(item.Id), CreateBlockEntityId.Create());
            //ブロックの設置
            _worldBlockDatastore.AddBlock(block, data.X, data.Y, blockDirection);

            //アイテムを減らし、セットする
            item = item.SubItem(1);
            _playerInventoryDataStore.GetInventoryData(data.PlayerId).MainOpenableInventory
                .SetItem(inventorySlot, item);


            return new List<List<byte>>();
        }
    }


    [MessagePackObject(true)]
    public class SendPlaceHotBarBlockProtocolMessagePack : ProtocolMessagePackBase
    {
        public SendPlaceHotBarBlockProtocolMessagePack(int playerId, int direction, int slot, int x, int y)
        {
            Tag = SendPlaceHotBarBlockProtocol.Tag;
            PlayerId = playerId;
            Direction = direction;
            Slot = slot;
            X = x;
            Y = y;
        }

        [Obsolete("デシリアライズ用のコンストラクタです。基本的に使用しないでください。")]
        public SendPlaceHotBarBlockProtocolMessagePack()
        {
        }

        public int PlayerId { get; set; }
        public int Direction { get; set; }
        public int Slot { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
    }
}