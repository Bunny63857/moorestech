﻿using MainGame.Basic;

namespace MainGame.UnityView.Block
{
    public interface IBlockPlacePreview
    {
        public void SetDirection(BlockDirection blockDirection);
        
        public void SetActive(bool active);
    }
}