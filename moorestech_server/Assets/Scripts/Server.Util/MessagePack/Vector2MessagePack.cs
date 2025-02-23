using System;
using MessagePack;
using UnityEngine;

namespace Server.Util.MessagePack
{
    [MessagePackObject]
    public class Vector2MessagePack
    {
        [Obsolete("デシリアライズ用のコンストラクタです。基本的に使用しないでください。")]
        public Vector2MessagePack()
        {
        }

        public Vector2MessagePack(float x, float y)
        {
            X = x;
            Y = y;
        }

        public Vector2MessagePack(Vector2Int coreVector2Int)
        {
            X = coreVector2Int.x;
            Y = coreVector2Int.y;
        }


        [Key(0)] public float X { get; set; }

        [Key(1)] public float Y { get; set; }
    }
}