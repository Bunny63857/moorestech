﻿using Core.Item;

namespace Game.Block.BlockInventory
{
    /// <summary>
    ///     ベルトコンベアに乗っているアイテムを機械に入れたり、機械からベルトコンベアにアイテムを載せるなどの処理をするための共通インターフェース
    ///     ブロック同士でアイテムをやり取りしたいときに使う
    /// </summary>
    public interface IBlockInventory
    {
        public IItemStack InsertItem(IItemStack itemStack);
        public void AddOutputConnector(IBlockInventory blockInventory);
        public void RemoveOutputConnector(IBlockInventory blockInventory);

        public IItemStack GetItem(int slot);
        void SetItem(int slot, IItemStack itemStack);
        public int GetSlotSize();
    }
}