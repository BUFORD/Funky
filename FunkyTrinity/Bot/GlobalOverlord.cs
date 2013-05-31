﻿using System;
using Zeta;
using System.Linq;
using Zeta.Common;
using Zeta.Internals.Actors;
using System.Collections.Generic;
using Zeta.CommonBot;
using Zeta.TreeSharp;

namespace FunkyTrinity
{
	 public partial class Funky
	 {
		  // Total main loops so we can update things every XX loops
		  private static int iCombatLoops=0;
		  private static bool GlobalOverlord(object ret)
		  {
				// If we aren't in the game of a world is loading, don't do anything yet
				if (!ZetaDia.IsInGame||ZetaDia.IsLoadingWorld)
				{
					 Bot.Combat.lastChangedZigZag=DateTime.Today;
					 Bot.Combat.vPositionLastZigZagCheck=Vector3.Zero;
					 return false;
				}

				// World ID safety caching incase it's ever unavailable
				if (ZetaDia.CurrentWorldDynamicId!=-1)
					 Bot.Character.iCurrentWorldID=ZetaDia.CurrentWorldDynamicId;

				//Check if we need to resfresh class data!
				if (SettingsFunky.UseLevelingLogic)
				{
					 double lastlevelupMS=DateTime.Now.Subtract(LastLevelUp).TotalSeconds;
					 if (lastlevelupMS<30&&lastlevelupMS>15)
					 {
						  Bot.Class=null;
						  LastLevelUp=DateTime.MinValue;
					 }
				}

				// Store all of the player's abilities every now and then, to keep it cached and handy, also check for critical-mass timer changes etc.
				iCombatLoops++;
				if (Bot.Class==null||iCombatLoops>=50)
				{
					 // Update the cached player's cache
					 ActorClass tempClass=ActorClass.Invalid;
					 try
					 {
						  tempClass=ZetaDia.Actors.Me.ActorClass;
					 } catch (NullReferenceException)
					 {
						  Logging.WriteDiagnostic("[Funky] Safely handled exception trying to get character class.");
					 }
					 if (tempClass!=ActorClass.Invalid&&Bot.Class==null)
						  Bot.Class=new Bot.CharacterInfo(tempClass);


					 iCombatLoops=0;

					 //Set Character Radius?
					 if (Bot.Character.fCharacterRadius==0f)
						  Bot.Character.fCharacterRadius=ZetaDia.Me.ActorInfo.Sphere.Radius;

					 // Game difficulty, used really for vault on DH's
					 if (ZetaDia.Service.CurrentHero.CurrentDifficulty!=GameDifficulty.Invalid)
						  Bot.Character.iCurrentGameDifficulty=ZetaDia.Service.CurrentHero.CurrentDifficulty;
				}

				// Recording of all the XML's in use this run
				#region NewProfileCheck
				if (DateTime.Now.Subtract(lastProfileCheck).TotalMilliseconds>1000)
				{
					 lastProfileCheck=DateTime.Now;
					 string sThisProfile=Zeta.CommonBot.Settings.GlobalSettings.Instance.LastProfile;
					 if (sThisProfile!=Funky.sLastProfileSeen)
					 {
						  //herbfunk stats
						  Statistics.ProfileStats.UpdateProfileChanged();

						  // See if we appear to have started a new game
						  if (!String.IsNullOrEmpty(Funky.sFirstProfileSeen)&&sThisProfile==Funky.sFirstProfileSeen)
						  {
								Bot.iTotalProfileRecycles++;
								if (Bot.iTotalProfileRecycles>Bot.iTotalJoinGames&&Bot.iTotalProfileRecycles>Bot.iTotalLeaveGames)
								{
									 Log("Reseting Game Data -- Total Profile Recycles exceedes join and leave count!");
									 Funky.ResetGame();
								}
						  }
						  Funky.listProfilesLoaded.Add(sThisProfile);
						  Funky.sLastProfileSeen=sThisProfile;
						  if (String.IsNullOrEmpty(Funky.sFirstProfileSeen))
								Funky.sFirstProfileSeen=sThisProfile;
					 }
				}
				#endregion

				Bot.Class.SecondaryHotbarBuffPresent();

				// Clear target current and reset key variables used during the target-handling function
				Bot.Combat.ResetTargetHandling();
				Bot.Combat.DontMove=false;

				// Should we refresh target list?
				if (dbRefresh.ShouldRefreshObjectList)
				{
					 dbRefresh.RefreshDiaObjects();

					 // We have a target, start the target handler!
					 if (Bot.Target.CurrentTarget!=null)
					 {
						  Bot.Combat.bWholeNewTarget=true;
						  Bot.Combat.DontMove=true;
						  Bot.Combat.bPickNewAbilities=true;
						  return true;
					 }
				}
				else
				{
					 //Check OOC ID Behavior..
					 if (SettingsFunky.OOCIdentifyItems&&ShouldRunIDBehavior())
					 {
						  Logging.WriteDiagnostic("[Funky] Starting OOC ID Behavior");
						  Bot.Combat.DontMove=true;
						  return true;
					 }
					 else if (MuleBehavior)
					 {
						  if (BotMain.StatusText.Contains("Game Finished"))
						  {
								if (ZetaDia.Actors.GetActorsOfType<Zeta.Internals.Actors.Gizmos.GizmoPlayerSharedStash>(true, true).Any())
								{
									 //Zeta.CommonBot.BotMain.CurrentBot.Logic.Stop(null);
									 return true;
								}
						  }
					 }
					 // Only do something when pulsing if it's been at least 5 seconds since last pulse, to prevent spam
					 else if (SettingsFunky.UseLevelingLogic&&Bot.Character.iMyLevel<60&&DateTime.Now.Subtract(_lastLooked).TotalSeconds>5)
					 {
						  // Every 5 minutes, re-check all equipped items and clear stored blacklist
						  if (DateTime.Now.Subtract(_lastFullEvaluation).TotalSeconds>300)
						  {
								bNeedFullItemUpdate=true;
						  }
						  // Now check the backpack
						  CheckBackpack();
					 }
					 // Return false here means we only do all of the below OOC stuff at max once every 150ms
					 return false;
				}
				// Pop a potion when necessary
				if (Bot.Character.dCurrentHealthPct<=Bot.Class.EmergencyHealthPotionLimit)
				{
					 if (!Bot.Character.bIsIncapacitated&&AbilityUseTimer(SNOPower.DrinkHealthPotion))
					 {
						  Bot.AttemptToUseHealthPotion();
					 }
				}
				// Clear the temporary blacklist every 90 seconds
				if (DateTime.Now.Subtract(dateSinceTemporaryBlacklistClear).TotalSeconds>90)
				{
					 dateSinceTemporaryBlacklistClear=DateTime.Now;
					 hashRGUIDTemporaryIgnoreBlacklist=new HashSet<int>();
				}
				if (SettingsFunky.DebugStatusBar&&bResetStatusText)
				{
					 bResetStatusText=false;
					 BotMain.StatusText="[Funky] No more targets - DemonBuddy/profile management is now in control";
				}

				// Nothing to do... do we have some maintenance we can do instead, like out of combat buffing?
				Bot.Combat.lastChangedZigZag=DateTime.Today;
				Bot.Combat.vPositionLastZigZagCheck=Vector3.Zero;

				// Out of combat buffing etc. but only if we don't want to return to town etc.
				AnimationState myAnimationState=Bot.Character.CurrentAnimationState;
				if (!Bot.Character.bIsInTown&&!TownRunManager.bWantToTownRun&&myAnimationState!=AnimationState.Attacking&&myAnimationState!=AnimationState.Casting&&myAnimationState!=AnimationState.Channeling)
				{
					 Bot.Combat.powerBuff=GilesAbilitySelector(false, true, false);
					 if (Bot.Combat.powerBuff.Power!=SNOPower.None)
					 {
						  WaitWhileAnimating(4, true);
						  ZetaDia.Me.UsePower(Bot.Combat.powerBuff.Power, Bot.Combat.powerBuff.TargetPosition, Bot.Combat.powerBuff.WorldID, Bot.Combat.powerBuff.TargetRaGuid);
						  Bot.Combat.powerLastSnoPowerUsed=Bot.Combat.powerBuff.Power;
						  dictAbilityLastUse[Bot.Combat.powerBuff.Power]=DateTime.Now;
						  WaitWhileAnimating(3, true);
					 }
				}

				//Override Townportal Tag Behavior (After it starts..)
				if (Bot.Character.CurrentProfileBehavior==null||Bot.Character.CurrentProfileBehavior.Behavior.Guid!=Zeta.CommonBot.ProfileManager.CurrentProfileBehavior.Behavior.Guid)
				{
					 Bot.Character.CurrentProfileBehavior=Zeta.CommonBot.ProfileManager.CurrentProfileBehavior;

					 if (Bot.Character.CurrentProfileBehavior.GetType()==typeof(Zeta.CommonBot.Profile.Common.UseTownPortalTag))
					 {
						  Logging.WriteVerbose("Current Profile Behavior is TownPortal Tag");
						  Bot.Character.IsRunningTownPortalBehavior=true;
						  return true;
					 }
					 else
						  Bot.Character.IsRunningTownPortalBehavior=false;
				}


				// Ok let DemonBuddy do stuff this loop, since we're done for the moment
				return false;
		  }

		  //Used when we actually want to handle a target!
		  public static RunStatus HandleTarget(object ret)
		  {
				if (shouldPreformOOCItemIDing)
					 return HandleIDBehavior(); //Check if we are doing OOC ID behavior..
				else if (Bot.Target.CurrentTarget!=null)
					 return Bot.Target.HandleThis();  //Default Behavior: Current Target
				else if (MuleBehavior)
				{
					 if (!TransferedGear)
					 {
						  return NewMuleGame.StashTransfer();
					 }
					 else if (!Finished)
					 {
						  return NewMuleGame.FinishMuleBehavior();
					 }
				}

				return RunStatus.Success;
		  }
	 }
}