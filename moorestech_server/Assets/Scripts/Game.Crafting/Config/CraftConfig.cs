using System.Collections.Generic;
using System.Linq;
using Core.ConfigJson;
using Core.Item;
using Game.Crafting.Interface;

namespace Game.Crafting.Config
{
    public class CraftConfig : ICraftingConfig
    {
        public IReadOnlyList<CraftingConfigData> CraftingConfigList => _configDataList;
        private readonly List<CraftingConfigData> _configDataList;

        public CraftConfig(ItemStackFactory itemStackFactory, ConfigJsonList configJson)
        {
            //ロードしたコンフィグのデータを元に、CraftingConfigDataを作成
            _configDataList = new CraftConfigJsonLoad(itemStackFactory).Load(configJson.SortedCraftRecipeConfigJsonList);
        }

        public IReadOnlyList<CraftingConfigData> GetResultItemCraftingConfigList(int itemId)
        {
            return _configDataList.Where(configData => configData.ResultItem.Id == itemId).ToList();
        }

        public CraftingConfigData GetCraftingConfigData(int index)
        {
            return _configDataList[index];
        }
    }
}