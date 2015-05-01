using MargieBot.MessageProcessors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MargieBot.Models
{
    public class MargieSimpleResponseChainer
    {
        public Bot Bot { get; set; }
        public Func<MargieContext, bool> CanRespond { get; set; }
        public bool RequireBotMention { get; set; }
        public string Response { get; set; }
        
        internal MargieSimpleResponseChainer() { }

        public MargieSimpleResponseChainer With(string response)
        {
            this.Response = response;
            this.Bot.ResponseProcessors.Add(
                MessageProcessorHelper.Create(
                    this.CanRespond,
                    (MargieContext context) => { return response; },
                    false
                )
            );

            return this;
        }
    }
}
