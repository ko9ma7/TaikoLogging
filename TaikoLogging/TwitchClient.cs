﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using TwitchLib;
using TwitchLib.Client;
using TwitchLib.Api.V5;
using TwitchLib.Client.Models;
using System.Drawing;
using System.IO;

namespace TaikoLogging
{
    class RinClient
    {
        readonly ConnectionCredentials credentials = new ConnectionCredentials(TwitchInfo.BotUsername, TwitchInfo.BotToken);
        TwitchClient client;
        GoogleSheetInterface sheet;

        bool newSongIncoming = false;
        Bitmap newSongBitmap;



        // Minor note to myself
        // "Unaccounted for [UserState]: badge-info" keeps popping up in the console window
        // This is not my error, it's an error with TwitchLib, which has been fixed but not released yet

        public RinClient()
        {
            sheet = new GoogleSheetInterface();
            Connect();
        }

        ~RinClient()
        {
            Disconnect();
        }

        internal void Connect()
        {
            client = new TwitchClient();
            client.Initialize(credentials, TwitchInfo.ChannelName);

            client.OnMessageReceived += Client_OnMessageReceived;
            client.OnConnectionError += Client_OnConnectionError;
            client.Connect();
        }

        private void Client_OnConnectionError(object sender, TwitchLib.Client.Events.OnConnectionErrorArgs e)
        {
            //Console.WriteLine($"Error!! {e.Error}");

            // That's probably good enough
            Program.logger.LogVariable("TwitchClient Connection Error", "Error", e.Error);
        }

        private void Client_OnMessageReceived(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            #region oldCommands
            //if(CheckDBCommands("!notranked", e) || CheckDBCommands("!removeranked", e) || CheckDBCommands("!undoranked", e))
            //{
            //    sheet.RemoveLastRanked();
            //    SendTwitchMessage("Last ranked match removed");
            //}
            //else if (CheckDBCommands("!song ", e) && newSongIncoming)
            //{
            //    string songTitle = e.ChatMessage.Message.Remove(0, 6);
            //    DirectoryInfo dirInfo = new DirectoryInfo(@"D:\My Stuff\My Programs\Taiko\TaikoLogging\TaikoLogging\Data\Title Bitmaps\BaseTitles\");
            //    var result = dirInfo.GetFiles();
            //    int numScreenshots = 0;
            //    for (int i = 0; i < result.Length; i++)
            //    {
            //        if (result[i].Name.Remove(result[i].Name.IndexOf('.')) == songTitle)
            //        {
            //            numScreenshots++;
            //        }
            //    }


            //    newSongBitmap.Save(@"D:\My Stuff\My Programs\Taiko\TaikoLogging\TaikoLogging\Data\Title Bitmaps\BaseTitles\" + songTitle + ".png");
            //    newSongIncoming = false;
            //    newSongBitmap = null;
            //    SendTwitchMessage(songTitle + " has been added!");
            //    Program.analysis.NewSongAdded();
            //}
            //else if (CheckDBCommands("!random mode", e))
            //{
            //    Program.analysis.RandomModeToggle();
            //}
            //else if (CheckDBCommands("!random", e))
            //{
            //    sheet.GetRandomSong();
            //}
            #endregion

            if (e.ChatMessage.IsBroadcaster == true)
            {
                Program.commands.CheckCommands(e.ChatMessage.Message);
            }
        }

        internal void Disconnect()
        {

        }

        public void SendTwitchMessage(string message)
        {
            if (Program.twitchOn == true)
            {
                client.SendMessage(TwitchInfo.ChannelName, message);
                Console.WriteLine("Twitch message sent: " + message);
            }
            else
            {
                Console.WriteLine("Twitch offline: " + message);
            }
        }

        private bool CheckDBCommands(string command, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            return e.ChatMessage.Message.StartsWith(command, StringComparison.InvariantCultureIgnoreCase) && string.Compare(e.ChatMessage.Username, "Deathblood", true) == 0;
        }

        public void PrepareNewSong(Bitmap bmp)
        {
            newSongIncoming = true;
            newSongBitmap = new Bitmap(bmp);
            SendTwitchMessage("Couldn't figure out the song, !song <song>");
        }
    }
}
