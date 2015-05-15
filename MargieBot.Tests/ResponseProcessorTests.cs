using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MargieBot.Models;
using NSubstitute;
using System.Text.RegularExpressions;

namespace MargieBot.Tests
{
    [TestClass]
    public class ResponseProcessorTests
    {
        public ResponseProcessorTests()
        {
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Utility methods
        private ResponseContext GetResponseContext(string messageText)
        {
            var context = Substitute.For<ResponseContext>();
            context.Message = new SlackMessage() {
                ChatHub = Substitute.For<SlackChatHub>(),
                MentionsBot = Regex.IsMatch(messageText, @"\bmargie\b"),
                RawData = string.Empty,
                Text = messageText,
                User = Substitute.For<SlackUser>()
            };

            return context;
        }
        #endregion

        [TestMethod]
        public void TestMethod1()
        {
            //
            // TODO: Add test logic here
            //
        }
    }
}
