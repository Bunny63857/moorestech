using System.Collections.Generic;
using System.Linq;
using Core.Item;
using Core.Item.Config;
using Core.Item.Util;
using Game.PlayerInventory.Interface;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using PlayerInventory;
using Server;
using Server.PacketHandle;
using Server.Util;
using Test.Module;

namespace Test.CombinedTest.Server.PacketTest
{
    public class PlayerInventoryProtocolTest
    {
        [Test]
        public void GetPlayerInventoryProtocolTest()
        {
            int playerId = 1;

            var (packet, serviceProvider) = new PacketResponseCreatorDiContainerGenerators().Create();


            //1回目のデータ要求
            var payload = new List<byte>();
            payload.AddRange(ToByteList.Convert((short) 3));
            payload.AddRange(ToByteList.Convert(playerId));

            var response = new ByteArrayEnumerator(packet.GetPacketResponse(payload)[0].ToList());

            //データの検証
            Assert.AreEqual(4, response.MoveNextToGetShort());
            Assert.AreEqual(playerId, response.MoveNextToGetInt());
            Assert.AreEqual(0, response.MoveNextToGetShort());
            //プレイヤーインベントリの検証
            for (int i = 0; i < PlayerInventoryConst.MainInventoryColumns; i++)
            {
                Assert.AreEqual(ItemConst.EmptyItemId, response.MoveNextToGetInt());
                Assert.AreEqual(0, response.MoveNextToGetInt());
            }
            //クラフトインベントリの検証
            for (int i = 0; i < PlayerInventoryConst.CraftingInventorySize; i++)
            {
                Assert.AreEqual(ItemConst.EmptyItemId, response.MoveNextToGetInt());
                Assert.AreEqual(0, response.MoveNextToGetInt());
            }
            //クラフト結果アイテムの検証
            Assert.AreEqual(ItemConst.EmptyItemId, response.MoveNextToGetInt());
            Assert.AreEqual(0, response.MoveNextToGetInt());
            //クラフト不可能である事の検証
            Assert.AreEqual(0, response.MoveNextToGetByte());
            
            
            
            //2回目のデータ要求のためにアイテムをセットする
            var playerInventoryData =
                serviceProvider.GetService<IPlayerInventoryDataStore>().GetInventoryData(playerId);
            ItemStackFactory itemStackFactory = new ItemStackFactory(new TestItemConfig());
            playerInventoryData.MainInventory.SetItem(0, itemStackFactory.Create(1, 5));
            playerInventoryData.MainInventory.SetItem(20, itemStackFactory.Create(3, 1));
            playerInventoryData.MainInventory.SetItem(34, itemStackFactory.Create(10, 7));


            //2回目のデータ要求
            response = new ByteArrayEnumerator(packet.GetPacketResponse(payload)[0].ToList());
            Assert.AreEqual(4, response.MoveNextToGetShort());
            Assert.AreEqual(playerId, response.MoveNextToGetInt());
            Assert.AreEqual(0, response.MoveNextToGetShort());

            //データの検証
            for (int i = 0; i < PlayerInventoryConst.MainInventorySize; i++)
            {
                if (i == 0)
                {
                    Assert.AreEqual(1, response.MoveNextToGetInt());
                    Assert.AreEqual(5, response.MoveNextToGetInt());
                }
                else if (i == 20)
                {
                    Assert.AreEqual(3, response.MoveNextToGetInt());
                    Assert.AreEqual(1, response.MoveNextToGetInt());
                }
                else if (i == 34)
                {
                    Assert.AreEqual(10, response.MoveNextToGetInt());
                    Assert.AreEqual(7, response.MoveNextToGetInt());
                }
                else
                {
                    Assert.AreEqual(ItemConst.EmptyItemId, response.MoveNextToGetInt());
                    Assert.AreEqual(0, response.MoveNextToGetInt());
                }
            }
            //クラフトインベントリの検証
            for (int i = 0; i < PlayerInventoryConst.CraftingInventorySize; i++)
            {
                Assert.AreEqual(ItemConst.EmptyItemId, response.MoveNextToGetInt());
                Assert.AreEqual(0, response.MoveNextToGetInt());
            }
            //クラフト結果アイテムの検証
            Assert.AreEqual(ItemConst.EmptyItemId, response.MoveNextToGetInt());
            Assert.AreEqual(0, response.MoveNextToGetInt());
            //クラフト不可能である事の検証
            Assert.AreEqual(0, response.MoveNextToGetByte());
        }
    }
}