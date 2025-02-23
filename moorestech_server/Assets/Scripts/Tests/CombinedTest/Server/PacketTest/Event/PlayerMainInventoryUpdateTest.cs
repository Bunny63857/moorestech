using System.Collections.Generic;
using System.Linq;
using Core.Item;
using Game.PlayerInventory.Interface;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Server.Boot;
using Server.Event.EventReceive;
using Server.Protocol.PacketResponse;
using Server.Protocol.PacketResponse.Util.InventoryMoveUtil;
using Tests.Module.TestMod;

namespace Tests.CombinedTest.Server.PacketTest.Event
{
    public class PlayerMainInventoryUpdateTest
    {
        private const int PlayerId = 0;

        [Test]
        public void UpdateTest()
        {
            var (packetResponse, serviceProvider) =
                new PacketResponseCreatorDiContainerGenerators().Create(TestModDirectory.ForUnitTestModDirectory);

            var response = packetResponse.GetPacketResponse(EventRequestData(0));
            Assert.AreEqual(0, response.Count);


            //インベントリにアイテムを追加
            var playerInventoryData = serviceProvider.GetService<IPlayerInventoryDataStore>().GetInventoryData(0);
            playerInventoryData.MainOpenableInventory.SetItem(5,
                serviceProvider.GetService<ItemStackFactory>().Create(1, 5));

            //追加時のイベントのキャッチ
            response = packetResponse.GetPacketResponse(EventRequestData(PlayerId));
            Assert.AreEqual(1, response.Count);

            //チェック
            var data = MessagePackSerializer.Deserialize<MainInventoryUpdateEventMessagePack>(response[0].ToArray());
            Assert.AreEqual(5, data.Slot);
            Assert.AreEqual(1, data.Item.Id);
            Assert.AreEqual(5, data.Item.Count);


            //インベントリ内のアイテムの移動を実際に移動のプロトコルを用いてテストする
            //分割のイベントのテスト
            packetResponse.GetPacketResponse(PlayerInventoryItemMove(true, 5, 3));
            packetResponse.GetPacketResponse(PlayerInventoryItemMove(false, 4, 3));

            response = packetResponse.GetPacketResponse(EventRequestData(PlayerId));

            Assert.AreEqual(4, response.Count);

            var grabUp = MessagePackSerializer.Deserialize<MainInventoryUpdateEventMessagePack>(response[0].ToArray());
            var setMainInventory =
                MessagePackSerializer.Deserialize<MainInventoryUpdateEventMessagePack>(response[1].ToArray());
            var outMainInventory =
                MessagePackSerializer.Deserialize<MainInventoryUpdateEventMessagePack>(response[2].ToArray());
            var grabDown =
                MessagePackSerializer.Deserialize<MainInventoryUpdateEventMessagePack>(response[3].ToArray());

            Assert.AreEqual(GrabInventoryUpdateToSetEventPacket.EventTag, grabUp.EventTag); //イベントタグの確認 アイテムを持ち上げる
            Assert.AreEqual(MainInventoryUpdateToSetEventPacket.EventTag, setMainInventory.EventTag); //インベントリのアイテムがへる
            Assert.AreEqual(MainInventoryUpdateToSetEventPacket.EventTag,
                outMainInventory.EventTag); //インベントリにアイテムがセットされる
            Assert.AreEqual(GrabInventoryUpdateToSetEventPacket.EventTag, grabDown.EventTag); //アイテムが置かれる

            Assert.AreEqual(0, grabUp.Slot); //移動時のスロット確認
            Assert.AreEqual(5, setMainInventory.Slot);
            Assert.AreEqual(4, outMainInventory.Slot);
            Assert.AreEqual(0, grabDown.Slot);

            Assert.AreEqual(1, grabUp.Item.Id); //アイテムIDの確認
            Assert.AreEqual(1, setMainInventory.Item.Id);
            Assert.AreEqual(1, outMainInventory.Item.Id);
            Assert.AreEqual(0, grabDown.Item.Id);

            Assert.AreEqual(3, grabUp.Item.Count); //アイテム数の確認
            Assert.AreEqual(2, setMainInventory.Item.Count);
            Assert.AreEqual(3, outMainInventory.Item.Count);
            Assert.AreEqual(0, grabDown.Item.Count);


            //合成のテスト
            packetResponse.GetPacketResponse(PlayerInventoryItemMove(true, 4, 3));
            packetResponse.GetPacketResponse(PlayerInventoryItemMove(false, 5, 3));

            response = packetResponse.GetPacketResponse(EventRequestData(PlayerId));

            Assert.AreEqual(4, response.Count);
            grabUp = MessagePackSerializer.Deserialize<MainInventoryUpdateEventMessagePack>(response[0].ToArray());
            setMainInventory =
                MessagePackSerializer.Deserialize<MainInventoryUpdateEventMessagePack>(response[1].ToArray());
            outMainInventory =
                MessagePackSerializer.Deserialize<MainInventoryUpdateEventMessagePack>(response[2].ToArray());
            grabDown = MessagePackSerializer.Deserialize<MainInventoryUpdateEventMessagePack>(response[3].ToArray());


            Assert.AreEqual(GrabInventoryUpdateToSetEventPacket.EventTag, grabUp.EventTag); //イベントタグの確認 アイテムを持ち上げる
            Assert.AreEqual(MainInventoryUpdateToSetEventPacket.EventTag, setMainInventory.EventTag); //インベントリのアイテムがへる
            Assert.AreEqual(MainInventoryUpdateToSetEventPacket.EventTag,
                outMainInventory.EventTag); //インベントリにアイテムがセットされる
            Assert.AreEqual(GrabInventoryUpdateToSetEventPacket.EventTag, grabDown.EventTag); //アイテムが置かれる

            Assert.AreEqual(0, grabUp.Slot); //移動時のスロット確認
            Assert.AreEqual(4, setMainInventory.Slot);
            Assert.AreEqual(5, outMainInventory.Slot);
            Assert.AreEqual(0, grabDown.Slot);

            Assert.AreEqual(1, grabUp.Item.Id); //アイテムIDの確認
            Assert.AreEqual(0, setMainInventory.Item.Id);
            Assert.AreEqual(1, outMainInventory.Item.Id);
            Assert.AreEqual(0, grabDown.Item.Id);

            Assert.AreEqual(3, grabUp.Item.Count); //アイテム数の確認
            Assert.AreEqual(0, setMainInventory.Item.Count);
            Assert.AreEqual(5, outMainInventory.Item.Count);
            Assert.AreEqual(0, grabDown.Item.Count);
        }


        private List<byte> EventRequestData(int plyaerID)
        {
            return MessagePackSerializer.Serialize(new EventProtocolMessagePack(plyaerID)).ToList();
            ;
        }

        private List<byte> PlayerInventoryItemMove(bool toGrab, int inventorySlot, int itemCount)
        {
            InventoryItemMoveProtocolMessagePack messagePack;
            if (toGrab)
            {
                var from = new ItemMoveInventoryInfo(ItemMoveInventoryType.MainInventory);
                var to = new ItemMoveInventoryInfo(ItemMoveInventoryType.GrabInventory);
                messagePack = new InventoryItemMoveProtocolMessagePack(PlayerId, itemCount, ItemMoveType.SwapSlot,
                    from,inventorySlot, to,0);
            }
            else
            {
                var from = new ItemMoveInventoryInfo(ItemMoveInventoryType.GrabInventory);
                var to = new ItemMoveInventoryInfo(ItemMoveInventoryType.MainInventory);
                messagePack = new InventoryItemMoveProtocolMessagePack(PlayerId, itemCount, ItemMoveType.SwapSlot, 
                    from,0, to,inventorySlot);
            }

            return MessagePackSerializer.Serialize(messagePack).ToList();
        }
    }
}