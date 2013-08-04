﻿using System;
using System.Linq;
using Zeta;
using Zeta.TreeSharp;
using Zeta.Common;
using Zeta.Internals.SNO;
using Zeta.Internals.Actors;
using FunkyTrinity.Enums;
using FunkyTrinity.ability;
using FunkyTrinity.Cache;

namespace FunkyTrinity.Movement
{

		  public static class TargetMovement
		  {
				//TargetMovement -- Used during Target Handling to move the bot into Interaction Range.

				internal static Vector3 LastPlayerLocation=Vector3.Zero;
				internal static int BlockedMovementCounter=0;
				internal static int NonMovementCounter=0;

				internal static DateTime LastMovementDuringCombat=DateTime.Today;
				internal static DateTime LastMovementAttempted=DateTime.Today;
				internal static DateTime LastMovementCommand=DateTime.Today;

				internal static bool IsAlreadyMoving=false;
				internal static float LastDistanceFromTarget=-1f;

				internal static Vector3 LastTargetLocation=Vector3.Zero;
				internal static Vector3 CurrentTargetLocation=Vector3.Zero;

				//internal bool UsedAutoMovementCommand=false;

				internal static RunStatus TargetMoveTo(CacheObject obj)
				{

					 #region DebugInfo
					 if (Bot.SettingsFunky.DebugStatusBar)
					 {
						  string Action="[Move-";
						  switch (obj.targetType.Value)
						  {
								case TargetType.Avoidance:
									 Action+="Avoid] ";
									 break;
								case TargetType.Unit:
									 if (obj.LOSV3!=Vector3.Zero)
										  Action+="LOS] ";
									 else if (Bot.NavigationCache.groupRunningBehavior&&Bot.NavigationCache.groupingCurrentUnit!=null&&Bot.NavigationCache.groupingCurrentUnit==obj)
										  Action+="Grouping] ";
									 else
										  Action+="Combat] ";

									 break;
								case TargetType.Item:
								case TargetType.Gold:
								case TargetType.Globe:
									 Action+="Pickup] ";
									 break;
								case TargetType.Interactable:
									 Action+="Interact] ";
									 break;
								case TargetType.Container:
									 Action+="Open] ";
									 break;
								case TargetType.Destructible:
								case TargetType.Barricade:
									 Action+="Destroy] ";
									 break;
								case TargetType.Shrine:
									 Action+="Click] ";
									 break;
						  }
						  Bot.Target.UpdateStatusText(Action);
					 }
					 #endregion

					 // Are we currently incapacitated? If so then wait...
					 if (Bot.Character.bIsIncapacitated||Bot.Character.bIsRooted)
						  return RunStatus.Running;

					 if (Bot.SettingsFunky.SkipAhead)
						  SkipAheadCache.RecordSkipAheadCachePoint();

					 // Some stuff to avoid spamming usepower EVERY loop, and also to detect stucks/staying in one place for too long
					 bool bForceNewMovement=false;

					 //Herbfunk: Added this to prevent stucks attempting to move to a target blocked. (Case: 3 champs behind a wall, within range but could not engage due to being on the other side.)
					 if (NonMovementCounter>50)
					 {
						  Logging.WriteVerbose("{0}: Ignoring obj {1} due to no movement counter reaching {2}", "[Funky]", obj.InternalName+" _ SNO:"+obj.SNOID, NonMovementCounter);
						  obj.BlacklistLoops=50;
						  Bot.Combat.bForceTargetUpdate=true;
						  NonMovementCounter=0;

						  // Reset the emergency loop counter and return success
						  return RunStatus.Running;
					 }

					 Bot.NavigationCache.ObstaclePrioritizeCheck();

					 #region DistanceChecks
					 // Count how long we have failed to move - body block stuff etc.
					 if (obj.DistanceFromTarget==LastDistanceFromTarget)
					 {
						  bForceNewMovement=true;
						  if (DateTime.Now.Subtract(LastMovementDuringCombat).TotalMilliseconds>=250)
						  {
								LastMovementDuringCombat=DateTime.Now;
								// We've been stuck at least 250 ms, let's go and pick new targets etc.
								BlockedMovementCounter++;
								Bot.Combat.bForceCloseRangeTarget=true;
								Bot.Combat.lastForcedKeepCloseRange=DateTime.Now;



								// Tell target finder to prioritize close-combat targets incase we were bodyblocked
								#region TargetingPriortize
								switch (BlockedMovementCounter)
								{
									 case 2:
									 case 3:
										  //Than check our movement state
										  //If we are moving to a LOS location.. nullify it!
										  if (obj.LOSV3!=Vector3.Zero)
										  {
												Logging.WriteVerbose("Blockcounter Reseting LOS Movement Vector");
												obj.LOSV3=Vector3.Zero;
										  }



										  var intersectingObstacles=Bot.Combat.NearbyObstacleObjects //ObjectCache.Obstacles.Values.OfType<CacheServerObject>()
																					.Where(obstacle =>
																						 !Bot.Combat.PrioritizedRAGUIDs.Contains(obstacle.RAGUID)//Only objects not already prioritized
																						 &&obstacle.Obstacletype.HasValue
																						 &&ObstacleType.Navigation.HasFlag(obstacle.Obstacletype.Value)//only navigation/intersection blocking objects!
																						 &&obstacle.RadiusDistance<=10f);

										  if (intersectingObstacles.Any())
										  {
												var intersectingObjectRAGUIDs=(from objs in intersectingObstacles
																						 select objs.RAGUID);

												Bot.Combat.PrioritizedRAGUIDs.AddRange(intersectingObjectRAGUIDs);
										  }

										  if (Bot.NavigationCache.groupRunningBehavior)
										  {
												Logging.WriteVerbose("Grouping Behavior stopped due to blocking counter");
												Bot.NavigationCache.GroupingFinishBehavior();
												Bot.Combat.bForceTargetUpdate=true;
												return RunStatus.Running;
										  }

										  if (obj.targetType.Value==TargetType.Avoidance)
										  {//Avoidance Movement..
												Bot.Combat.timeCancelledFleeMove=DateTime.Now;
												Bot.Combat.timeCancelledEmergencyMove=DateTime.Now;
												Bot.NavigationCache.BlacklistLastSafespot();
												Bot.UpdateAvoidKiteRates();
												Bot.Combat.bForceTargetUpdate=true;
												return RunStatus.Running;
										  }



										  break;
									 default:
										  if (obj.targetType.Value!=TargetType.Avoidance)
										  {
												//Finally try raycasting to see if navigation is possible..
												if (obj.Actortype.HasValue&&
													 (obj.Actortype.Value==ActorType.Gizmo||obj.Actortype.Value==ActorType.Unit))
												{
													 Vector3 hitTest;
													 // No raycast available, try and force-ignore this for a little while, and blacklist for a few seconds
													 if (Zeta.Navigation.Navigator.Raycast(Bot.Character.Position, obj.Position, out hitTest))
													 {
														  if (hitTest!=Vector3.Zero)
														  {
																obj.RequiresLOSCheck=true;
																obj.BlacklistLoops=10;
																Logging.WriteDiagnostic("Ignoring object "+obj.InternalName+" due to not moving and raycast failure!", true);
																Bot.Combat.bForceTargetUpdate=true;
																return RunStatus.Running;
														  }
													 }
												}
												else if (obj.Actortype.HasValue&&obj.Actortype.Value.HasFlag(ActorType.Item))
												{
													 Bot.Combat.timeCancelledEmergencyMove=DateTime.Now;
													 Bot.Combat.timeCancelledFleeMove=DateTime.Now;

													 //Check if we can walk to this location from current location..
													 if (!Navigation.CanRayCast(Bot.Character.Position, CurrentTargetLocation, NavCellFlags.AllowWalk))
													 {
														  obj.BlacklistLoops=10;
															Logging.WriteDiagnostic("Ignoring Item due to AllowWalk RayCast Failure!", true);
														  Bot.Combat.bForceTargetUpdate=true;
														  return RunStatus.Running;
													 }
												}
										  }
										  else
										  {
												if (!Navigation.CanRayCast(Bot.Character.Position, CurrentTargetLocation, NavCellFlags.AllowWalk))
												{
													 Logging.WriteVerbose("Cannot continue with avoidance movement due to raycast failure!");
													 BlockedMovementCounter=0;

													 Bot.Combat.iMillisecondsCancelledEmergencyMoveFor/=2;
													 Bot.Combat.timeCancelledEmergencyMove=DateTime.Now;
													 //Ignore avoidance movements.
													 Bot.Combat.iMillisecondsCancelledFleeMoveFor/=2;
													 Bot.Combat.timeCancelledFleeMove=DateTime.Now;

													 Bot.NavigationCache.BlacklistLastSafespot();
													 Bot.Combat.bForceTargetUpdate=true;
													 return RunStatus.Running;
												}
										  }
										  break;
								}
								#endregion


								if (obj.targetType.Value==TargetType.Item)
								{
									 obj.BlacklistLoops=1;
									 Bot.Combat.bForceTargetUpdate=true;
								}

								return RunStatus.Running;
						  } // Been 250 milliseconds of non-movement?
					 }
					 else
					 {
						  // Movement has been made, so count the time last moved!
						  LastMovementDuringCombat=DateTime.Now;
					 }
					 #endregion

					 // Update the last distance stored
					 LastDistanceFromTarget=obj.DistanceFromTarget;

					 // See if we want to ACTUALLY move, or are just waiting for the last move command...
					 if (!bForceNewMovement&&IsAlreadyMoving&&CurrentTargetLocation==LastTargetLocation&&DateTime.Now.Subtract(LastMovementCommand).TotalMilliseconds<=100)
					 {
						  return RunStatus.Running;
					 }

					 float currentDistance=Vector3.Distance(LastTargetLocation, CurrentTargetLocation);

					 // If we're doing avoidance, globes or backtracking, try to use special abilities to move quicker
					 #region SpecialMovementChecks
					 if ((TargetType.Avoidance|TargetType.Gold|TargetType.Globe).HasFlag(obj.targetType.Value))
					 {

						  bool bTooMuchZChange=((Bot.Character.Position.Z-CurrentTargetLocation.Z)>=4f);

						  Ability MovementPower;
						  if (Bot.Class.FindMovementPower(out MovementPower))
						  {
								double lastUsedAbilityMS=MovementPower.LastUsedMilliseconds;
								bool foundMovementPower=false;
								float pointDistance=0f;
								Vector3 vTargetAimPoint=CurrentTargetLocation;
								bool ignoreLOSTest=false;

								switch (MovementPower.Power)
								{
									 case SNOPower.Monk_TempestRush:
										  vTargetAimPoint=MathEx.CalculatePointFrom(CurrentTargetLocation, Bot.Character.Position, 10f);
										  Bot.Character.UpdateAnimationState(false, true);
										  bool isHobbling=Bot.Character.CurrentSNOAnim.HasFlag(SNOAnim.Monk_Female_Hobble_Run|SNOAnim.Monk_Male_HTH_Hobble_Run);
										  foundMovementPower=(!bTooMuchZChange&&!Bot.Class.bWaitingForSpecial&&currentDistance<15f&&((isHobbling||lastUsedAbilityMS<300)&&Bot.Character.dCurrentEnergy>15)
												&&!ObjectCache.Obstacles.DoesPositionIntersectAny(vTargetAimPoint, ObstacleType.ServerObject));

										  break;
									 case SNOPower.DemonHunter_Vault:
										  foundMovementPower=(obj.targetType.Value!=TargetType.Destructible&&!bTooMuchZChange&&currentDistance>=18f&&
																	 (lastUsedAbilityMS>=Bot.SettingsFunky.Class.iDHVaultMovementDelay));
										  pointDistance=35f;
										  if (currentDistance>pointDistance)
												vTargetAimPoint=MathEx.CalculatePointFrom(CurrentTargetLocation, Bot.Character.Position, pointDistance);
										  break;
									 case SNOPower.Barbarian_FuriousCharge:
										  foundMovementPower=(obj.targetType.Value!=TargetType.Destructible&&!bTooMuchZChange
																	 &&(currentDistance>20f||obj.targetType.Value.HasFlag(TargetType.Avoidance)&&(NonMovementCounter>0||BlockedMovementCounter>0)));

										  pointDistance=35f;
										  if (currentDistance>pointDistance)
												vTargetAimPoint=MathEx.CalculatePointFrom(CurrentTargetLocation, Bot.Character.Position, pointDistance);

										  break;
									 case SNOPower.Barbarian_Leap:
									 case SNOPower.Wizard_Archon_Teleport:
									 case SNOPower.Wizard_Teleport:
										  foundMovementPower=(obj.targetType.Value!=TargetType.Destructible&&!bTooMuchZChange
																	 &&(currentDistance>20f||obj.targetType.Value.HasFlag(TargetType.Avoidance)&&(NonMovementCounter>0||BlockedMovementCounter>0)));

										  pointDistance=35f;
										  if (currentDistance>pointDistance)
												vTargetAimPoint=MathEx.CalculatePointFrom(CurrentTargetLocation, Bot.Character.Position, pointDistance);

										  //Teleport and Leap ignores raycast testing below!
										  ignoreLOSTest=true;
										  break;
									 case SNOPower.Barbarian_Whirlwind:
										  break;
									 default:
										  Bot.Character.WaitWhileAnimating(3, true);

										  ZetaDia.Me.UsePower(MovementPower.Power, CurrentTargetLocation, Bot.Character.iCurrentWorldID, -1);
										  MovementPower.LastUsed=DateTime.Now;

										  Bot.Character.WaitWhileAnimating(6, true);
										  // Store the current destination for comparison incase of changes next loop
										  LastTargetLocation=CurrentTargetLocation;
										  // Reset total body-block count, since we should have moved
										  if (DateTime.Now.Subtract(Bot.Combat.lastForcedKeepCloseRange).TotalMilliseconds>=2000)
												BlockedMovementCounter=0;
										  return RunStatus.Running;
								}

								if (foundMovementPower)
								{

									 if ((MovementPower.Power==SNOPower.Monk_TempestRush&&lastUsedAbilityMS>500)||
										  (ignoreLOSTest||ZetaDia.Physics.Raycast(Bot.Character.Position, vTargetAimPoint, NavCellFlags.AllowWalk)))
									 {
										  ZetaDia.Me.UsePower(MovementPower.Power, vTargetAimPoint, Bot.Character.iCurrentWorldID, -1);
										  MovementPower.LastUsed=DateTime.Now;

										  // Store the current destination for comparison incase of changes next loop
										  LastTargetLocation=CurrentTargetLocation;

										  // Reset total body-block count, since we should have moved
										  if (DateTime.Now.Subtract(Bot.Combat.lastForcedKeepCloseRange).TotalMilliseconds>=2000)
												BlockedMovementCounter=0;
										  return RunStatus.Running;
									 }
								}
						  }

						  //Special Whirlwind Code
						  if (Bot.Class.AC==Zeta.Internals.Actors.ActorClass.Barbarian&&Bot.Class.HotbarPowers.Contains(SNOPower.Barbarian_Whirlwind))
						  {
								// Whirlwind against everything within range (except backtrack points)
								if (Bot.Character.dCurrentEnergy>=10
									 &&Bot.Combat.iAnythingWithinRange[(int)RangeIntervals.Range_20]>=1
									 &&obj.DistanceFromTarget<=12f
									 &&(!Bot.Class.HotbarPowers.Contains(SNOPower.Barbarian_Sprint)||Bot.Class.HasBuff(SNOPower.Barbarian_Sprint))
									 &&(!(TargetType.Avoidance|TargetType.Gold|TargetType.Globe).HasFlag(obj.targetType.Value)&obj.DistanceFromTarget>=6f)
									 &&(obj.targetType.Value!=TargetType.Unit
									 ||(obj.targetType.Value==TargetType.Unit&&!obj.IsTreasureGoblin
										  &&(!Bot.SettingsFunky.Class.bSelectiveWhirlwind
												||Bot.Combat.bAnyNonWWIgnoreMobsInRange
												||!CacheIDLookup.hashActorSNOWhirlwindIgnore.Contains(obj.SNOID)))))
								{
									 // Special code to prevent whirlwind double-spam, this helps save fury
									 bool bUseThisLoop=SNOPower.Barbarian_Whirlwind!=Bot.Combat.powerLastSnoPowerUsed;
									 if (!bUseThisLoop)
									 {
										  Bot.Combat.powerLastSnoPowerUsed=SNOPower.None;
											if (DateTime.Now.Subtract(PowerCacheLookup.dictAbilityLastUse[SNOPower.Barbarian_Whirlwind]).TotalMilliseconds>=200)
												bUseThisLoop=true;
									 }
									 if (bUseThisLoop)
									 {
										  ZetaDia.Me.UsePower(SNOPower.Barbarian_Whirlwind, CurrentTargetLocation, Bot.Character.iCurrentWorldID, -1);
										  Bot.Combat.powerLastSnoPowerUsed=SNOPower.Barbarian_Whirlwind;
											PowerCacheLookup.dictAbilityLastUse[SNOPower.Barbarian_Whirlwind]=DateTime.Now;
									 }
									 // Store the current destination for comparison incase of changes next loop
									 LastTargetLocation=CurrentTargetLocation;
									 // Reset total body-block count
									 if ((!Bot.Combat.bForceCloseRangeTarget||DateTime.Now.Subtract(Bot.Combat.lastForcedKeepCloseRange).TotalMilliseconds>Bot.Combat.iMillisecondsForceCloseRange)&&
										 DateTime.Now.Subtract(Bot.Combat.lastForcedKeepCloseRange).TotalMilliseconds>=2000)
										  BlockedMovementCounter=0;
									 return RunStatus.Running;
								}
						  }
					 }
					 #endregion


					 // Now for the actual movement request stuff
					 IsAlreadyMoving=true;
					 LastMovementCommand=DateTime.Now;


					 if (DateTime.Now.Subtract(LastMovementAttempted).TotalMilliseconds>=250||currentDistance>=2f||bForceNewMovement)
					 {
						  if (obj.targetType.Value.Equals(TargetType.Avoidance))
						  {
								if (NonMovementCounter<10)
									 ZetaDia.Me.Movement.MoveActor(CurrentTargetLocation);
								else
									 ZetaDia.Me.UsePower(SNOPower.Walk, CurrentTargetLocation, Bot.Character.iCurrentWorldID, -1);
						  }
						  else
						  {
								//Use Walk Power when not using LOS Movement, target is not an item and target does not ignore LOS.
								bool UsePower=(NonMovementCounter<10&&!obj.UsingLOSV3&&obj.targetType.Value!=TargetType.Item&&!obj.IgnoresLOSCheck);
								if (UsePower)
								{
									 ZetaDia.Me.UsePower(SNOPower.Walk, CurrentTargetLocation, Bot.Character.iCurrentWorldID, -1);
								}
								else
									 ZetaDia.Me.Movement.MoveActor(CurrentTargetLocation);
						  }

						  LastMovementAttempted=DateTime.Now;
						  // Store the current destination for comparison incase of changes next loop
						  LastTargetLocation=CurrentTargetLocation;
						  // Reset total body-block count, since we should have moved
						  if (DateTime.Now.Subtract(Bot.Combat.lastForcedKeepCloseRange).TotalMilliseconds>=2000)
								BlockedMovementCounter=0;

						  //Herbfunk: Quick fix for stuck occuring on above average mob who is damaged..
						  if (LastPlayerLocation==Bot.Character.Position)
								NonMovementCounter++;
						  else
								NonMovementCounter=0;

						  LastPlayerLocation=Bot.Character.Position;
					 }

					 return RunStatus.Running;
				}

				internal static void ResetTargetMovementVars()
				{
					 BlockedMovementCounter=0;
					 NonMovementCounter=0;
					 NewTargetResetVars();
				}
				internal static void NewTargetResetVars()
				{
					 LastDistanceFromTarget=-1f;
					 IsAlreadyMoving=false;
					 LastMovementCommand=DateTime.Today;
				}
		  }
    
}