﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.PlayerInventory.Interface;
using MessagePack;
using Server.Util.MessagePack;
using UnityEngine;

namespace Server.Protocol.PacketResponse
{
    public class PlayerInventoryResponseProtocol : IPacketResponse
    {
        public const string Tag = "va:playerInvRequest";

        private readonly IPlayerInventoryDataStore _playerInventoryDataStore;

        public PlayerInventoryResponseProtocol(IPlayerInventoryDataStore playerInventoryDataStore)
        {
            _playerInventoryDataStore = playerInventoryDataStore;
        }

        public List<List<byte>> GetResponse(List<byte> payload)
        {
            var data = MessagePackSerializer.Deserialize<RequestPlayerInventoryProtocolMessagePack>(payload.ToArray());

            var playerInventory = _playerInventoryDataStore.GetInventoryData(data.PlayerId);

            //ExportInventoryLog(playerInventory);

            //メインインベントリのアイテムを設定
            var mainItems = new List<ItemMessagePack>();
            for (var i = 0; i < PlayerInventoryConst.MainInventorySize; i++)
            {
                var id = playerInventory.MainOpenableInventory.GetItem(i).Id;
                var count = playerInventory.MainOpenableInventory.GetItem(i).Count;
                mainItems.Add(new ItemMessagePack(id, count));
            }


            //グラブインベントリのアイテムを設定
            var grabItem = new ItemMessagePack(
                playerInventory.GrabInventory.GetItem(0).Id,
                playerInventory.GrabInventory.GetItem(0).Count);



            var response = MessagePackSerializer.Serialize(new PlayerInventoryResponseProtocolMessagePack(
                data.PlayerId, mainItems.ToArray(), grabItem));


            return new List<List<byte>> { response.ToList() };
        }


        /// <summary>
        ///     デバッグ用でインベントリの中身が知りたい時に使用する
        /// </summary>
        public static void ExportInventoryLog(PlayerInventoryData playerInventory, bool isExportMain,
            bool isExportCraft, bool isExportGrab)
        {
            var inventoryStr = new StringBuilder();
            inventoryStr.AppendLine("Main Inventory");


            if (isExportMain)
                //メインインベントリのアイテムを設定
                for (var i = 0; i < PlayerInventoryConst.MainInventorySize; i++)
                {
                    var id = playerInventory.MainOpenableInventory.GetItem(i).Id;
                    var count = playerInventory.MainOpenableInventory.GetItem(i).Count;

                    inventoryStr.Append(id + " " + count + "  ");
                    if ((i + 1) % PlayerInventoryConst.MainInventoryColumns == 0) inventoryStr.AppendLine();
                }

            inventoryStr.AppendLine();

            if (isExportGrab)
            {
                inventoryStr.AppendLine("Grab Inventory");
                inventoryStr.AppendLine(playerInventory.GrabInventory.GetItem(0).Id + " " +
                                        playerInventory.GrabInventory.GetItem(0).Count + "  ");
            }

            Debug.Log(inventoryStr);
        }
    }


    [MessagePackObject(true)]
    public class RequestPlayerInventoryProtocolMessagePack : ProtocolMessagePackBase
    {
        [Obsolete("デシリアライズ用のコンストラクタです。基本的に使用しないでください。")]
        public RequestPlayerInventoryProtocolMessagePack()
        {
        }

        public RequestPlayerInventoryProtocolMessagePack(int playerId)
        {
            Tag = PlayerInventoryResponseProtocol.Tag;
            PlayerId = playerId;
        }

        public int PlayerId { get; set; }
    }


    [MessagePackObject(true)]
    public class PlayerInventoryResponseProtocolMessagePack : ProtocolMessagePackBase
    {
        [Obsolete("デシリアライズ用のコンストラクタです。基本的に使用しないでください。")]
        public PlayerInventoryResponseProtocolMessagePack()
        {
        }


        public PlayerInventoryResponseProtocolMessagePack(int playerId, ItemMessagePack[] main, ItemMessagePack grab)
        {
            Tag = PlayerInventoryResponseProtocol.Tag;
            PlayerId = playerId;
            Main = main;
            Grab = grab;
        }

        public int PlayerId { get; set; }

        public ItemMessagePack[] Main { get; set; }
        public ItemMessagePack Grab { get; set; }
    }
}