using System;
using MargieBot.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MargieBot.Tests
{
    [TestClass]
    public class BotNameRegexComposerTests
    {
        [TestMethod]
        [TestCategory("Bot helpers")]
        public void TestWithoutAliases()
        {
            BotNameRegexComposer composer = new BotNameRegexComposer();
            string actual = composer.ComposeFor("margiebot", "U123456", new string[] { });

            Assert.AreEqual(@"(<@U123456>|\bmargiebot\b)", actual);
        }

        [TestMethod]
        [TestCategory("Bot helpers")]
        public void TestWithAliases()
        {
            BotNameRegexComposer composer = new BotNameRegexComposer();
            string actual = composer.ComposeFor("margiebot", "U123456", new string[] { "margie", "snuffleupagus" });

            Assert.AreEqual(@"(<@U123456>|\bmargiebot\b|\bmargie\b|\bsnuffleupagus\b)", actual);
        }
    }
}