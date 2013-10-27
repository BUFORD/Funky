﻿using System;
using Zeta;
using System.Linq;
using Zeta.Common;
using System.Collections.Generic;
using Zeta.Internals.Actors;
using System.Threading;
using Zeta.CommonBot;
using FunkyBot.XMLTags;

namespace FunkyBot
{
	 public partial class Funky
	 {
		  private static bool DumpedDeathInfo=false;

		  private void FunkyOnDeath(object src, EventArgs mea)
		  {
				if(!DumpedDeathInfo)
				{
					 if (Bot.Settings.Debug.FunkyLogFlags.HasFlag(LogLevel.OutOfCombat))
						  Logger.Write(LogLevel.User, "Death Info Dump \r\n ProfileBehavior {0} \r\n Last Target Behavior {1} \r\n"+
									  "{2} \r\n"+
									  "Triggering Avoidances={3} -- RequiredAvoidance={4} -- LastAvoidAction={5} \r\n"+
									  "Nearby Flee Triggering Units={6} -- IsFleeing={7} -- LastFleeAction={8} \r\n",
									  Bot.Profile.CurrentProfileBehavior.GetType().ToString(), Bot.Targeting.lastBehavioralType.ToString(),
									  Cache.ObjectCache.Objects.DumpDebugInfo(),
									  Bot.Combat.TriggeringAvoidances.Count, Bot.Targeting.RequiresAvoidance, Bot.Targeting.LastAvoidanceMovement.ToString(),
									  Bot.Combat.FleeTriggeringUnits.Count, Bot.Combat.IsFleeing, Bot.Targeting.LastFleeAction.ToString());

					 DumpedDeathInfo=true;
				}

                //if (DateTime.Now.Subtract(Bot.Stats.lastDied).TotalSeconds>10)
                //{
					 
                     //Bot.Stats.lastDied=DateTime.Now;
                     //Bot.Stats.iTotalDeaths++;
                     //Bot.Stats.iDeathsThisRun++;
					 //Bot.BotStatistics.GameStats.TotalDeaths++;

					 ResetBot();

					 
					 // Does Trinity need to handle deaths?
                     if (TrinityMaxDeathsTag.MaxDeathsAllowed > 0)
					 {
                         if (ProfileTracking.TotalStats.CurrentTrackingProfile.DeathCount > TrinityMaxDeathsTag.MaxDeathsAllowed)
						  {
								Logging.Write("[Funky] You have died too many times. Now restarting the game.");

                                ProfileTracking.TrackedProfile FirstProfile = ProfileTracking.TotalStats.ProfilesTracked.OrderBy(p => p.DateStartedProfile).First();

                                string sUseProfile = FirstProfile.ProfileName;
								ProfileManager.Load(!string.IsNullOrEmpty(sUseProfile)
																?sUseProfile
																:Zeta.CommonBot.Settings.GlobalSettings.Instance.LastProfile);
								Thread.Sleep(1000);
								ResetGame();
								ZetaDia.Service.Party.LeaveGame();
								Thread.Sleep(10000);
						  }
						  else
						  {
								Logging.Write("[Funky] I'm sorry, but I seem to have let you die :( Now restarting the current profile.");
								ProfileManager.Load(Zeta.CommonBot.Settings.GlobalSettings.Instance.LastProfile);
								Thread.Sleep(3500);
						  }
					 }

                //}
		  }
	 }
}