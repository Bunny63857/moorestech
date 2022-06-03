using System;
using System.Collections.Generic;
using Core.Item;
using Game.PlayerInventory.Interface;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Server.Util;

namespace Server.Protocol.PacketResponse
{
    public class SendCommandProtocol : IPacketResponse
    {
        public const string Tag = "va:sendCommand";
        
        private readonly IPlayerInventoryDataStore _playerInventoryDataStore;
        private readonly ItemStackFactory _itemStackFactory;
        public SendCommandProtocol(ServiceProvider serviceProvider)
        {
            _playerInventoryDataStore = serviceProvider.GetService<IPlayerInventoryDataStore>();
            _itemStackFactory = serviceProvider.GetService<ItemStackFactory>();
        }

        public List<List<byte>> GetResponse(List<byte> payload)
        {
            var data = MessagePackSerializer.Deserialize<SendCommandProtocolMessagePack>(payload.ToArray());
            
            
            var command = data.Command.Split(' ');//command text
            
            //他のコマンドを実装する場合、この実装方法をやめる
            if (command[0] == "give")
            {
                var inventory = _playerInventoryDataStore.GetInventoryData(int.Parse(command[1]));
                var item = _itemStackFactory.Create(int.Parse(command[2]), int.Parse(command[3]));
                inventory.MainOpenableInventory.InsertItem(item);
            }
            
            return new List<List<byte>>();
        }
    }
    
        
    [MessagePackObject(keyAsPropertyName :true)]
    public class SendCommandProtocolMessagePack : ProtocolMessagePackBase
    {
        public string Command { get; set; }
    }
}