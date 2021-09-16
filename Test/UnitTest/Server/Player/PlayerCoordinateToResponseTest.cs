﻿using System.Collections.Generic;
using industrialization.OverallManagement.DataStore;
using industrialization.OverallManagement.Util;
using NUnit.Framework;
using Server.Const;
using Server.PacketHandle.PacketResponse.Player;

namespace industrialization_test.UnitTest.Server.Player
{
    public class PlayerCoordinateToResponseTest
    {
        [Test]
        public void OneCoordinateResponseTest()
        {
            var p = new PlayerCoordinateToResponse();
            //1回目は全てを返す
            var cList = p.GetResponseCoordinate(CoordinateCreator.New(0, 0));

            var ans = new List<Coordinate>();
            if (ChunkResponseConst.ChunkSize != 20 || ChunkResponseConst.PlayerVisibleRangeChunk != 5)
            {
                Assert.Fail("Changed const?");
            }

            for (int i = -40; i <= 40; i+=ChunkResponseConst.ChunkSize)
            {
                for (int j = -40; j <= 40; j+=ChunkResponseConst.ChunkSize)
                {
                    ans.Add(CoordinateCreator.New(i, j));
                }
            }

            foreach (var a in ans)
            {
                Assert.True(cList.Contains(a));
            }
            //2回目は何も返さない
            cList = p.GetResponseCoordinate(CoordinateCreator.New(0, 0));
            Assert.AreEqual(0,cList.Count);
        }

        [Test]
        public void ShiftSideToCoordinateResponseTest()
        {
            
            var p = new PlayerCoordinateToResponse();
            //1回目は全てを返す
            var cList = p.GetResponseCoordinate(CoordinateCreator.New(0, 0));
            Assert.AreEqual(cList.Count,ChunkResponseConst.PlayerVisibleRangeChunk * ChunkResponseConst.PlayerVisibleRangeChunk);
            
            //2回目1チャンクx分を増加させる
            cList = p.GetResponseCoordinate(CoordinateCreator.New(25, 0));
            var ans = new List<Coordinate>();
            for (int i = -2; i < 3; i++)
            {
                ans.Add(CoordinateCreator.New( 60,i * ChunkResponseConst.ChunkSize));
            }
            
            foreach (var a in ans)
            {
                Assert.True(cList.Contains(a));
            }
        }
        
        [Test]
        public void ShiftSideAndUpToCoordinateResponseTest()
        {
            
            var p = new PlayerCoordinateToResponse();
            //1回目は全てを返す
            var cList = p.GetResponseCoordinate(CoordinateCreator.New(0, 0));
            Assert.AreEqual(cList.Count,ChunkResponseConst.PlayerVisibleRangeChunk * ChunkResponseConst.PlayerVisibleRangeChunk);
            
            //2回目1チャンクx分を増加させる
            cList = p.GetResponseCoordinate(CoordinateCreator.New(25, 25));
            var ans = new List<Coordinate>();
            for (int i = -1; i < 4; i++)
            {
                ans.Add(CoordinateCreator.New(i * ChunkResponseConst.ChunkSize, 60));
            }
            ans.Add(CoordinateCreator.New(-20, 60));
            ans.Add(CoordinateCreator.New(0, 60));
            ans.Add(CoordinateCreator.New(20, 60));
            ans.Add(CoordinateCreator.New(40, 60));
            
            foreach (var a in ans)
            {
                Assert.True(cList.Contains(a));
            }
        }
    }
}