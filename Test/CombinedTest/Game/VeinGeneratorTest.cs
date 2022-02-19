using Core.Ore;
using Core.Ore.Config;
using Game.WorldMap;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.CombinedTest.Game
{
    [TestClass]
    public class VeinGeneratorTest
    {
        [TestMethod]
        public void GenerateTest()
        {
            //直接生成してテストできないので、500x500の範囲で生成して100個以上鉱石があればOKとする
            var veinGenerator = new VeinGenerator(new Seed(5000),new OreConfig());

            var count = 0;
            for (int i = 0; i < 500; i++)
            {
                for (int j = 0; j < 500; j++)
                {
                    var oreId = veinGenerator.GetOreId(i, j);
                    if (oreId != OreConst.NoneOreId)
                    {
                        count++;
                    }
                }
            }
            
            Assert.IsTrue(100 < count);
        }
        
    }
}