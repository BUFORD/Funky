﻿using System;
using System.Linq;
using Zeta;
using Zeta.Internals.Actors;
using Zeta.Internals;
using Zeta.Common;
using System.Collections.Generic;
using Zeta.Internals.SNO;
using Zeta.CommonBot;
using FunkyTrinity.ability;
using FunkyTrinity.Cache;
using FunkyTrinity.Movement;

namespace FunkyTrinity
{

	 public static partial class Bot
	 {
		  ///<summary>
		  ///Cache of current combat variables
		  ///</summary>
		  public class CombatCache
		  {

				public CombatCache()
				{
					 bWholeNewTarget=false;
					 bPickNewAbilities=false;
					 bWaitingAfterPower=false;
					 bWaitingForPower=false;
					 bWaitingForPotion=false;
					 bForceTargetUpdate=false;
					 bWasRootedLastTick=false;

					 ShouldCheckItemLooted=false;
					 reCheckedFinished=false;
					 recheckCount=0;


					 LastHealthDropPct=0d;
					 LastHealthChange=DateTime.Today;
					 powerPrime=new Ability();
					 iElitesWithinRange=new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
					 iAnythingWithinRange=new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
					 iNonRendedTargets_6=0;
					 bAnyBossesInRange=false;
					 bAnyChampionsPresent=false;
					 bAnyTreasureGoblinsPresent=false;
					 bAnyMobsInCloseRange=false;
					 bAnyNonWWIgnoreMobsInRange=false;
					 RequiresAvoidance=false;
					 TravellingAvoidance=false;
					 bForceCloseRangeTarget=false;
					 lastHadUnitInSights=DateTime.Today;
					 lastHadEliteUnitInSights=DateTime.Today;
					 lastHadContainerAsTarget=DateTime.Today;
					 lastHadRareChestAsTarget=DateTime.Today;
					 dateSincePickedTarget=DateTime.Today;
					 LastAvoidanceMovement=DateTime.Today;
					 SurroundingUnits=0;
					 DontMove=false;
					 CriticalAvoidance=false;
					 FleeingLastTarget=false;
					 AvoidanceLastTarget=false;
					 UsesDOTDPSAbility=false;
				}

				internal CacheObject LastCachedTarget { get; set; }
				internal List<int> PrioritizedRAGUIDs=new List<int>();
				internal List<CacheServerObject> NearbyObstacleObjects=new List<CacheServerObject>();
				internal List<int> NearbyAvoidances=new List<int>();
				internal List<CacheUnit> FleeTriggeringUnits=new List<CacheUnit>();
				internal List<CacheAvoidance> TriggeringAvoidances=new List<CacheAvoidance>();
				internal List<int> UnitRAGUIDs=new List<int>();
				internal List<int> ValidClusterUnits=new List<int>();
				internal List<CacheUnit> DistantUnits=new List<CacheUnit>();



				internal List<Cluster> CurrentTargetClusters=new List<Cluster>();
				internal List<Cluster> CurrentGroupClusters=new List<Cluster>();

				///<summary>
				///Tracks Lists of Clusters for specific Conditions used during Ability Clustering.
				///</summary>
				internal Dictionary<ClusterConditions, List<Cluster>> AbilityClusters=new Dictionary<ClusterConditions, List<Cluster>>();

				internal DateTime LastClusterTargetLogicRefresh=DateTime.MinValue;
				internal DateTime LastClusterGroupingLogicRefresh=DateTime.MinValue;

				//This is used during Ability Selection -- we reuse the targeting cluster data if possible!
				internal List<Cluster> RunKMeans(ClusterConditions CC)
				{
					 //Get unit objects only!
					 List<CacheUnit> listObjectUnits=Bot.ValidObjects.OfType<CacheUnit>().Where(u => 
						  UnitRAGUIDs.Contains(u.RAGUID)
						  &&u.CentreDistance<=CC.MaximumDistance
						  &&(!CC.IgnoreNonTargetable||u.IsTargetable.HasValue&&u.IsTargetable.Value)).ToList();
					   

					 //Logging.WriteVerbose("Total Units {0}", listObjectUnits.Count.ToString());
					 if (listObjectUnits.Count>0)
					 {
						  return Cluster.RunKmeans(listObjectUnits, CC.ClusterDistance).Where(c => c.ListUnits.Count>=CC.MinimumUnits&&(CC.DOTDPSRatio==0.00d||c.DotDPSRatio<=CC.DOTDPSRatio)).ToList();
					 }
					 else
						  return new List<Cluster>();
				}


				//Cache last filtered list generated
				internal List<Cluster> LastClusterList=new List<Cluster>();
				private DateTime lastClusterComputed=DateTime.Today;
				internal List<Cluster> Clusters(ClusterConditions CC)
				{
					 //only run Kmeans every 200ms or when cache is empty!
					 if (DateTime.Now.Subtract(lastClusterComputed).TotalMilliseconds>200||!AbilityClusters.ContainsKey(CC))
					 {
						  lastClusterComputed=DateTime.Now;
						  List<Cluster> newClusterList=RunKMeans(CC);

						  //Sort by distance -- reverse to get nearest unit First
						  if (newClusterList.Count>0)
						  {
								newClusterList=newClusterList.OrderBy(o => o.NearestMonsterDistance).ToList();
								newClusterList.First().ListUnits.Sort();
								newClusterList.First().ListUnits.Reverse();
						  }

						  //Add or update dictionary entry.
						  if (!AbilityClusters.ContainsKey(CC))
						  {
								Logging.WriteVerbose("Creating new entry for ClusterConditions -- {0}", CC.ToString());
								AbilityClusters.Add(CC, newClusterList);
						  }
						  else
								AbilityClusters[CC]=newClusterList;
					 }

					 //Logging.WriteVerbose("Ability Clusters Found {0}", AbilityClusters[CC].Count.ToString());
					 return AbilityClusters[CC];
				}


				internal void UpdateGroupClusteringVariables()
				{
					 //grouping clustering
					 if (Bot.SettingsFunky.AttemptGroupingMovements)
					 {
						  if ((CurrentGroupClusters.Count==0&&DateTime.Compare(LastClusterGroupingLogicRefresh, DateTime.Now)<0)
								||(CurrentGroupClusters.Count>0&&DateTime.Now.Subtract(LastClusterGroupingLogicRefresh).TotalMilliseconds>1250))
						  {
								//Clear Clusters and Unit collection
								CurrentGroupClusters=new List<Cluster>();

								//Check if there are enough units present currently..
								if (DistantUnits.Count>Bot.SettingsFunky.GroupingMinimumUnitsInCluster)
								{

									 //Update Cluster Collection
									 CurrentGroupClusters=Cluster.RunKmeans(DistantUnits, Bot.SettingsFunky.GroupingClusterRadiusDistance)
										  .Where(cluster => cluster.ListUnits.Count>=Bot.SettingsFunky.GroupingMinimumUnitsInCluster&&cluster.NearestMonsterDistance<=Bot.SettingsFunky.GroupingMaximumDistanceAllowed)
										  .OrderByDescending(cluster => cluster.NearestMonsterDistance).ToList();

									 if (Bot.SettingsFunky.LogGroupingOutput)
										  Logging.WriteVerbose("Updated Group Clusters. Count={0}", CurrentGroupClusters.Count.ToString());

									 LastClusterGroupingLogicRefresh=DateTime.Now;
								}

						  }
					 }
				}

				///<summary>
				///The main cluster refresh -- creates general clustering to be used in more specific clustering checks.
				///</summary>
				internal void UpdateTargetClusteringVariables()
				{

					 //normal clustering
					 if ((Bot.SettingsFunky.EnableClusteringTargetLogic
						  &&(!Bot.SettingsFunky.IgnoreClusteringWhenLowHP||Bot.Character.dCurrentHealthPct>Bot.SettingsFunky.IgnoreClusterLowHPValue)
						  &&(DateTime.Now.Subtract(dateSincePickedTarget).TotalMilliseconds>500||DateTime.Now.Subtract(LastClusterTargetLogicRefresh).TotalMilliseconds>200)))
					 {
						  //Clear Clusters and Unit collection
						  CurrentTargetClusters=new List<Cluster>();
						  ValidClusterUnits=new List<int>();

						  //Check if there are enough units present currently..
						  if (UnitRAGUIDs.Count>=Bot.SettingsFunky.ClusterMinimumUnitCount)
						  {
								//Get unit objects only!
								List<CacheObject> listObjectUnits=Bot.ValidObjects.Where(u => UnitRAGUIDs.Contains(u.RAGUID)).ToList();

								//Make sure there are units before we continue!
								if (listObjectUnits.Count>0)
								{
									 //Update Cluster Collection
									 CurrentTargetClusters=Cluster.RunKmeans(listObjectUnits, Bot.SettingsFunky.ClusterDistance);
									 LastClusterTargetLogicRefresh=DateTime.Now;

									 //Add each RAGUID to collection only if clusters contained units meets minimum setting
									 foreach (var item in CurrentTargetClusters)
									 {
										  if (item.ListUnits.Count>=Bot.SettingsFunky.ClusterMinimumUnitCount)
												ValidClusterUnits.AddRange(item.RAGUIDS);
									 }
								}
						  }
					 }



				}


				internal DateTime LastHealthChange { get; set; }
				internal double LastHealthDropPct { get; set; }

				#region Kite & Avoid

				///<summary>
				///Tracks if kiting was used last loop.
				///</summary>
				internal bool FleeingLastTarget { get; set; }
				internal bool AvoidanceLastTarget { get; set; }
				//Kiting
				internal bool IsFleeing { get; set; }
				// Prevent spam-kiting too much - allow fighting between each kite movement
				internal DateTime timeCancelledFleeMove=DateTime.Today;
				internal int iMillisecondsCancelledFleeMoveFor=0;
				//Duration: Seconds of the kite movement
				internal int iSecondsFleeMoveFor=0;

				internal DateTime LastFleeAction=DateTime.Today;
				//Avoidance Related
				internal bool RequiresAvoidance { get; set; }
				internal bool TravellingAvoidance { get; set; }
				internal bool DontMove { get; set; }
				internal bool CriticalAvoidance { get; set; }
				internal DateTime LastAvoidanceMovement { get; set; }

				// This force-prevents avoidance for XX loops incase we get stuck trying to avoid stuff
				internal DateTime timeCancelledEmergencyMove=DateTime.Today;
				internal int iMillisecondsCancelledEmergencyMoveFor=0;
				//Duration: Seconds of the avoid movement
				internal int iSecondsEmergencyMoveFor=0;

				// This lets us know if there is a target but it's in avoidance so we can just "stay put" until avoidance goes
				internal bool bStayPutDuringAvoidance=false;


				#endregion


				// Variables used to actually hold powers the power-selector has picked to use, for buffing and main power use
				internal Ability powerBuff { get; set; }
				internal Ability powerPrime;
				internal SNOPower powerLastSnoPowerUsed { get; set; }

				//Loot Check
				internal bool ShouldCheckItemLooted { get; set; }
				internal int recheckCount { get; set; }
				internal bool reCheckedFinished { get; set; }


				#region HandlerFlags

				// A flag to indicate whether we have a new target from the overlord (decorator) or not, in which case don't refresh targets again this first loop
				internal bool bWholeNewTarget { get; set; }
				// A flag to indicate if we should pick a new power/Ability to use or not
				internal bool bPickNewAbilities { get; set; }
				// Flag used to indicate if we are simply waiting for a power to go off - so don't do any new target checking or anything
				internal bool bWaitingForPower { get; set; }
				// And a special post-use pause
				internal bool bWaitingAfterPower { get; set; }
				// If we are waiting before popping a potion
				internal bool bWaitingForPotion { get; set; }
				// Force a target update after certain interactions
				internal bool bForceTargetUpdate { get; set; }
				// Variable to let us force new target creations immediately after a root
				internal bool bWasRootedLastTick { get; set; }
				// This holds whether or not we want to prioritize a close-target, used when we might be body-blocked by monsters
				internal bool bForceCloseRangeTarget { get; set; }
				#endregion

				// how long to force close-range targets for
				internal int iMillisecondsForceCloseRange=0;
				// Date time we were last told to stick to close range targets
				internal DateTime lastForcedKeepCloseRange=DateTime.Today;


				// Variables relating to quick-reference of monsters within sepcific ranges (if anyone has suggestion for similar functionality with reduced CPU use, lemme know, but this is fast atm!)
				internal int[] iElitesWithinRange;
				internal int[] iAnythingWithinRange;

				internal int iNonRendedTargets_6 { get; set; }
				internal bool UsesDOTDPSAbility { get; set; }
				internal int SurroundingUnits { get; set; }


				#region Last seen objects
				// Last had any mob in range, for loot-waiting
				internal DateTime lastHadUnitInSights { get; set; }
				// When we last saw a boss/elite etc.
				internal DateTime lastHadEliteUnitInSights { get; set; }
				//Last time we had a container, for loot-waiting
				internal DateTime lastHadContainerAsTarget { get; set; }
				//When we last saw a "rare" chest
				internal DateTime lastHadRareChestAsTarget { get; set; }
				// Store the date-time when we *FIRST* picked this target, so we can blacklist after X period of time targeting
				internal DateTime dateSincePickedTarget { get; set; }
				internal int iTotalNumberGoblins=0;
				internal DateTime lastGoblinTime=DateTime.Today;
				#endregion


				internal int iACDGUIDLastWhirlwind=0;
				internal Vector3 vSideToSideTarget=Vector3.Zero;
				internal DateTime lastChangedZigZag=DateTime.Today;
				internal Vector3 vPositionLastZigZagCheck=Vector3.Zero;

				internal bool bAnyChampionsPresent { get; set; }
				internal bool bAnyTreasureGoblinsPresent { get; set; }
				internal bool bAnyMobsInCloseRange { get; set; }
				internal bool bAnyBossesInRange { get; set; }
				// A flag to say whether any NONE-hashActorSNOWhirlwindIgnore things are around
				internal bool bAnyNonWWIgnoreMobsInRange { get; set; }
				/// <summary>
				/// Check LoS if waller avoidance detected
				/// </summary>
				internal bool bCheckGround=false;


				internal string DebugString
				{
					 get
					 {
						  string strDebug_LAST=string.Format("LastPickedTarget {0} -- LastHealthChanged {1} // HealthDrop {2}\r\n"+
																		  "lastHadUnitInSights {3} -- lastHadEliteUnitInSights {4} -- lastHadContainerAsTarget {5} -- lastHadRareChestAsTarget {6}\r\n"+
																		  "LastAvoidanceMovement {7}",
																		  this.dateSincePickedTarget.ToString(), this.LastHealthChange.ToString(), this.LastHealthDropPct.ToString(),
																		  this.lastHadUnitInSights.ToString(), this.lastHadEliteUnitInSights.ToString(), this.lastHadContainerAsTarget.ToString(), this.lastHadRareChestAsTarget.ToString(), this.LastAvoidanceMovement.ToString());

						  string strDebug_ClusterTarget=string.Format("LastClusterTargetLogicRefresh {0}\r\n"+
																					 "CurrentTargetClusters Count {1} -- ValidClusterUnits Count {2}",
																					 this.LastClusterTargetLogicRefresh.ToString(), this.CurrentTargetClusters.Count, this.ValidClusterUnits.Count);

						  string strDebug_ClusterGroup=string.Format("LastClusterGroupingLogicRefresh {0}\r\n"+
																					 "CurrentGroupClusters Count {1} -- DistantUnits Count {2}",
																					 this.LastClusterGroupingLogicRefresh.ToString(), this.CurrentGroupClusters.Count, this.DistantUnits.Count);

						  string strDebug_ClusterAbility=string.Format("Total Ability Cluster Entries {0}", this.AbilityClusters.Keys.ToString());



						  return String.Format("Combat Cache\r\n {0} \r\n {1} \r\n {2} \r\n {3}", strDebug_LAST, strDebug_ClusterTarget, strDebug_ClusterGroup, strDebug_ClusterAbility);
					 }
				}






				internal void Reset()
				{
					 iElitesWithinRange=new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
					 iAnythingWithinRange=new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
					 iNonRendedTargets_6=0;
					 bAnyBossesInRange=false;
					 bAnyChampionsPresent=false;
					 bAnyTreasureGoblinsPresent=false;
					 bAnyMobsInCloseRange=false;
					 bAnyNonWWIgnoreMobsInRange=false;
					 TravellingAvoidance=false;
					 UnitRAGUIDs=new List<int>();
					 SurroundingUnits=0;
					 TriggeringAvoidances.Clear();
					 IsFleeing=false;
					 UsesDOTDPSAbility=false;
					 bCheckGround=false;
					 NearbyAvoidances.Clear();
					 NearbyObstacleObjects.Clear();
					 FleeTriggeringUnits.Clear();
					 DistantUnits.Clear();
				}
				internal void ResetTargetHandling()
				{
					 Bot.Target.CurrentTarget=null;
					 //Bot.NavigationCache.ResetPathing();
					 FunkyTrinity.Movement.TargetMovement.ResetTargetMovementVars();


					 bWaitingForPower=false;
					 bWaitingAfterPower=false;
					 bWaitingForPotion=false;
					 bWasRootedLastTick=false;
					 recheckCount=0;
					 reCheckedFinished=false;

					 LastHealthChange=DateTime.MinValue;
					 LastHealthDropPct=0d;

				}
		  }

	 }

}