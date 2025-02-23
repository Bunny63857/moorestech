using System.Collections.Generic;
using Game.World.Interface.DataStore;
using Constant;
using MainGame.Network.Event;
using MainGame.UnityView.Chunk;
using UnityEngine;
using VContainer.Unity;

namespace MainGame.Presenter.Block
{
    /// <summary>
    ///     サーバーからのパケットを受け取り、Viewにブロックの更新情報を渡す
    ///     IInitializableがないとDIコンテナ作成時にインスタンスが生成されないので実装しています
    /// </summary>
    public class ChunkDataPresenter : IInitializable
    {
        private readonly Dictionary<Vector2Int, int[,]> _chunk = new();
        private readonly ChunkBlockGameObjectDataStore _chunkBlockGameObjectDataStore;

        public ChunkDataPresenter(ReceiveChunkDataEvent receiveChunkDataEvent, ChunkBlockGameObjectDataStore chunkBlockGameObjectDataStore)
        {
            _chunkBlockGameObjectDataStore = chunkBlockGameObjectDataStore;
            //イベントをサブスクライブする
            receiveChunkDataEvent.OnChunkUpdateEvent += OnChunkUpdate;
            receiveChunkDataEvent.OnBlockUpdateEvent += OnBlockUpdate;
        }

        public void Initialize()
        {
        }

        /// <summary>
        ///     チャンクの更新イベント
        /// </summary>
        private void OnChunkUpdate(ChunkUpdateEventProperties properties)
        {
            var chunkPos = properties.ChunkPos;
            //チャンクの情報を追加か更新
            _chunk[chunkPos] = properties.BlockIds;

            //ブロックの更新イベントを発行
            for (var i = 0; i < ChunkConstant.ChunkSize; i++)
            for (var j = 0; j < ChunkConstant.ChunkSize; j++)
                ViewPlaceOrRemoveBlock(chunkPos + new Vector2Int(i, j), properties.BlockIds[i, j], properties.BlockDirections[i, j]);
        }

        /// <summary>
        ///     単一のブロックの更新イベント
        /// </summary>
        private void OnBlockUpdate(BlockUpdateEventProperties properties)
        {
            var blockPos = properties.BlockPos;
            var chunkPos = ChunkConstant.BlockPositionToChunkOriginPosition(blockPos);

            if (!_chunk.ContainsKey(chunkPos)) return;

            //ブロックを置き換え
            var (i, j) = (
                GetBlockArrayIndex(chunkPos.x, blockPos.x),
                GetBlockArrayIndex(chunkPos.y, blockPos.y));
            _chunk[chunkPos][i, j] = properties.BlockId;

            //viewにブロックがおかれたことを通知する
            ViewPlaceOrRemoveBlock(blockPos, properties.BlockId, properties.BlockDirection);
        }

        private void ViewPlaceOrRemoveBlock(Vector2Int position, int id, BlockDirection blockDirection)
        {
            if (id == BlockConstant.NullBlockId)
            {
                _chunkBlockGameObjectDataStore.GameObjectBlockRemove(position);
                return;
            }

            _chunkBlockGameObjectDataStore.GameObjectBlockPlace(position, id, blockDirection);
        }


        /// <summary>
        ///     ブロックの座標とチャンクの座標から、IDの配列のインデックスを取得する
        /// </summary>
        private Vector2Int GetBlockArrayIndex(Vector2Int chunkPos, Vector2Int blockPos)
        {
            var (x, y) = (
                GetBlockArrayIndex(chunkPos.x, blockPos.x),
                GetBlockArrayIndex(chunkPos.y, blockPos.y));
            return new Vector2Int(x, y);
        }

        private int GetBlockArrayIndex(int chunkPos, int blockPos)
        {
            if (0 <= chunkPos) return blockPos - chunkPos;
            return -chunkPos - -blockPos;
        }
    }
}