﻿using System;
using Zeta;
using Zeta.Common;
using System.Collections.Generic;
using Zeta.Internals.Actors;
using System.Threading;
using Zeta.CommonBot;
using System.IO;

namespace FunkyTrinity
{
    public partial class Funky
    {
		  
        // Each time we join & leave a game, might as well clear the hashset of looked-at dropped items - just to keep it smaller
        private static void FunkyOnJoinGame(object src, EventArgs mea)
        {
				Bot.Stats.iTotalJoinGames++;
				Bot.Stats.LastJoinedGame=DateTime.Now;

            ResetGame();
            //Start new current game stats
				Bot.Stats.Statistics.ItemStats.CurrentGame.Reset();
				Bot.Stats.Statistics.GameStats.CurrentGame.Reset();
				ResetProfileVars();

				

				//Disconnect -- Starting Profile Setup.
				if (HadDisconnectError)
				{
					 Logging.Write("Disconnect Error Last Game.. attempting to find starting profile.");
					 ReloadStartingProfile();
					 HadDisconnectError=false;
				}

        }
    }
}