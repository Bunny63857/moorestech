using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Core.Inventory;
using Core.Item;
using Game.Crafting.Interface;
using Game.PlayerInventory.Interface;
using Game.PlayerInventory.Interface.Event;
using PlayerInventory.Event;

namespace PlayerInventory.ItemManaged
{
    public class CraftingOpenableInventoryData : ICraftingOpenableInventory
    {
        private readonly OpenableInventoryItemDataStoreService _openableInventoryService;
        private readonly int _playerId;
        private readonly CraftInventoryUpdateEvent _craftInventoryUpdateEvent;
        private readonly IIsCreatableJudgementService _isCreatableJudgementService;

        private readonly GrabInventoryData _grabInventoryData;
        private readonly MainOpenableInventoryData _mainOpenableInventoryData;

        public CraftingOpenableInventoryData(int playerId, CraftInventoryUpdateEvent craftInventoryUpdateEvent,
            ItemStackFactory itemStackFactory,IIsCreatableJudgementService isCreatableJudgementService, MainOpenableInventoryData mainOpenableInventoryData, GrabInventoryData grabInventoryData)
        {
            _playerId = playerId;
            
            _craftInventoryUpdateEvent = craftInventoryUpdateEvent;
            _isCreatableJudgementService = isCreatableJudgementService;
            _mainOpenableInventoryData = mainOpenableInventoryData;
            _grabInventoryData = grabInventoryData;
            _openableInventoryService = new OpenableInventoryItemDataStoreService(InvokeEvent, 
                itemStackFactory, PlayerInventoryConst.CraftingSlotSize);
        }
        public CraftingOpenableInventoryData(int playerId, CraftInventoryUpdateEvent craftInventoryUpdateEvent, ItemStackFactory itemStackFactory,IIsCreatableJudgementService isCreatableJudgementService,List<IItemStack> itemStacks, MainOpenableInventoryData mainOpenableInventoryData, GrabInventoryData grabInventoryData) : 
            this(playerId, craftInventoryUpdateEvent, itemStackFactory,isCreatableJudgementService, mainOpenableInventoryData, grabInventoryData)
        {
            for (int i = 0; i < itemStacks.Count; i++)
            {
                _openableInventoryService.SetItemWithoutEvent(i,itemStacks[i]);
            }
        }

        
        
        
        
        public void NormalCraft()
        {
            //クラフトが可能なアイテムの配置かチェック
            //クラフト結果のアイテムを持ちスロットに追加可能か判定
            if (!IsCreatable() && !_grabInventoryData.GetItem(0).IsAllowedToAdd(_isCreatableJudgementService.GetResult(CraftingItems))) return;
            
            //クラフト結果のアイテムを取得しておく
            var result = _isCreatableJudgementService.GetResult(CraftingItems);
            
            //クラフトしたアイテムを消費する
            var craftConfig = _isCreatableJudgementService.GetCraftingConfigData(CraftingItems);
            for (int i = 0; i < PlayerInventoryConst.CraftingSlotSize; i++)
            {
                //クラフトしたアイテムを消費する
                var subItem = _openableInventoryService.Inventory[i].SubItem(craftConfig.Items[i].Count);
                //インベントリにセット
                _openableInventoryService.SetItem(i, subItem);
            }
            
            
            //元のクラフト結果のアイテムを足したアイテムを持ちインベントリに追加
            var outputSlotItem = _grabInventoryData.GetItem(0);
            var addedOutputSlot = outputSlotItem.AddItem(result).ProcessResultItemStack;
            _grabInventoryData.SetItem(0, addedOutputSlot);
        }
        
        
        
        

        public void AllCraft()
        {
            throw new System.NotImplementedException();
        }

        public void OneStackCraft()
        {
            throw new System.NotImplementedException();
        }

        public IItemStack GetCreatableItem() { return _isCreatableJudgementService.GetResult(CraftingItems); }
        public bool IsCreatable() { return _isCreatableJudgementService.IsCreatable(CraftingItems); }


        private IReadOnlyList<IItemStack> CraftingItems => _openableInventoryService.Inventory;

        private void InvokeEvent(int slot, IItemStack itemStack)
        {
            _craftInventoryUpdateEvent.OnInventoryUpdateInvoke(
                new PlayerInventoryUpdateEventProperties(_playerId,slot,itemStack));
        }


        #region delgate to PlayerInventoryItemDataStoreService
        public ReadOnlyCollection<IItemStack> Items => _openableInventoryService.Items;
        public IItemStack GetItem(int slot) { return _openableInventoryService.GetItem(slot); }
        public void SetItem(int slot, IItemStack itemStack) { _openableInventoryService.SetItem(slot, itemStack); }
        public void SetItem(int slot, int itemId, int count) { _openableInventoryService.SetItem(slot, itemId, count); }
        public IItemStack ReplaceItem(int slot, IItemStack itemStack) { return _openableInventoryService.ReplaceItem(slot, itemStack); }
        public IItemStack ReplaceItem(int slot, int itemId, int count) { return _openableInventoryService.ReplaceItem(slot, itemId, count); }
        public IItemStack InsertItem(IItemStack itemStack) { return _openableInventoryService.InsertItem(itemStack); }
        public IItemStack InsertItem(int itemId, int count) { return _openableInventoryService.InsertItem(itemId, count); }
        public int GetSlotSize() { return _openableInventoryService.GetSlotSize(); }

        #endregion
    }
}