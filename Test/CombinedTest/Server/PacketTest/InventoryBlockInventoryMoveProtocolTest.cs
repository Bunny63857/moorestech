using System.Collections.Generic;
using Core.Block.BlockFactory;
using Core.Block.Blocks.Machine;
using Core.Inventory;
using Core.Item;
using Game.PlayerInventory.Interface;
using Game.World.Interface.DataStore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Server;
using Server.Protocol;
using Server.Util;
using Test.Module.TestConfig;

namespace Test.CombinedTest.Server.PacketTest
{
    public class InventoryBlockInventoryMoveProtocolTest
    {
        private const int MachineBlockId = 1;
        [Test]
        public void VanillaMachineInventoryItemMove()
        {
            int playerId = 1;
            int playerSlotIndex = 2;
            int blockInventorySlotIndex = 0;

            //初期設定----------------------------------------------------------

            var (packet, serviceProvider) = new PacketResponseCreatorDiContainerGenerators().Create(TestModuleConfigPath.FolderPath);
            var itemStackFactory = serviceProvider.GetService<ItemStackFactory>();

            //ブロックの作成、設置
            int x = 0;
            int y = 0;
            var blockInventoryData =
                (VanillaMachine)serviceProvider.GetService<BlockFactory>().Create(MachineBlockId,1);
            serviceProvider.GetService<IWorldBlockDatastore>().AddBlock(blockInventoryData,0,0,BlockDirection.East);

            Check(blockInventoryData, itemStackFactory, packet, x, y);
        }

        private void Check(
            IOpenableInventory blockOpenableInventoryData,
            ItemStackFactory itemStackFactory,
            PacketResponseCreator packet,
            int x,int y)
        {
            //アイテムの設定
            blockOpenableInventoryData.SetItem(0, itemStackFactory.Create(1, 5));
            blockOpenableInventoryData.SetItem(1, itemStackFactory.Create(2, 1));

            //実際に移動させてテスト
            //全てのアイテムを移動させるテスト
            packet.GetPacketResponse(BlockInventoryItemMove(0, 2, 5, x,y));
            Assert.AreEqual(blockOpenableInventoryData.GetItem(0), itemStackFactory.CreatEmpty());
            Assert.AreEqual(blockOpenableInventoryData.GetItem(2), itemStackFactory.Create(1, 5));

            //一部のアイテムを移動させるテスト
            packet.GetPacketResponse(BlockInventoryItemMove(2, 0, 3, x,y));
            Assert.AreEqual(blockOpenableInventoryData.GetItem(0), itemStackFactory.Create(1, 3));
            Assert.AreEqual(blockOpenableInventoryData.GetItem(2), itemStackFactory.Create(1, 2));

            //一部のアイテムを移動しようとするが他にスロットがあるため失敗するテスト
            packet.GetPacketResponse(BlockInventoryItemMove(0, 1, 1, x,y));
            Assert.AreEqual(blockOpenableInventoryData.GetItem(0), itemStackFactory.Create(1, 3));
            Assert.AreEqual(blockOpenableInventoryData.GetItem(1), itemStackFactory.Create(2, 1));

            //全てのアイテムを移動させるテスト
            packet.GetPacketResponse(BlockInventoryItemMove(0, 1, 3, x,y));
            Assert.AreEqual(blockOpenableInventoryData.GetItem(0), itemStackFactory.Create(2, 1));
            Assert.AreEqual(blockOpenableInventoryData.GetItem(1), itemStackFactory.Create(1, 3));

            //アイテムを加算するテスト
            packet.GetPacketResponse(BlockInventoryItemMove(2, 1, 2, x,y));
            Assert.AreEqual(blockOpenableInventoryData.GetItem(2), itemStackFactory.CreatEmpty());
            Assert.AreEqual(blockOpenableInventoryData.GetItem(1), itemStackFactory.Create(1, 5));
            
        }
        

        private List<byte> BlockInventoryItemMove(int fromSlot,int toSlot,int itemCount,int x,int y)
        {
            var payload = new List<byte>();
            payload.AddRange(ToByteList.Convert((short) 7));
            payload.AddRange(ToByteList.Convert(x));
            payload.AddRange(ToByteList.Convert(y));
            payload.AddRange(ToByteList.Convert(fromSlot));
            payload.AddRange(ToByteList.Convert(toSlot));
            payload.AddRange(ToByteList.Convert(itemCount));
            return payload;
        }
    }
}