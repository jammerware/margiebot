using System.Text.RegularExpressions;
using MargieBot.Models;
using MargieBot.Responders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace MargieBot.Tests
{
    [TestClass]
    public class ResponderTests
    {
        private TestContext testContextInstance;
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
                MentionsBot = Regex.IsMatch(messageText, @"\bmargie\b", RegexOptions.IgnoreCase),
                RawData = string.Empty,
                Text = messageText,
                User = Substitute.For<SlackUser>()
            };

            return context;
        }
        #endregion

        [TestMethod]
        public void SimpleResponderCanRespond()
        {
            Bot bot = new Bot();
            IResponder responder = bot.RespondsTo("Hi").With("Hello, friend!").GetResponder();
            ResponseContext context = GetResponseContext("Hi everybody.");

            Assert.AreEqual(true, responder.CanRespond(context));
        }

        [TestMethod]
        public void SimpleResponderRespondsCorrectly()
        {
            Bot bot = new Bot();
            IResponder responder = bot.RespondsTo("Hi").With("Hello, friend!").GetResponder();
            ResponseContext context = GetResponseContext("Hi, everybody.");

            Assert.AreEqual("Hello, friend!", responder.GetResponse(context).Text);
        }

        [TestMethod]
        public void SimpleResponderWithMentionRespondsToMention()
        {
            Bot bot = new Bot();
            IResponder responder = bot.RespondsTo("Hi").With("Hello, friend!").IfBotIsMentioned().GetResponder();
            ResponseContext context = GetResponseContext("Hi, Margie.");

            Assert.AreEqual(true, responder.CanRespond(context));
        }

        [TestMethod]
        public void SimpleResponderWithMentionDoesntRespondWithoutMention()
        {
            Bot bot = new Bot();
            IResponder responder = bot.RespondsTo("Hi").With("Hello, friend!").IfBotIsMentioned().GetResponder();
            ResponseContext context = GetResponseContext("Hi, everybody.");

            Assert.AreEqual(false, responder.CanRespond(context));
        }
    }
}