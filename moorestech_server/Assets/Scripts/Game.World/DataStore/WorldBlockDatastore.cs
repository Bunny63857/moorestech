﻿using System;
using System.Collections.Generic;
using System.Linq;
using Core.Const;
using Game.Block.Interface;
using Game.Block.Interface.BlockConfig;
using Game.Block.Interface.State;
using Game.World.Event;
using Game.World.Interface.DataStore;
using Game.World.Interface.Event;
using UnityEngine;

namespace Game.World.DataStore
{
    /// <summary>
    ///     ワールドに存在するブロックとその座標の対応づけを行います。
    /// </summary>
    public class WorldBlockDatastore : IWorldBlockDatastore
    {
        private readonly IBlockConfig _blockConfig;
        private readonly IBlockFactory _blockFactory;

        //メインのデータストア
        private readonly Dictionary<int, WorldBlockData> _blockMasterDictionary = new();

        private readonly BlockPlaceEvent _blockPlaceEvent;
        private readonly BlockRemoveEvent _blockRemoveEvent;

        //座標とキーの紐づけ
        private readonly Dictionary<Vector2Int, int> _coordinateDictionary = new();


        private readonly IBlock _nullBlock = new NullBlock();

        public WorldBlockDatastore(IBlockPlaceEvent blockPlaceEvent, IBlockFactory blockFactory,
            IBlockRemoveEvent blockRemoveEvent, IBlockConfig blockConfig)
        {
            _blockFactory = blockFactory;
            _blockConfig = blockConfig;
            _blockRemoveEvent = (BlockRemoveEvent)blockRemoveEvent;
            _blockPlaceEvent = (BlockPlaceEvent)blockPlaceEvent;
        }

        public event Action<(ChangedBlockState state, IBlock block, int x, int y)> OnBlockStateChange;

        public bool AddBlock(IBlock block, int x, int y, BlockDirection blockDirection)
        {
            //既にキーが登録されてないか、同じ座標にブロックを置こうとしてないかをチェック
            if (!_blockMasterDictionary.ContainsKey(block.EntityId) &&
                !_coordinateDictionary.ContainsKey(new Vector2Int(x, y)))
            {
                var c = new Vector2Int(x, y);
                var data = new WorldBlockData(block, x, y, blockDirection, _blockConfig);
                _blockMasterDictionary.Add(block.EntityId, data);
                _coordinateDictionary.Add(c, block.EntityId);
                _blockPlaceEvent.OnBlockPlaceEventInvoke(new BlockPlaceEventProperties(c, data.Block, blockDirection));

                block.OnBlockStateChange += state => { OnBlockStateChange?.Invoke((state, block, x, y)); };

                return true;
            }

            return false;
        }

        public bool RemoveBlock(int x, int y)
        {
            if (!Exists(x, y)) return false;

            var entityId = GetEntityId(x, y);
            if (!_blockMasterDictionary.ContainsKey(entityId)) return false;

            var data = _blockMasterDictionary[entityId];

            _blockRemoveEvent.OnBlockRemoveEventInvoke(new BlockRemoveEventProperties(
                new Vector2Int(x, y), data.Block));

            _blockMasterDictionary.Remove(entityId);
            _coordinateDictionary.Remove(new Vector2Int(x, y));
            return true;
        }


        public IBlock GetBlock(int x, int y)
        {
            return GetBlockDatastore(x, y)?.Block ?? _nullBlock;
        }

        public WorldBlockData GetOriginPosBlock(int x, int y)
        {
            return _coordinateDictionary.TryGetValue(new Vector2Int(x, y), out var entityId)
                ? _blockMasterDictionary[entityId]
                : null;
        }

        public bool TryGetBlock(int x, int y, out IBlock block)
        {
            block = GetBlock(x, y);
            block ??= _nullBlock;
            return block != _nullBlock;
        }

        public (int, int) GetBlockPosition(int entityId)
        {
            if (_blockMasterDictionary.TryGetValue(entityId, out var data)) return (data.OriginX, data.OriginY);

            throw new Exception("ブロックがありません");
        }

        public BlockDirection GetBlockDirection(int x, int y)
        {
            var block = GetBlockDatastore(x, y);
            //TODO ブロックないときの処理どうしよう
            return block?.BlockDirection ?? BlockDirection.North;
        }


        public bool Exists(int x, int y)
        {
            return GetBlock(x, y).BlockId != BlockConst.EmptyBlockId;
        }

        private int GetEntityId(int x, int y)
        {
            return GetBlockDatastore(x, y).Block.EntityId;
        }

        /// <summary>
        ///     TODO GetBlockは頻繁に呼ばれる訳では無いが、この方式は効率が悪いのでなにか改善したい
        /// </summary>
        private WorldBlockData GetBlockDatastore(int x, int y)
        {
            foreach (var block in
                     _blockMasterDictionary.Where(block => block.Value.IsContain(x, y)))
                return block.Value;

            return null;
        }

        #region Component

        public bool ExistsComponentBlock<TComponent>(int x, int y)
        {
            return GetBlock(x, y) is TComponent;
        }

        public TComponent GetBlock<TComponent>(int x, int y)
        {
            var block = GetBlock(x, y);
            if (block is TComponent component) return component;

            throw new Exception("Block is not " + typeof(TComponent));
        }

        public bool TryGetBlock<TComponent>(int x, int y, out TComponent component)
        {
            if (ExistsComponentBlock<TComponent>(x, y))
            {
                component = GetBlock<TComponent>(x, y);
                return true;
            }

            component = default;
            return false;
        }

        #endregion


        #region Save&Load

        public List<SaveBlockData> GetSaveBlockDataList()
        {
            var list = new List<SaveBlockData>();
            foreach (var block in _blockMasterDictionary)
                list.Add(new SaveBlockData(
                    block.Value.OriginX,
                    block.Value.OriginY,
                    block.Value.Block.BlockHash,
                    block.Value.Block.EntityId,
                    block.Value.Block.GetSaveState(),
                    (int)block.Value.BlockDirection));

            return list;
        }

        public void LoadBlockDataList(List<SaveBlockData> saveBlockDataList)
        {
            foreach (var block in saveBlockDataList)
                AddBlock(
                    _blockFactory.Load(block.BlockHash, block.EntityId, block.State),
                    block.X,
                    block.Y,
                    (BlockDirection)block.Direction);
        }

        #endregion
    }
}