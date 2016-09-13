using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SmartyBot.Controllers
{
    [Route("api/[controller]")]
    [BotAuthentication]
    public class MessagesController : Controller
    {
        public MessagesController()
        {
            // TODO: Use IoC container
            _speller = new Dictionary();
        }

        public async Task<IActionResult> Post([FromBody]Activity activity)
        {
            try
            {
                if (activity.Type == ActivityTypes.Message)
                {
                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                    bool needPartOfSpeech = activity.Text.StartsWith("!");

                    string[] words = activity.Text.Split(' ');

                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < words.Length; ++i)
                    {
                        if (needPartOfSpeech)
                        {
                            builder.Append(words[i]);
                            string partOfSpeech = await _speller.GetPartOfSpeech(words[i]);
                            if (partOfSpeech != null)
                            {
                                builder.Append($"({partOfSpeech})");
                            }
                        }
                        else
                        {
                            string synonym = await _speller.GetSynonym(words[i]);
                            if (synonym != null)
                            {
                                builder.Append(synonym);
                            }
                            else
                            {
                                builder.Append(words[i]);
                            }
                        }

                        builder.Append(" ");
                    }

                    Activity reply = activity.CreateReply(builder.ToString());
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        private Dictionary _speller;
    }
}
