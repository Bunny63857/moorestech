﻿using Core.EnergySystem.Electric;
using Game.Block.Factory.BlockTemplate;

namespace Game.Block.Blocks.PowerGenerator
{
    public class VanillaElectricGenerator : VanillaPowerGeneratorBase, IElectricGenerator
    {
        public VanillaElectricGenerator(VanillaPowerGeneratorProperties data) : base(data)
        {
        }

        public VanillaElectricGenerator(VanillaPowerGeneratorProperties data, string state) : base(data, state)
        {
        }
    }
}