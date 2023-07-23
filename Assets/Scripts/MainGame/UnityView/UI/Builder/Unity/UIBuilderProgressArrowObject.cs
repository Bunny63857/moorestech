﻿using MainGame.UnityView.UI.Builder.Element;
using UnityEngine;
using UnityEngine.UI;

namespace MainGame.UnityView.UI.Builder.Unity
{
    public class UIBuilderProgressArrowObject : MonoBehaviour,IUIBuilderObject
    {
        [SerializeField] private Image arrowBlack;
        
        public IUIBluePrintElement BluePrintElement { get; private set; }
        public void Initialize(IUIBluePrintElement bluePrintElement)
        {
            BluePrintElement = bluePrintElement;
        }

        public void SetFillAmount(float amount)
        {
            arrowBlack.fillAmount = amount;
        }
    }
}