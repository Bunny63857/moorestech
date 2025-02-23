using System.Collections.Generic;
using System.Linq;
using Core.Item;
using Core.Item.Config;
using Game.Block.BlockInventory;
using Game.Block.Interface;
using Game.Block.Interface.BlockConfig;
using Game.PlayerInventory.Interface;
using Game.World.Interface.DataStore;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Server.Boot;
using Server.Protocol.PacketResponse;
using Tests.Module.TestMod;

namespace Tests.CombinedTest.Server.PacketTest
{
    public class RemoveBlockProtocolTest
    {
        private const int MachineBlockId = 1;
        private const int PlayerId = 0;

        [Test]
        public void RemoveTest()
        {
            var (packet, serviceProvider) =
                new PacketResponseCreatorDiContainerGenerators().Create(TestModDirectory.ForUnitTestModDirectory);
            var worldBlock = serviceProvider.GetService<IWorldBlockDatastore>();
            var blockFactory = serviceProvider.GetService<IBlockFactory>();
            var blockConfig = serviceProvider.GetService<IBlockConfig>();
            var itemStackFactory = serviceProvider.GetService<ItemStackFactory>();

            var playerInventoryData =
                serviceProvider.GetService<IPlayerInventoryDataStore>().GetInventoryData(PlayerId);

            var block = blockFactory.Create(MachineBlockId, 0);
            var blockInventory = (IBlockInventory)block;
            blockInventory.InsertItem(itemStackFactory.Create(10, 7));
            var blockConfigData = blockConfig.GetBlockConfig(block.BlockId);

            //削除するためのブロックの生成
            worldBlock.AddBlock(block, 0, 0, BlockDirection.North);

            Assert.AreEqual(0, worldBlock.GetBlock(0, 0).EntityId);

            //プレイヤーインベントリに削除したブロックを追加


            //プロトコルを使ってブロックを削除
            packet.GetPacketResponse(RemoveBlock(0, 0, PlayerId));


            //削除したブロックがワールドに存在しないことを確認
            Assert.False(worldBlock.Exists(0, 0));


            var playerSlotIndex = PlayerInventoryConst.HotBarSlotToInventorySlot(0);
            //ブロック内のアイテムがインベントリに入っているか
            Assert.AreEqual(10, playerInventoryData.MainOpenableInventory.GetItem(playerSlotIndex).Id);
            Assert.AreEqual(7, playerInventoryData.MainOpenableInventory.GetItem(playerSlotIndex).Count);

            //削除したブロックは次のスロットに入っているのでそれをチェック
            Assert.AreEqual(blockConfigData.ItemId,
                playerInventoryData.MainOpenableInventory.GetItem(playerSlotIndex + 1).Id);
            Assert.AreEqual(1, playerInventoryData.MainOpenableInventory.GetItem(playerSlotIndex + 1).Count);
        }


        //インベントリがいっぱいで一部のアイテムが残っている場合のテスト
        [Test]
        public void InventoryFullToRemoveBlockSomeItemRemainTest()
        {
            var (packet, serviceProvider) =
                new PacketResponseCreatorDiContainerGenerators().Create(TestModDirectory.ForUnitTestModDirectory);
            var worldBlock = serviceProvider.GetService<IWorldBlockDatastore>();
            var blockFactory = serviceProvider.GetService<IBlockFactory>();
            var itemConfig = serviceProvider.GetService<IItemConfig>();
            var itemStackFactory = serviceProvider.GetService<ItemStackFactory>();

            var mainInventory =
                serviceProvider.GetService<IPlayerInventoryDataStore>().GetInventoryData(PlayerId)
                    .MainOpenableInventory;

            //インベントリの2つのスロットを残してインベントリを満杯にする
            for (var i = 2; i < mainInventory.GetSlotSize(); i++)
                mainInventory.SetItem(i, itemStackFactory.Create(1000, 1));

            //一つの目のスロットにはID3の最大スタック数から1個少ないアイテムを入れる
            var id3MaxStack = itemConfig.GetItemConfig(3).MaxStack;
            mainInventory.SetItem(0, itemStackFactory.Create(3, id3MaxStack - 1));
            //二つめのスロットにはID4のアイテムを1つ入れておく
            mainInventory.SetItem(1, itemStackFactory.Create(4, 1));


            //削除するためのブロックの生成
            var block = blockFactory.Create(MachineBlockId, 0);
            var blockInventory = (IBlockInventory)block;
            //ブロックにはID3のアイテムを2個と、ID4のアイテムを5個入れる
            //このブロックを削除したときに、ID3のアイテムが1個だけ残る
            blockInventory.SetItem(0, itemStackFactory.Create(3, 2));
            blockInventory.SetItem(1, itemStackFactory.Create(4, 5));

            //ブロックを設置
            worldBlock.AddBlock(block, 0, 0, BlockDirection.North);


            //プロトコルを使ってブロックを削除
            packet.GetPacketResponse(RemoveBlock(0, 0, PlayerId));


            //削除したブロックがワールドに存在してることを確認
            Assert.True(worldBlock.Exists(0, 0));

            //プレイヤーのインベントリにブロック内のアイテムが入っているか確認
            Assert.AreEqual(itemStackFactory.Create(3, id3MaxStack), mainInventory.GetItem(0));
            Assert.AreEqual(itemStackFactory.Create(4, 6), mainInventory.GetItem(1));

            //ブロックのインベントリが減っているかを確認
            Assert.AreEqual(itemStackFactory.Create(3, 1), blockInventory.GetItem(0));
            Assert.AreEqual(itemStackFactory.CreatEmpty(), blockInventory.GetItem(1));
        }

        //ブロックの中にアイテムはないけど、プレイヤーのインベントリが満杯でブロックを破壊できない時のテスト
        [Test]
        public void InventoryFullToCantRemoveBlockTest()
        {
            var (packet, serviceProvider) =
                new PacketResponseCreatorDiContainerGenerators().Create(TestModDirectory.ForUnitTestModDirectory);
            var worldBlock = serviceProvider.GetService<IWorldBlockDatastore>();
            var blockFactory = serviceProvider.GetService<IBlockFactory>();
            var itemConfig = serviceProvider.GetService<IItemConfig>();
            var itemStackFactory = serviceProvider.GetService<ItemStackFactory>();

            var mainInventory =
                serviceProvider.GetService<IPlayerInventoryDataStore>().GetInventoryData(PlayerId)
                    .MainOpenableInventory;

            //インベントリを満杯にする
            for (var i = 0; i < mainInventory.GetSlotSize(); i++)
                mainInventory.SetItem(i, itemStackFactory.Create(1000, 1));

            //ブロックを設置
            var block = blockFactory.Create(MachineBlockId, 0);
            worldBlock.AddBlock(block, 0, 0, BlockDirection.North);


            //プロトコルを使ってブロックを削除
            packet.GetPacketResponse(RemoveBlock(0, 0, PlayerId));


            //ブロックが削除できていないことを検証
            Assert.True(worldBlock.Exists(0, 0));
        }


        private List<byte> RemoveBlock(int x, int y, int playerId)
        {
            return MessagePackSerializer.Serialize(new RemoveBlockProtocolMessagePack(playerId, x, y)).ToList();
        }
    }
}