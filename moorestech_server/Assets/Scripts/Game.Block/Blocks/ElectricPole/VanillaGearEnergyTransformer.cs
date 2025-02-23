﻿using Core.EnergySystem.Gear;

namespace Game.Block.Blocks.ElectricPole
{
    public class VanillaGearEnergyTransformer : VanillaEnergyTransformerBase, IGearEnergyTransformer
    {
        public VanillaGearEnergyTransformer(int blockId, int entityId, long blockHash) : base(blockId, entityId,
            blockHash)
        {
        }
    }
}