﻿using System;
using System.Linq;
using System.Collections.Generic;
using FunkyTrinity.Avoidances;
using FunkyTrinity.Movement;
using Zeta;
using Zeta.Common;
using Zeta.Internals.Actors;
using Zeta.Internals;
using Zeta.Internals.SNO;
using Zeta.CommonBot;
using Zeta.Internals.Actors.Gizmos;
using System.Windows;
using System.Collections;
using System.Collections.ObjectModel;

using FunkyTrinity.Cache;
using FunkyTrinity.Cache.Enums;

namespace FunkyTrinity.Cache
{

		  ///<summary>
		  ///Contains Collections for all the cached objects being tracked.
		  ///</summary>
		  public static partial class ObjectCache
		  {
				internal static CacheObject FakeCacheObject;
				///<summary>
				///Usable Objects -- refresh inside Target.UpdateTarget
				///</summary>
				internal static List<CacheObject> ValidObjects { get; set; }

				///<summary>
				///Cached Objects.
				///</summary>
				public static ObjectCollection Objects=new ObjectCollection();

				///<summary>
				///Obstacles related to either avoidances or navigational blocks.
				///</summary>
				public static ObstacleCollection Obstacles=new ObstacleCollection();

				///<summary>
				///Cached Sno Data.
				///</summary>
				public static SnoCollection cacheSnoCollection=new SnoCollection();

				internal static bool CheckTargetTypeFlag(TargetType property, TargetType flag)
				{
					 return (property&flag)!=0;
				}

				///<summary>
				///Adds/Updates CacheObjects inside collection by Iteration of RactorList
				///This is the method that caches all live data about an object!
				///</summary>
				internal static bool UpdateCacheObjectCollection()
				{
					 if (!ZetaDia.IsInGame) return false;

					 HashSet<int> hashDoneThisRactor=new HashSet<int>();
					 foreach (Actor thisActor in ZetaDia.Actors.RActorList)
					 {
						  int tmp_raGUID;
						  DiaObject thisObj;

						  if (!thisActor.IsValid) continue;
						  //Convert to DiaObject
						  thisObj=(DiaObject)thisActor;
						  tmp_raGUID=thisObj.RActorGuid;

						  // See if we've already checked this ractor, this loop
						  if (hashDoneThisRactor.Contains(tmp_raGUID)) continue;
						  hashDoneThisRactor.Add(tmp_raGUID);

						  //Update RactorGUID and check blacklisting..
						  if (BlacklistCache.IsRAGUIDBlacklisted(tmp_raGUID)) continue;
						  CacheObject tmp_CachedObj;
						  using (ZetaDia.Memory.AcquireFrame())
						  {
								if (!ObjectCache.Objects.TryGetValue(tmp_raGUID, out tmp_CachedObj))
								{
									 Vector3 tmp_position;
									 int tmp_acdguid;
									 int tmp_SNOID;

									 #region SNO
									 //Lookup SNO
									 try
									 {
										  tmp_SNOID=thisObj.ActorSNO;
									 } catch (NullReferenceException) { Logging.WriteVerbose("Failure to get SNO from object! RaGUID: {0}", tmp_raGUID); continue; }
									 #endregion


									 //check our SNO blacklist
									 if (BlacklistCache.IsSNOIDBlacklisted(tmp_SNOID)&&!CacheIDLookup.hashSummonedPets.Contains(tmp_SNOID)) continue;


									 #region Position
									 try
									 {
										  tmp_position=thisObj.Position;
									 } catch (NullReferenceException) { Logging.WriteVerbose("Failure to get position vector for RAGUID {0}", tmp_raGUID); continue; }

									 #endregion

									 #region AcdGUID
									 try
									 {
										  tmp_acdguid=thisObj.ACDGuid;
									 } catch (NullReferenceException) { Logging.WriteVerbose("Failure to get ACDGUID for RAGUID {0}", tmp_raGUID); continue; }

									 #endregion



									 tmp_CachedObj=new CacheObject(tmp_SNOID, tmp_raGUID, tmp_acdguid, tmp_position);
								}
								else
									 //Reset unseen var
									 tmp_CachedObj.LoopsUnseen=0;


								//Validate
								try
								{
									 if (thisObj.CommonData==null||thisObj.CommonData.ACDGuid!=thisObj.ACDGuid) continue;
								} catch (NullReferenceException)
								{
									 continue;
								}



								//Check if this object is a summoned unit by a player...
								#region SummonedUnits
								if (tmp_CachedObj.IsSummonedPet)
								{
									 // Get the summoned-by info, cached if possible
									 if (!tmp_CachedObj.SummonerID.HasValue)
									 {
										  try
										  {
												tmp_CachedObj.SummonerID=thisObj.CommonData.GetAttribute<int>(ActorAttributeType.SummonedByACDID);
										  } catch (Exception ex)
										  {
												Logging.WriteVerbose("[Funky] Safely handled exception getting summoned-by info ["+tmp_CachedObj.SNOID.ToString()+"]");
												Logging.WriteDiagnostic(ex.ToString());
												continue;
										  }
									 }

									 //See if this summoned unit was summoned by the bot.
									 if (Bot.Character.iMyDynamicID==tmp_CachedObj.SummonerID.Value)
									 {
										  //Now modify the player data pets count..
										  if (Bot.Class.AC==ActorClass.Monk)
												Bot.Character.PetData.MysticAlly++;
										  else if (Bot.Class.AC==ActorClass.DemonHunter&&CacheIDLookup.hashDHPets.Contains(tmp_CachedObj.SNOID))
												Bot.Character.PetData.DemonHunterPet++;
										  else if (Bot.Class.AC==ActorClass.WitchDoctor)
										  {
												if (CacheIDLookup.hashZombie.Contains(tmp_CachedObj.SNOID))
													 Bot.Character.PetData.ZombieDogs++;
												else if (CacheIDLookup.hashGargantuan.Contains(tmp_CachedObj.SNOID))
													 Bot.Character.PetData.Gargantuan++;
										  }
										  else if (Bot.Class.AC==ActorClass.Wizard)
										  {
												//only count when range is within 45f (so we can summon a new one)
												if (CacheIDLookup.hashWizHydras.Contains(tmp_CachedObj.SNOID)&&tmp_CachedObj.CentreDistance<=45f)
													 Bot.Character.PetData.WizardHydra++;
										  }
									 }

									 //We return regardless if it was summoned by us or not since this object is not anything we want to deal with..
									 tmp_CachedObj.NeedsRemoved=true;
									 continue;
								}
								#endregion

								//Update any SNO Data.
								#region SNO_Cache_Update
								if (tmp_CachedObj.ref_DiaObject==null||tmp_CachedObj.ContainsNullValues())
								{
									 if (!tmp_CachedObj.UpdateData(thisObj, tmp_CachedObj.RAGUID))
										  continue;
								}
								else if (!tmp_CachedObj.IsFinalized)
								{//Finalize this data by recreating it and updating the Sno cache with a new finalized entry, this also clears our all Sno cache dictionaries since we no longer need them!
									 ObjectCache.cacheSnoCollection.FinalizeEntry(tmp_CachedObj.SNOID);
								}
								#endregion

								//Objects with static positions already cached don't need to be updated here.
								if (!tmp_CachedObj.NeedsUpdate) continue;

								//Obstacles -- (Not an actual object we add to targeting.)
								if (CheckTargetTypeFlag(tmp_CachedObj.targetType.Value,TargetType.Avoidance)||tmp_CachedObj.IsObstacle||tmp_CachedObj.HandleAsAvoidanceObject)
								{
									 #region Obstacles
									 bool bRequireAvoidance=false;
									 bool bTravellingAvoidance=false;

									 CacheObstacle thisObstacle;

									 //Do we have this cached?
									 if (!ObjectCache.Obstacles.TryGetValue(tmp_CachedObj.RAGUID, out thisObstacle))
									 {
										  Avoidances.AvoidanceType AvoidanceType=Avoidances.AvoidanceType.None;
										  if (tmp_CachedObj.IsAvoidance)
										  {
												AvoidanceType=AvoidanceCache.FindAvoidanceUsingSNOID(tmp_CachedObj.SNOID);
												if (AvoidanceType==Avoidances.AvoidanceType.None)
												{
													 AvoidanceType=AvoidanceCache.FindAvoidanceUsingName(tmp_CachedObj.InternalName);
													 if (AvoidanceType==AvoidanceType.None) continue;
												}
										  }

										  if (tmp_CachedObj.IsAvoidance&&tmp_CachedObj.IsProjectileAvoidance)
										  {//Ranged Projectiles require more than simple bounding points.. so we create it as avoidance zone to cache it with properties.
												//Check for intersection..
												ActorMovement thisMovement=thisObj.Movement;

												Vector2 Direction=thisMovement.DirectionVector;
												Ray R=new Ray(tmp_CachedObj.Position, Direction.ToVector3());
												double Speed;
												//Lookup Cached Speed, or add new entry.
												if (!ObjectCache.dictProjectileSpeed.TryGetValue(tmp_CachedObj.SNOID, out Speed))
												{
													 Speed=thisMovement.DesiredSpeed;
													 ObjectCache.dictProjectileSpeed.Add(tmp_CachedObj.SNOID, Speed);
												}

												thisObstacle=new CacheAvoidance(tmp_CachedObj, AvoidanceType, R, Speed);
												ObjectCache.Obstacles.Add(thisObstacle);
										  }
										  else if (tmp_CachedObj.IsAvoidance)
										  {

												//Poison Gas Can Be Friendly...
												if (AvoidanceType==Avoidances.AvoidanceType.PoisonGas)
												{
													 int TeamID=0;
													 try
													 {
														  TeamID=thisObj.CommonData.GetAttribute<int>(ActorAttributeType.TeamID);
													 } catch
													 {
														  if (Bot.Settings.Debug.FunkyLogFlags.HasFlag(LogLevel.Execption))
																Logger.Write(LogLevel.Execption, "Failed to retrieve TeamID attribute for object {0}", thisObstacle.InternalName);
													 }

													 //ID of 1 means its non-hostile!
													 if (TeamID==1)
													 {
														  //Logger.Write(LogLevel.None, "Ignoring Avoidance {0} due to Friendly TeamID match!", tmp_CachedObj.InternalName);
														  BlacklistCache.AddObjectToBlacklist(tmp_CachedObj.RAGUID, BlacklistType.Permanent);
														  continue;
													 }
												}
	

												thisObstacle=new CacheAvoidance(tmp_CachedObj, AvoidanceType);
												ObjectCache.Obstacles.Add(thisObstacle);
										  }
										  else
										  {
												//Obstacles.
												thisObstacle=new CacheServerObject(tmp_CachedObj);
												ObjectCache.Obstacles.Add(thisObstacle);
												continue;
										  }
									 }

									 //Test if this avoidance requires movement now.
									 if (thisObstacle is CacheAvoidance)
									 {
										  //Check last time we attempted avoidance movement (Only if its been at least a second since last time we required it..)
										  //if (DateTime.Now.Subtract(Bot.Combat.LastAvoidanceMovement).TotalMilliseconds<1000)
										  //continue;

										  CacheAvoidance thisAvoidance=thisObstacle as CacheAvoidance;

										  if (AvoidanceCache.IgnoreAvoidance(thisAvoidance.AvoidanceType)) continue;

										  //Only update position of Movement Avoidances!
										  if (thisAvoidance.Obstacletype.Value==ObstacleType.MovingAvoidance)
										  {
												//Blacklisted updates
												if (thisAvoidance.BlacklistRefreshCounter>0&&
													 !thisAvoidance.CheckUpdateForProjectile)
												{
													 thisAvoidance.BlacklistRefreshCounter--;
												}

												bRequireAvoidance=thisAvoidance.UpdateProjectileRayTest(tmp_CachedObj.Position);
												//If we need to avoid, than enable travel avoidance flag also.
												if (bRequireAvoidance) bTravellingAvoidance=true;
										  }
										  else
										  {
												if (thisObstacle.CentreDistance<50f)
													 Bot.Combat.NearbyAvoidances.Add(thisObstacle.RAGUID);

												if (thisAvoidance.Position.Distance(Bot.Character.Position)<=thisAvoidance.Radius)
													 bRequireAvoidance=true;
										  }

										  Bot.Combat.RequiresAvoidance=bRequireAvoidance;
										  Bot.Combat.TravellingAvoidance=bTravellingAvoidance;
										  if (bRequireAvoidance)
												Bot.Combat.TriggeringAvoidances.Add((CacheAvoidance)thisObstacle);
									 }
									 else
									 {
										  //Add this server object to cell weighting in MGP
										  //MGP.AddCellWeightingObstacle(thisObstacle.SNOID, thisObstacle.CollisionRadius.Value);

										  //Add nearby objects to our collection (used in navblock/obstaclecheck methods to reduce queries)
										  if (thisObstacle.CentreDistance<25f)
												Bot.Combat.NearbyObstacleObjects.Add((CacheServerObject)thisObstacle);
									 }

									 continue;
									 #endregion
								}

								if (tmp_CachedObj.ObjectShouldBeRecreated)
								{
									 //Specific updates
									 if (tmp_CachedObj.Actortype.Value==ActorType.Item)
									 {
										  tmp_CachedObj=new CacheItem(tmp_CachedObj);
									 }
									 else if (tmp_CachedObj.Actortype.Value==ActorType.Unit)
									 {
										  tmp_CachedObj=new CacheUnit(tmp_CachedObj);
									 }
									 else if (tmp_CachedObj.Actortype.Value==ActorType.Gizmo)
									 {

										  if (CheckTargetTypeFlag(tmp_CachedObj.targetType.Value,TargetType.Interactables))
												tmp_CachedObj=new CacheInteractable(tmp_CachedObj);
										  else
												tmp_CachedObj=new CacheDestructable(tmp_CachedObj);
									 }

									 //Update Properties
									 tmp_CachedObj.UpdateProperties();
								}

								if (!tmp_CachedObj.UpdateData())
									 continue;

								//Obstacle cache
								if (tmp_CachedObj.Obstacletype.Value!=ObstacleType.None
									 &&(CheckTargetTypeFlag(tmp_CachedObj.targetType.Value,TargetType.ServerObjects)))
								{
									 CacheObstacle thisObstacleObj;

									 if (!ObjectCache.Obstacles.TryGetValue(tmp_CachedObj.RAGUID, out thisObstacleObj))
									 {
										  CacheServerObject newobj=new CacheServerObject(tmp_CachedObj);
										  ObjectCache.Obstacles.Add(tmp_CachedObj.RAGUID, newobj);

										  //Add nearby objects to our collection (used in navblock/obstaclecheck methods to reduce queries)
										  if (CacheIDLookup.hashSNONavigationObstacles.Contains(newobj.SNOID))
												Navigation.MGP.AddCellWeightingObstacle(newobj.SNOID, newobj.Radius);
									 }
									 else
									 {
										  if (thisObstacleObj.targetType.Value==TargetType.Unit)
										  {
												//Since units position requires updating, we update using the CacheObject
												thisObstacleObj.Position=tmp_CachedObj.Position;
												ObjectCache.Obstacles[tmp_CachedObj.RAGUID]=thisObstacleObj;
										  }
										  if (thisObstacleObj.CentreDistance<=25f)
												Bot.Combat.NearbyObstacleObjects.Add((CacheServerObject)thisObstacleObj);
									 }

								}

								//cache it
								if (ObjectCache.Objects.ContainsKey(tmp_CachedObj.RAGUID))
									 ObjectCache.Objects[tmp_CachedObj.RAGUID]=tmp_CachedObj;
								else
									 ObjectCache.Objects.Add(tmp_CachedObj.RAGUID, tmp_CachedObj);


						  }
					 } //End of Loop


					 //Tally up unseen objects.
					 var UnseenObjects=ObjectCache.Objects.Keys.Where<int>(O => !hashDoneThisRactor.Contains(O)).ToList();
					 if (UnseenObjects.Count()>0)
					 {
						  for (int i=0; i<UnseenObjects.Count(); i++)
						  {
								ObjectCache.Objects[UnseenObjects[i]].LoopsUnseen++;
						  }
					 }

					 //Trim our collection every 5th refresh.
					 UpdateLoopCounter++;
					 if (UpdateLoopCounter>4)
					 {
						  UpdateLoopCounter=0;
						  //Now flag any objects not seen for 5 loops. Gold/Globe only 1 loop.
						  foreach (var item in ObjectCache.Objects.Values.Where<CacheObject>(CO => CO.LoopsUnseen>=5||
								(CO.targetType.HasValue&&(CheckTargetTypeFlag(CO.targetType.Value,TargetType.Gold|TargetType.Globe))&&CO.LoopsUnseen>0)))
						  {
								item.NeedsRemoved=true;
						  }
					 }


					 return true;
				}
				//Trimming/Update Vars
				private static int UpdateLoopCounter=0;


				#region SNO Cache Dictionaries
				internal static Dictionary<int, ActorType?> dictActorType=new Dictionary<int, ActorType?>();
				internal static Dictionary<int, TargetType?> dictTargetType=new Dictionary<int, TargetType?>();
				internal static Dictionary<int, MonsterSize?> dictMonstersize=new Dictionary<int, MonsterSize?>();
				internal static Dictionary<int, MonsterType?> dictMonstertype=new Dictionary<int, MonsterType?>();
				internal static Dictionary<int, float?> dictCollisionRadius=new Dictionary<int, float?>();
				internal static Dictionary<int, String> dictInternalName=new Dictionary<int, String>();
				internal static Dictionary<int, ObstacleType?> dictObstacleType=new Dictionary<int, ObstacleType?>();
				internal static Dictionary<int, float?> dictActorSphereRadius=new Dictionary<int, float?>();
				internal static Dictionary<int, bool?> dictCanBurrow=new Dictionary<int, bool?>();
				internal static Dictionary<int, bool?> dictGrantsNoXp=new Dictionary<int, bool?>();
				internal static Dictionary<int, bool?> dictDropsNoLoot=new Dictionary<int, bool?>();
				internal static Dictionary<int, GizmoType?> dictGizmoType=new Dictionary<int, GizmoType?>();
				internal static Dictionary<int, bool?> dictIsBarricade=new Dictionary<int, bool?>();
				internal static Dictionary<int, double> dictProjectileSpeed=new Dictionary<int, double>();
				#endregion

				internal static readonly HashSet<SNOAnim> KnockbackAnims=new HashSet<SNOAnim>
				{

					 /*










					 */
				};

				//Common Used Profile Tags that should be considered Out-Of-Combat Behavior.
				internal static readonly HashSet<Type> oocDBTags=new HashSet<Type> 
																	{ 
																	  typeof(Zeta.CommonBot.Profile.Common.UseWaypointTag), 
																	  typeof(Zeta.CommonBot.Profile.Common.UseObjectTag),
																	  typeof(Zeta.CommonBot.Profile.Common.UseTownPortalTag),
																	  typeof(Zeta.CommonBot.Profile.Common.WaitTimerTag),
																	  typeof (FunkyTrinity.XMLTags.TrinityTownPortal),
																	};

				//Common Used Profile Tags that requires backtracking during combat sessions.
				internal static readonly HashSet<Type> InteractiveTags=new HashSet<Type> 
																	{ 
																	  typeof(Zeta.CommonBot.Profile.Common.UseWaypointTag), 
																	  typeof(Zeta.CommonBot.Profile.Common.UseObjectTag),
																	  typeof(Zeta.CommonBot.Profile.Common.UseTownPortalTag),
																	  typeof(Zeta.CommonBot.Profile.Common.UsePortalTag),
																	};
		  }
	 
}