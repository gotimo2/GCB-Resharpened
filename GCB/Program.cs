using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.IO;
using System.Linq;

//define namespace
namespace GCB
{
	//define class
	public class Program
	{
		
		//define client
		private DiscordSocketClient Client;
		public static void Main(string[] args)
			=> new Program().MainAsync().GetAwaiter().GetResult();

		//get bad word list
		string[] words = System.IO.File.ReadAllLines("badwordlist.txt");
		string token = File.ReadAllText("Token.txt");


		//main start loop
		public async Task MainAsync()
		{
			//log.AutoFlush = true; //turns out streamwriter buffers stuff and you need to flush them to have them actually written down. who would've thunketh?
			
			//set discord client
			Client = new DiscordSocketClient();

			//login with token
			await Client.LoginAsync(TokenType.Bot, token);
			//start
			await Client.StartAsync();
			//set message received event to cusom task
			Client.MessageReceived += MessageReceived;

			//labda task for bot is connected message
			Client.Ready += () =>
			{
				Console.WriteLine("Bot is connected!");
				return Task.CompletedTask;
			};
			//keep the bot active forever
			await Task.Delay(-1);
		}

		//on message received
		private async Task MessageReceived(SocketMessage message)
		{
			//for words in bad word list
			foreach(String word in words)
            {
				//if the message contains the word
				if (message.Content.ToLower().Contains(word))
                {
					//log it to console
					Console.WriteLine($"{message.Author.Username} swore!");
					//React with :eyes:
					await message.AddReactionAsync(new Emoji("👀"));

					//get guild by name, because this is the easiest way to do it.
					var chnl = message.Channel as SocketGuildChannel;
					var Guild = chnl.Guild.Name;

					//Log it to log.txt
					StreamWriter log = new StreamWriter("Log.txt", append: true); //open streamwriter
					await log.WriteLineAsync($"{message.Author.Id} as {message.Author.Username} in {Guild} at {DateTime.Now} said a banned word in message \"{message.Content}\""); //write line
					log.Close(); //close the streamwriter
				}
            }

            //check if msg calls to swearcount
            if (message.Content.StartsWith("!swearcount"))
            {
				//if it does, get the first mentioned person
				if (message.MentionedUsers != null) {
					int occurences = 0;
					//get their id
					String targetID = message.MentionedUsers.ElementAt(0).Id.ToString();
					//read log and split it into array
					string[] lines = File.ReadAllLines("Log.txt");
					//go through the logs and check
					foreach (string line in lines)
                    {
						//count how often the specified user is in the logs
						if (line.StartsWith(targetID)){
							occurences++;
						}
                    }
					//send a message back
					await message.Channel.SendMessageAsync($"user {targetID} as {message.MentionedUsers.ElementAt(0).Username} has said bannned words {occurences} times!");
				}

                else
                {
					await message.Channel.SendMessageAsync("Invalid mention!");
                }
				

            }
		}

	}
}
