using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MainGame.Basic.UI;
using MainGame.UnityView.UI.Inventory.View;
using MainGame.UnityView.UI.UIState.UIObject;
using UnityEngine;

namespace MainGame.UnityView.UI.Tutorial
{
    public class GameUIHighlight : MonoBehaviour
    {
        [SerializeField] private RectTransformHighlight rectTransformHighlight;

        [CanBeNull] delegate RectTransformReadonlyData RectTransformGetAction();
        
        /// <summary>
        /// 各タイプに応じた <see cref="RectTransformReadonlyData"/> を返すアクションを定義する
        /// わざわざアクションを定義する理由は、動的生成されるUIの場合、SerializeFieldで定義取得することができないため
        /// そのようなオブジェクトでも取得できるようにするためにこうする
        /// </summary>
        private readonly Dictionary<HighlightType,RectTransformGetAction> _rectTransformGetActions = new();

        [SerializeField] private RectTransform craftItemPutButton;
        [SerializeField] private PlayerInventorySlots playerInventorySlots;
        
        /// <summary>
        /// ハイライトのオブジェクトを実際に管理する
        /// </summary>
        private readonly Dictionary<HighlightType, RectTransformHighlightObject> _rectTransformHighlightObjects = new Dictionary<HighlightType, RectTransformHighlightObject>();
        /// <summary>
        /// <see cref="_rectTransformHighlightObjects"/>のキーを保持する
        /// わざわざ別でリストを保持する理由は、マイフレームキーのリストを生成して破棄するのはパフォーマンス上問題が発生しそうなので、
        /// 追加、削除のタイミングのみで更新を行うようにする
        /// </summary>
        private List<HighlightType> _rectTransformHighlightObjectKeys = new List<HighlightType>();

        private void Start()
        {
            var itemPutButtonReadonly = new RectTransformReadonlyData(craftItemPutButton);
            _rectTransformGetActions.Add(HighlightType.CraftItemPutButton, () => itemPutButtonReadonly);
            _rectTransformGetActions.Add(HighlightType.CraftResultSlot, () => playerInventorySlots.GetSlotRect(CraftInventoryObjectCreator.ResultSlotName));
        }

        public void SetHighlight(HighlightType highlightType,bool isActive)
        {
            var isExist = _rectTransformHighlightObjects.TryGetValue(highlightType, out var highlightObject);

            switch (isExist)
            {
                //ハイライトがない場合でオンにする場合は作成
                case false when isActive:
                {
                    _rectTransformHighlightObjects.Add(highlightType, null);
                    CreateAndSetTransformHighlightObject(highlightType);
                    break;
                }
                //ハイライトがあって、オフにする場合は削除
                case true when !isActive:
                    highlightObject?.Destroy();
                    _rectTransformHighlightObjects.Remove(highlightType);
                    break;
            }
            //Dictionaryが変更されたのでキーのリストを更新
            _rectTransformHighlightObjectKeys = _rectTransformHighlightObjects.Keys.ToList();
        }

        
        /// <summary>
        /// オブジェクトの更新、作成できる場合は作成する
        /// </summary>
        private void Update()
        {
            foreach (var key in _rectTransformHighlightObjectKeys)
            {
                //nullじゃ無い場合はオブジェクトの更新処理
                if (_rectTransformHighlightObjects[key] != null)
                {
                    _rectTransformHighlightObjects[key].UpdateObject();
                    continue;
                }
                
                CreateAndSetTransformHighlightObject(key);
            }
            
        }

        /// <summary>
        /// そのオブジェクトに対するハイライトの作成を試みる
        /// 作成できた場合は<see cref="_rectTransformHighlightObjects"/>に格納する
        /// 作成できない場合はnullを格納する
        /// </summary>
        /// <param name="highlightType"></param>
        private void CreateAndSetTransformHighlightObject(HighlightType highlightType)
        {
            //nullの場合はオブジェクトの作成を試みる
            var readonlyData = _rectTransformGetActions[highlightType]();
            if (readonlyData != null)
            {
                _rectTransformHighlightObjects[highlightType] = rectTransformHighlight.CreateHighlightObject(readonlyData);
            }
            else
            {
                _rectTransformHighlightObjects[highlightType] = null;
            }
        }
    }
    
    public enum HighlightType
    {
        CraftItemPutButton,
        CraftResultSlot,
    }
}