using System;
using System.Collections.Generic;
using System.Linq;

namespace MargieBot
{
    public class ResponseContext
    {
        #region General "fixed" properties
        public bool BotHasResponded { get; set; }
        public string BotUserID { get; set; }
        public string BotUserName { get; set; }
        public SlackMessage Message { get; set; }
        public string TeamID { get; set; }
        public IReadOnlyDictionary<string, string> UserNameCache { get; set; }
        #endregion

        #region Content
        // the internal Content dictionary and its related methods allow responders to pass information to or share information with one another. Unless the
        // end programmer specifies otherwise, it won't have anything in it at all.
        private IDictionary<string, object> Content { get; set; }

        internal void Set(string key, object value)
        {
            this.Set<object>(key, value);
        }

        public T Get<T>(string key)
        {
            if (Content.ContainsKey(key)) {
                return (T)Content[key];
            }

            throw new ArgumentException("Couldn't find an entry in the ResponseContext with key \"" + key + "\".");
        }

        public void Set<T>(string key, T value)
        {
            if (Content.ContainsKey(key)) {
                Content[key] = value;
            }
            else {
                Content.Add(key, value);
            }
        }

        /// <summary>
        /// This overload is here to allow quick-getting of context content by type. If you're sure you're only going to have one object in the context
        /// of type "Floogleblatz," you can get a Floogleblatz you added to the context by calling context.Get<FloogleBlatz>();
        /// </summary>
        /// <typeparam name="T">The type of the content you're getting from the context.</typeparam>
        /// <returns>The entry in the context's content that matches the type provided</returns>
        public T Get<T>()
        {
            IList<T> matches = Content.Values.OfType<T>().ToList();
            if (matches.Count > 1) { throw new InvalidOperationException("A query for an object of type " + typeof(T).Name + " returned multiple results."); }
            else if (matches.Count == 1) { return (T)matches[0]; }
            return default(T);
        }

        /// <summary>
        /// This overload is here to allow quick-setting of context content by type. If you're sure you're only going to have one object in the context
        /// of type "Floogleblatz," you can add your Floogleblatz to the context by calling context.Set<FloogleBlatz>(myBlatz);
        /// </summary>
        /// <typeparam name="T">The type of the content you're adding to the context.</typeparam>
        /// <param name="content">The content you're adding to the context.</param>
        public void Set<T>(T content)
        {
            Content.Add(new Guid().ToString(), content);
        }
        #endregion

        public ResponseContext()
        {
            Content = new Dictionary<string, object>();
        }
    }
}