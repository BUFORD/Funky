﻿using System;
using System.Linq;
using FunkyTrinity.AbilityFunky;
using FunkyTrinity.Cache;
using FunkyTrinity.Cache.Enums;
using FunkyTrinity.Movement;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace FunkyTrinity.Targeting.Behaviors
{
	public class TBUpdateTarget:TargetBehavior
	{

		 public TBUpdateTarget() : base() { }


		 public override TargetBehavioralTypes TargetBehavioralTypeType
		 {
			  get
			  {
					return TargetBehavioralTypes.Target;
			  }
		 }
		 public override void Initialize()
		 {
			  base.Test=(ref Cache.CacheObject obj) =>
			  {
					FunkyTrinity.Bot.Combat.bStayPutDuringAvoidance=false;

					//cluster update
					FunkyTrinity.Bot.Combat.UpdateTargetClusteringVariables();

					//Standard weighting of valid objects -- updates current target.
					this.WeightEvaluationObjList(ref obj);


					//Final Possible Target Check
					if (obj==null)
					{
						 // No valid targets but we were told to stay put?
						 if (FunkyTrinity.Bot.Combat.bStayPutDuringAvoidance)
						 {
							  if (FunkyTrinity.Bot.Combat.TriggeringAvoidances.Count==0)
							  {
									obj=new CacheObject(FunkyTrinity.Bot.Character.Position, TargetType.Avoidance, 20000, "StayPutPoint", 2.5f, -1);
									return true;
							  }
							  else
									FunkyTrinity.Bot.Combat.iMillisecondsCancelledEmergencyMoveFor=0; //reset wait time
						 }

						 // See if we should wait for milliseconds for possible loot drops before continuing run
						 if (DateTime.Now.Subtract(FunkyTrinity.Bot.Targeting.lastHadUnitInSights).TotalMilliseconds<=FunkyTrinity.Bot.Settings.AfterCombatDelay&&DateTime.Now.Subtract(Bot.Targeting.lastHadEliteUnitInSights).TotalMilliseconds<=10000||
							  //Cut the delay time in half for non-elite monsters!
							 DateTime.Now.Subtract(FunkyTrinity.Bot.Targeting.lastHadUnitInSights).TotalMilliseconds<=FunkyTrinity.Bot.Settings.AfterCombatDelay)
						 {
							  obj=new CacheObject(FunkyTrinity.Bot.Character.Position, TargetType.Avoidance, 20000, "WaitForLootDrops", 2f, -1);
							  return true;

						 }
						 //Herbfunks wait after loot containers are opened. 3s for rare chests, half the settings delay for everything else.
						 if ((DateTime.Now.Subtract(FunkyTrinity.Bot.Targeting.lastHadRareChestAsTarget).TotalMilliseconds<=3750)||
							 (DateTime.Now.Subtract(FunkyTrinity.Bot.Targeting.lastHadContainerAsTarget).TotalMilliseconds<=(FunkyTrinity.Bot.Settings.AfterCombatDelay*1.25)))
						 {
							  obj=new CacheObject(FunkyTrinity.Bot.Character.Position, TargetType.Avoidance, 20000, "ContainerLootDropsWait", 2f, -1);
							  return true;
						 }

						 // Finally, a special check for waiting for wrath of the berserker cooldown before engaging Azmodan
						 if (FunkyTrinity.Bot.Class.HotbarPowers.Contains(SNOPower.Barbarian_WrathOfTheBerserker)&&FunkyTrinity.Bot.Settings.Class.bWaitForWrath&&!FunkyTrinity.Bot.Class.Abilities[SNOPower.Barbarian_WrathOfTheBerserker].AbilityUseTimer()&&
							 ZetaDia.CurrentWorldId==121214&&
							 (Vector3.Distance(FunkyTrinity.Bot.Character.Position, new Vector3(711.25f, 716.25f, 80.13903f))<=40f||Vector3.Distance(FunkyTrinity.Bot.Character.Position, new Vector3(546.8467f, 551.7733f, 1.576313f))<=40f))
						 {
							  Logging.Write("[Funky] Waiting for Wrath Of The Berserker cooldown before continuing to Azmodan.");
							  obj=new CacheObject(FunkyTrinity.Bot.Character.Position, TargetType.Avoidance, 20000, "GilesWaitForWrath", 0f, -1);
							  InactivityDetector.Reset();
							  return true;
						 }
						 // And a special check for wizard archon
						 if (FunkyTrinity.Bot.Class.HotbarPowers.Contains(SNOPower.Wizard_Archon)&&!Bot.Class.Abilities[SNOPower.Wizard_Archon].AbilityUseTimer()&&FunkyTrinity.Bot.Settings.Class.bWaitForArchon&&ZetaDia.CurrentWorldId==121214&&
							 (Vector3.Distance(FunkyTrinity.Bot.Character.Position, new Vector3(711.25f, 716.25f, 80.13903f))<=40f||Vector3.Distance(FunkyTrinity.Bot.Character.Position, new Vector3(546.8467f, 551.7733f, 1.576313f))<=40f))
						 {
							  Logging.Write("[Funky] Waiting for Wizard Archon cooldown before continuing to Azmodan.");
							  obj=new CacheObject(FunkyTrinity.Bot.Character.Position, TargetType.Avoidance, 20000, "GilesWaitForArchon", 0f, -1);
							  InactivityDetector.Reset();
							  return true;
						 }
						 // And a very sexy special check for WD BigBadVoodoo
						 if (FunkyTrinity.Bot.Class.HotbarPowers.Contains(SNOPower.Witchdoctor_BigBadVoodoo)&&!PowerManager.CanCast(SNOPower.Witchdoctor_BigBadVoodoo)&&ZetaDia.CurrentWorldId==121214&&
							 (Vector3.Distance(FunkyTrinity.Bot.Character.Position, new Vector3(711.25f, 716.25f, 80.13903f))<=40f||Vector3.Distance(FunkyTrinity.Bot.Character.Position, new Vector3(546.8467f, 551.7733f, 1.576313f))<=40f))
						 {
							  Logging.Write("[Funky] Waiting for WD BigBadVoodoo cooldown before continuing to Azmodan.");
							  obj=new CacheObject(FunkyTrinity.Bot.Character.Position, TargetType.Avoidance, 20000, "GilesWaitForVoodooo", 0f, -1);
							  InactivityDetector.Reset();
							  return true;
						 }


						 //Check if our current path intersects avoidances. (When not in town, and not currently inside avoidance)
						 if (!Bot.Character.bIsInTown&&(Bot.Settings.Avoidance.AttemptAvoidanceMovements||Bot.Combat.CriticalAvoidance)
								 &&Navigation.NP.CurrentPath.Count>0
								 &&Bot.Combat.TriggeringAvoidances.Count==0)
						 {
							  Vector3 curpos=FunkyTrinity.Bot.Character.Position;
							  IndexedList<Vector3> curpath=Navigation.NP.CurrentPath;

							  var CurrentNearbyPath=curpath.Where(v => curpos.Distance(v)<=40f);
							  if (CurrentNearbyPath!=null&&CurrentNearbyPath.Any())
							  {
									CurrentNearbyPath.OrderBy(v => curpath.IndexOf(v));

									Vector3 lastV3=Vector3.Zero;
									foreach (var item in CurrentNearbyPath)
									{
										 if (lastV3==Vector3.Zero)
											  lastV3=curpos;

										 if (ObjectCache.Obstacles.TestVectorAgainstAvoidanceZones(item, lastV3))
										 {
											  obj=new CacheObject(Bot.Character.Position, TargetType.Avoidance, 20000, "AvoidanceIntersection", 2.5f, -1);
											  return true;
										 }

										 lastV3=item;
									}
							  }
						 }
					}

					return false;
			  };
		 }

		 ///<summary>
		 ///Iterates through Usable objects and sets the Bot.CurrentTarget to the highest weighted object found inside the given list.
		 ///</summary>
		 private void WeightEvaluationObjList(ref CacheObject CurrentTarget)
		 {
			  // Store if we are ignoring all units this cycle or not
			  bool bIgnoreAllUnits=!FunkyTrinity.Bot.Combat.bAnyChampionsPresent
										  &&!FunkyTrinity.Bot.Combat.bAnyMobsInCloseRange
										  &&((!FunkyTrinity.Bot.Combat.bAnyTreasureGoblinsPresent&&FunkyTrinity.Bot.Settings.Targeting.GoblinPriority>=2)||FunkyTrinity.Bot.Settings.Targeting.GoblinPriority<2)
										  &&FunkyTrinity.Bot.Character.dCurrentHealthPct>=0.85d;


			  //clear our last "avoid" list..
			  ObjectCache.Objects.objectsIgnoredDueToAvoidance.Clear();

			  double iHighestWeightFound=0;

			  foreach (CacheObject thisobj in ObjectCache.ValidObjects)
			  {
					thisobj.UpdateWeight();

					if (thisobj.Weight==1)
					{
						 // Force the character to stay where it is if there is nothing available that is out of avoidance stuff and we aren't already in avoidance stuff
						 thisobj.Weight=0;
						 if (!FunkyTrinity.Bot.Combat.RequiresAvoidance) 
							  FunkyTrinity.Bot.Combat.bStayPutDuringAvoidance=true;

						 continue;
					}

					// Is the weight of this one higher than the current-highest weight? Then make this the new primary target!
					if (thisobj.Weight>iHighestWeightFound&&thisobj.Weight>0)
					{
						 //Check combat looting (Demonbuddy Setting)
						 if (iHighestWeightFound>0
											  &&thisobj.targetType.Value==TargetType.Item
											  &&!Zeta.CommonBot.Settings.CharacterSettings.Instance.CombatLooting
											  &&CurrentTarget.targetType.Value==TargetType.Unit) continue;


						 //cache RAGUID so we can switch back if we need to
						 int CurrentTargetRAGUID=CurrentTarget!=null?CurrentTarget.RAGUID:-1;

						 //Set our current target to this object!
						 CurrentTarget=ObjectCache.Objects[thisobj.RAGUID];

						 if (CurrentTarget.LOSV3!=Vector3.Zero)
						 {
							  if (CurrentTarget.LOSV3.Distance(Bot.Character.Position)<=2.5f)
									CurrentTarget.LOSV3=Vector3.Zero;
							  else
							  {
									iHighestWeightFound=thisobj.Weight;
									CurrentTarget=new CacheObject(CurrentTarget.LOSV3, TargetType.Avoidance, 20000d, "LOSV3", 2.5f);
									continue;
							  }
						 }
							 

						 bool resetTarget=false;
						 //Check for Range Classes and Unit Targets
						 if (!FunkyTrinity.Bot.Class.IsMeleeClass&&CurrentTarget.targetType.Value==TargetType.Unit&&FunkyTrinity.Bot.Combat.NearbyAvoidances.Count>0)
						 {
							  //set unit target (for Ability selector).
							  Bot.Targeting.CurrentUnitTarget=(CacheUnit)CurrentTarget;

							  //Generate next Ability..
							  Ability nextAbility=FunkyTrinity.Bot.Class.AbilitySelector(Bot.Targeting.CurrentUnitTarget, true);

							  //reset unit target
							  Bot.Targeting.CurrentUnitTarget=null;

							  if (nextAbility==Bot.Class.DefaultAttack&&!Bot.Class.CanUseDefaultAttack)
							  {
									if (thisobj.ObjectIsSpecial)
										 ObjectCache.Objects.objectsIgnoredDueToAvoidance.Add(thisobj);
									else
										 resetTarget=true;
							  }
									

							  //if (!thisobj.WithinInteractionRange())
							  //{
							  //	 if (nextAbility.IsRanged)
							  //	 {
							  //		  Vector3 destinationV3=nextAbility.DestinationVector;
							  //		  //Check if the estimated destination will also be inside avoidance zone..
							  //		  if (ObjectCache.Obstacles.IsPositionWithinAvoidanceArea(destinationV3)
							  //			  ||ObjectCache.Obstacles.TestVectorAgainstAvoidanceZones(destinationV3))
							  //		  {
							  //				//Only wait if the object is special and we are not avoiding..
							  //				if (thisobj.ObjectIsSpecial)
							  //				{
							  //					 if (!FunkyTrinity.Bot.Combat.RequiresAvoidance)
							  //					 {
							  //						  FunkyTrinity.Bot.Combat.bStayPutDuringAvoidance=true;
							  //						  resetTarget=true;
							  //					 }
							  //					 else if (!nextAbility.IsRanged&&nextAbility.Range>0)
							  //					 {
							  //						  //Non-Ranged Ability.. act like melee..
							  //						  //Try to find a spot
							  //						  ObjectCache.Objects.objectsIgnoredDueToAvoidance.Add(thisobj);
							  //					 }
							  //				}
							  //				else
							  //					 resetTarget=true;
							  //		  }

							  //	 }
							  //	 else
							  //	 {
							  //		  Vector3 TestPosition=thisobj.BotMeleeVector;
							  //		  if (ObjectCache.Obstacles.IsPositionWithinAvoidanceArea(TestPosition))
							  //				resetTarget=true;
							  //		  else if (ObjectCache.Obstacles.TestVectorAgainstAvoidanceZones(TestPosition)) //intersecting avoidances..
							  //		  {
							  //				if (thisobj.ObjectIsSpecial)
							  //				{//Only add this to the avoided list when its not currently inside avoidance area
							  //					 ObjectCache.Objects.objectsIgnoredDueToAvoidance.Add(thisobj);
							  //				}
							  //				else
							  //					 resetTarget=true;
							  //		  }
							  //	 }
							  //}
						 }

						 //Avoidance Attempt to find a location where we can attack!
						 if (ObjectCache.Objects.objectsIgnoredDueToAvoidance.Contains(thisobj))
						 {
							  //Wait if no valid target found yet.. and no avoidance movement required.
							  if (!FunkyTrinity.Bot.Combat.RequiresAvoidance)
									FunkyTrinity.Bot.Combat.bStayPutDuringAvoidance=true;

							  //Check Bot Navigationally blocked
							  FunkyTrinity.Bot.NavigationCache.RefreshNavigationBlocked();
							  if (!FunkyTrinity.Bot.NavigationCache.BotIsNavigationallyBlocked)
							  {
									Vector3 SafeLOSMovement;
									if (thisobj.GPRect.TryFindSafeSpot(FunkyTrinity.Bot.Character.Position, out SafeLOSMovement, thisobj.Position, FunkyTrinity.Bot.Character.ShouldFlee, true))
									{
										 CurrentTarget=new CacheObject(SafeLOSMovement, TargetType.Avoidance, 20000, "SafetyMovement", 2.5f, -1);
										 //Reset Avoidance Timer so we don't trigger it while moving towards the target!
										 FunkyTrinity.Bot.Combat.timeCancelledEmergencyMove=DateTime.Now;
										 FunkyTrinity.Bot.Combat.iMillisecondsCancelledEmergencyMoveFor=1000+((int)(FunkyTrinity.Bot.Targeting.CurrentTarget.CentreDistance/25f)*1000);
									}
									else
									{
										 resetTarget=true;
									}
							  }
						 }

						 if (resetTarget)
						 {
							  CurrentTarget=CurrentTargetRAGUID!=-1?ObjectCache.Objects[CurrentTargetRAGUID]:null;
							  continue;
						 }

						 iHighestWeightFound=thisobj.Weight;
					}

			  } // Loop through all the objects and give them a weight
		 }
	}
}
