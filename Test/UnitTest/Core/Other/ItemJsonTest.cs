﻿using Core.Config.Item;
using Core.ConfigJson;
using Core.Item.Config;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Server.StartServerSystem;
using Test.Module.TestConfig;
using Test.Module.TestMod;

namespace Test.UnitTest.Core.Other
{
    public class ItemJsonTest
    {
        [TestCase(1, 100)]
        [TestCase(2, 50)]
        [TestCase(3, 300)]
        [TestCase(4, 100)]
        [TestCase(5, 200)]
        [TestCase(7, 100)]
        public void JsonStackTest(int id, int stack)
        {
            var (_, serviceProvider) = new PacketResponseCreatorDiContainerGenerators().Create(TestModDirectory.ForUnitTestModDirectory);
            var itemConfig = serviceProvider.GetService<IItemConfig>();
            Assert.AreEqual(stack, itemConfig.GetItemConfig(id).MaxStack);
        }

        [TestCase(1, "Test1")]
        [TestCase(2, "Test2")]
        [TestCase(3, "Test3")]
        [TestCase(4, "Test4")]
        [TestCase(5, "Test5")]
        [TestCase(6, "Test6")]
        [TestCase(7, "Test6")]
        public void JsonNameTest(int id, string name)
        {
            var (_, serviceProvider) = new PacketResponseCreatorDiContainerGenerators().Create(TestModDirectory.ForUnitTestModDirectory);
            var itemConfig = serviceProvider.GetService<IItemConfig>();
            Assert.AreEqual(name, itemConfig.GetItemConfig(id).Name);
        }
    }
}