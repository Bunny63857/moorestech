using System.Collections.Generic;
using MainGame.UnityView.UI.Builder.Element;

namespace MainGame.UnityView.UI.Builder.BluePrint
{
    public class SubInventoryViewBluePrint : IInventoryViewBluePrint
    {
        public List<UIBluePrintItemSlot> OneSlots = new();
        public List<UIBluePrintItemSlotArray> ArraySlots = new();
        public List<TextElement> TextElements = new();

        public List<IUIBluePrintElement> Elements
        {
            get
            {
                var list = new List<IUIBluePrintElement>();
                list.AddRange(OneSlots);
                list.AddRange(ArraySlots);
                list.AddRange(TextElements);
                list.Sort((a,b) => b.Priority - a.Priority);
                return list;
            }
        }
    }
}