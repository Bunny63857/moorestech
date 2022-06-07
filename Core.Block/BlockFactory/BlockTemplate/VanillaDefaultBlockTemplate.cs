using Core.Block.Blocks;
using Core.Block.Config.LoadConfig;

namespace Core.Block.BlockFactory.BlockTemplate
{
    public class VanillaDefaultBlock : IBlockTemplate
    {
        public IBlock New(BlockConfigData param, int entityId,ulong blockHash)
        {
            return new VanillaBlock(param.BlockId, entityId,blockHash);
        }

        public IBlock Load(BlockConfigData param, int entityId,ulong blockHash, string state)
        {
            return new VanillaBlock(param.BlockId, entityId,blockHash);
        }
    }
}