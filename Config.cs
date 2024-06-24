using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;
using Terraria.ModLoader;
using Terraria;

namespace DiscordInteractsMod
{
    class ChatConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [TooltipArgs("Should the bot be able to send messages in Discord?")] public bool sendMessagesInDiscord = true;
        [TooltipArgs("Should the bot be able to send messages in-game?")] public bool sendMessagesInIngameChat = true;
        [TooltipArgs("Enter the API key for your bot here!")] public string apiKey;

        public override void OnLoaded()
        {
            DiscordInteractsMod.ChatConfig = this;
            base.OnLoaded();
        }

    }
}
