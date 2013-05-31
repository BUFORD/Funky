﻿using System;
using System.Linq;
using Zeta;
using System.Windows;
using Zeta.Common;
using Zeta.Internals.Actors;
using Zeta.CommonBot;
using Zeta.TreeSharp;

namespace FunkyTrinity
{
	 public partial class Funky
	 {
		  public class CacheItem : CacheObject
		  {
				public CacheItem(CacheObject baseobj)
					 : base(baseobj)
				{
				}



				public DiaItem ref_DiaItem { get; set; }
				public int? DynamicID { get; set; }
				public ItemQuality? Itemquality { get; set; }
				public bool ItemQualityRechecked { get; set; }
				public GilesItemType GilesItemType { get; set; }
				public bool? ShouldPickup { get; set; }

				[System.Diagnostics.DebuggerNonUserCode]
				public int? GoldAmount { get; set; }

				public int? BalanceID { get; set; }



				public CacheBalance BalanceData
				{
					 get
					 {
						  if (BalanceID.HasValue&&dictGameBalanceCache.ContainsKey(BalanceID.Value))
								return dictGameBalanceCache[BalanceID.Value];
						  else
								return null;
					 }
				}

				public override bool IsZDifferenceValid
				{
					 get
					 {
						  float fThisHeightDifference=Difference(Bot.Character.Position.Z, this.Position.Z);

						  if (this.targetType.HasValue&&this.targetType.Value==TargetType.Item)
						  {
								if (fThisHeightDifference>=26f)
									 return false;
						  }
						  else
						  {
								// Gold/Globes at 11+ z-height difference
								if (fThisHeightDifference>=11f)
									 return false;
						  }

						  return base.IsZDifferenceValid;
					 }
				}

				public override bool ObjectShouldBeRecreated
				{
					 get
					 {
						  return false;
					 }
				}

				public override void UpdateWeight()
				{
					 base.UpdateWeight();

					 if (this.Weight!=1)
					 {
						  switch (this.targetType.Value)
						  {

								case TargetType.Item:
									 this.Weight=13000d-(Math.Floor(this.CentreDistance)*190d);
									 // Point-blank items get a weight increase 
									 if (this.CentreDistance<=12f)
										  this.Weight+=600d;
									 // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
									 if (this==Bot.Character.LastCachedTarget&&this.CentreDistance<=25f)
										  this.Weight+=600;
									 // Give yellows more weight
									 if (this.Itemquality.Value>=ItemQuality.Rare4)
										  this.Weight+=6000d;
									 // Give legendaries more weight
									 if (this.Itemquality.Value>=ItemQuality.Legendary)
										  this.Weight+=10000d;
									 // Are we prioritizing close-range stuff atm? If so limit it at a value 3k lower than monster close-range priority
									 if ((Bot.Combat.bForceCloseRangeTarget||Bot.Character.bIsRooted))
										  this.Weight=18000-(Math.Floor(this.CentreDistance)*200);
									 // If there's a monster in the path-line to the item, reduce the weight by 25%
									 if (ObjectCache.Obstacles.Monsters.Any(cp => cp.TestIntersection(this)))
										  this.Weight*=0.75;
									 //Finally check if we should reduce the weight when more then 2 monsters are nearby..
									 if (Bot.Combat.iAnythingWithinRange[RANGE_25]>2&&
										  //But Only when we are low in health..
											 (Bot.Character.dCurrentHealthPct<0.25||
										  //Or we havn't changed targets after 2.5 secs
											 DateTime.Now.Subtract(Bot.Combat.dateSincePickedTarget).TotalSeconds>2.5))
										  this.Weight*=0.5;
									 //Did we have a target last time? and if so was it a goblin?
									 if (Bot.Character.LastCachedTarget.RAGUID!=-1)
									 {
										  if (SnoCacheLookup.hashActorSNOGoblins.Contains(Bot.Character.LastCachedTarget.RAGUID))
												this.Weight=0;
									 }
									 break;
								case TargetType.Gold:
									 if (this.GoldAmount>0)
										  this.Weight=11000d-(Math.Floor(this.CentreDistance)*200d);
									 // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
									 if (this==Bot.Character.LastCachedTarget&&this.CentreDistance<=25f)
										  this.Weight+=600;
									 // Are we prioritizing close-range stuff atm? If so limit it at a value 3k lower than monster close-range priority
									 if ((Bot.Combat.bForceCloseRangeTarget||Bot.Character.bIsRooted))
										  this.Weight=18000-(Math.Floor(this.CentreDistance)*200);
									 // If there's a monster in the path-line to the item, reduce the weight by 25%
									 if (ObjectCache.Obstacles.Monsters.Any(cp => cp.TestIntersection(this)))
										  this.Weight*=0.75;
									 //Did we have a target last time? and if so was it a goblin?
									 if (Bot.Character.LastCachedTarget.RAGUID!=-1)
									 {
										  if (SnoCacheLookup.hashActorSNOGoblins.Contains(Bot.Character.LastCachedTarget.RAGUID))
												this.Weight=0;
									 }
									 break;
								case TargetType.Globe:
									 if (Bot.Character.dCurrentHealthPct>Bot.Class.EmergencyHealthGlobeLimit)
									 {
										  this.Weight=0;
									 }
									 else
									 {
										  // Ok we have globes enabled, and our health is low...!
										  this.Weight=17000d-(Math.Floor(this.CentreDistance)*90d);
										  // Point-blank items get a weight increase
										  if (this.CentreDistance<=15f)
												this.Weight+=3000d;
										  // Close items get a weight increase
										  if (this.CentreDistance<=60f)
												this.Weight+=1500d;
										  // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
										  if (this==Bot.Character.LastCachedTarget&&this.CentreDistance<=25f)
												this.Weight+=400;
										  // If there's a monster in the path-line to the item, reduce the weight by 15% for each
										  Vector3 point=this.Position;
										  foreach (CacheServerObject tempobstacle in ObjectCache.Obstacles.Monsters.Where(cp => cp.TestIntersection(this)))
										  {
												this.Weight*=0.85;
										  }
										  // See if there's any AOE avoidance in that spot, if so reduce the weight by 10%
										  if (ObjectCache.Obstacles.Avoidances.Any(cp => cp.TestIntersection(this)))
												this.Weight*=0.9;
										  // Calculate a spot reaching a little bit further out from the globe, to help globe-movements
										  if (this.Weight>0)
												this.Position=MathEx.CalculatePointFrom(this.Position, Bot.Character.Position, this.CentreDistance+3f);
									 }
									 break;
						  }
					 }
				}

				public override bool ObjectIsValidForTargeting
				{
					 get
					 {
						  if (!base.ObjectIsValidForTargeting)
								return false;



						  //Z-Height Difference Check
						  if (!this.IsZDifferenceValid)
						  {
								this.BlacklistLoops=3;
								return false;
						  }

						  if (this.targetType.Value==TargetType.Item)
						  {
								if (!this.ShouldPickup.Value)
								{
									 this.NeedsRemoved=true;
									 this.BlacklistFlag=BlacklistType.Temporary;
									 return false;
								}


								// Ignore it if it's not in range yet - allow legendary items to have 15 feet extra beyond our profile max loot radius
								double dMultiplier=1d;

								if (iKeepLootRadiusExtendedFor>0||this.Itemquality>=ItemQuality.Rare4)
								{
									 dMultiplier+=0.25d;
								}
								if (iKeepLootRadiusExtendedFor>0||this.Itemquality>=ItemQuality.Legendary)
								{
									 dMultiplier+=0.45d;
								}

								double lootDistance=(Bot.Combat.iCurrentMaxLootRadius+SettingsFunky.ItemRange)*dMultiplier;

								if (this.CentreDistance>lootDistance) return false;

						  }
						  else
						  {
								// Blacklist objects already in pickup radius range
								if (this.CentreDistance<Bot.Character.PickupRadius)
								{
									 this.NeedsRemoved=true;
									 this.BlacklistFlag=BlacklistType.Temporary;
									 return false;
								}

								if (this.targetType==TargetType.Gold)
								{
									 if (this.GoldAmount.Value<SettingsFunky.MinimumGoldPile)
									 {
										  this.NeedsRemoved=true;
										  this.BlacklistFlag=BlacklistType.Temporary;
										  return false;
									 }
									 else if (this.CentreDistance>SettingsFunky.GoldRange)
									 {
										  return false;
									 }
								}
								else
								{
									 //GLOBE
									 // Ignore it if it's not in range yet
									 if (this.CentreDistance>Bot.Combat.iCurrentMaxLootRadius||this.CentreDistance>37f)
									 {
										  this.BlacklistFlag=BlacklistType.Temporary;
										  this.NeedsRemoved=true;
										  return false;
									 }
								}
						  }


						  return true;
					 }
				}

				public override bool UpdateData()
				{


					 if (this.ref_DiaItem==null)
					 {
						  try
						  {
								this.ref_DiaItem=(DiaItem)base.ref_DiaObject;
						  } catch (NullReferenceException) { Logging.WriteVerbose("Failure to convert obj to DiaItem!"); return false; }
					 }

					 if (this.targetType.Value==TargetType.Item)
					 {
						  #region Item
						  #region DynamicID
						  if (!this.DynamicID.HasValue)
						  {
								try
								{
									 this.DynamicID=base.ref_DiaObject.CommonData.DynamicId;
								} catch (NullReferenceException ex) { Logging.WriteVerbose("Failure to get Dynamic ID for {0} \r\n Exception: {1}", this.InternalName, ex.Message); return false; }
						  }
						  #endregion

						  //Gamebalance Update
						  if (!this.BalanceID.HasValue)
						  {
								try
								{
									 this.BalanceID=base.ref_DiaObject.CommonData.GameBalanceId;
								} catch (NullReferenceException) { Logging.WriteVerbose("Failure to get gamebalance ID for item {0}", this.InternalName); return false; }
						  }

						  if (!this.BalanceID.HasValue) return false;

						  //Check if game balance needs updated
						  #region GameBalance
						  if (this.BalanceData==null||this.BalanceData.bNeedsUpdated)
						  {
								CacheBalance thisnewGamebalance;


								try
								{
									 int tmp_Level=this.ref_DiaItem.CommonData.Level;
									 ItemType tmp_ThisType=this.ref_DiaItem.CommonData.ItemType;
									 ItemBaseType tmp_ThisDBItemType=this.ref_DiaItem.CommonData.ItemBaseType;
									 FollowerType tmp_ThisFollowerType=this.ref_DiaItem.CommonData.FollowerSpecialType;

									 bool tmp_bThisOneHanded=false;
									 bool tmp_bThisTwoHanded=false;
									 if (tmp_ThisDBItemType==ItemBaseType.Weapon)
									 {
										  tmp_bThisOneHanded=this.ref_DiaItem.CommonData.IsOneHand;
										  tmp_bThisTwoHanded=this.ref_DiaItem.CommonData.IsTwoHand;
									 }

									 thisnewGamebalance=new CacheBalance(tmp_Level, tmp_ThisType, tmp_ThisDBItemType, tmp_bThisOneHanded, tmp_bThisTwoHanded, tmp_ThisFollowerType);
								} catch (NullReferenceException)
								{
									 Logging.WriteVerbose("Failure to add/update gamebalance data for item {0}", this.InternalName);
									 return false;
								}


								if (this.BalanceData==null)
								{
									 dictGameBalanceCache.Add(this.BalanceID.Value, thisnewGamebalance);
								}
								else
								{
									 dictGameBalanceCache[this.BalanceID.Value]=thisnewGamebalance;
								}

						  }
						  #endregion

						  //Item Quality / Recheck
						  #region ItemQuality
						  if (!this.Itemquality.HasValue||this.ItemQualityRechecked==false)
						  {

								try
								{
									 this.Itemquality=this.ref_DiaItem.CommonData.ItemQualityLevel;
								} catch (NullReferenceException) { Logging.WriteVerbose("Failure to get item quality for {0}", this.InternalName); return false; }


								if (!this.ItemQualityRechecked)
									 this.ItemQualityRechecked=true;
								else
									 this.NeedsUpdate=false;
						  }
						  #endregion


						  //Pickup?
						  // Now see if we actually want it
						  #region PickupValidation
						  if (!this.ShouldPickup.HasValue)
						  {
								if (SettingsFunky.UseItemRules)
								{
									 Interpreter.InterpreterAction action=ItemRulesEval.checkPickUpItem(this, ItemEvaluationType.PickUp);
									 switch (action)
									 {
										  case Interpreter.InterpreterAction.PICKUP:
												this.ShouldPickup=true;
												break;
										  case Interpreter.InterpreterAction.IGNORE:
												this.ShouldPickup=false;
												break;
									 }
								}

								if (!this.ShouldPickup.HasValue)
								{
									 //Use Giles Scoring or DB Weighting..
									 this.ShouldPickup=
										  SettingsFunky.ItemRuleGilesScoring?GilesPickupItemValidation(this)
										  :ItemManager.Current.EvaluateItem((ACDItem)this.ref_DiaItem.CommonData, Zeta.CommonBot.ItemEvaluationType.PickUp); ;
								}

								//Low Level Evaluation
								if (SettingsFunky.UseLevelingLogic&&Bot.Character.iMyLevel<60)
								{
									 if (this.Itemquality.HasValue&&this.Itemquality.Value>=ItemQuality.Magic1)
									 {
										  //Check if we currently use this type of item.
										  if (!ItemTypeIsRestricted(this.BalanceData.thisItemType))
												this.ShouldPickup=true;
									 }
								}
						  }
						  else
								this.NeedsUpdate=false;

						  #endregion

						  #endregion
					 }
					 else
					 {
						  #region Gold
						  //Get gold value..
						  if (!this.GoldAmount.HasValue)
						  {
								try
								{
									 this.GoldAmount=this.ref_DiaItem.CommonData.GetAttribute<int>(ActorAttributeType.Gold);
								} catch (NullReferenceException) { Logging.WriteVerbose("Failure to get gold amount for gold pile!"); return false; }
						  }

						  this.NeedsUpdate=false;
						  #endregion
					 }
					 return true;
				}

				public override bool IsStillValid()
				{
					 if (ref_DiaItem==null||!ref_DiaItem.IsValid||ref_DiaItem.BaseAddress==IntPtr.Zero)
						  return false;

					 return base.IsStillValid();
				}

				public override RunStatus Interact()
				{
					 //Only validate if we can pickup if slots are minimum
					 if (Bot.Character.FreeBackpackSlots<=8)
					 {
						  if (this.ref_DiaItem!=null&&this.ref_DiaItem.BaseAddress!=IntPtr.Zero)
						  {

								if (!Zeta.CommonBot.Logic.BrainBehavior.CanPickUpItem(this.ref_DiaItem))
								{
									 Zeta.CommonBot.Logic.BrainBehavior.ForceTownrun("No more space to pickup item");
									 return RunStatus.Success;
								}


						  }
						  else
						  {
								Log("Failure to validate DiaItem!");
								return RunStatus.Success;
						  }
					 }

					 // Force waiting for global cooldown timer or long-animation abilities
					 if (Bot.Combat.powerPrime.iForceWaitLoopsBefore>=1)
					 {
						  //Logging.WriteDiagnostic("Debug: Force waiting BEFORE ability " + powerPrime.powerThis.ToString() + "...");
						  Bot.Combat.bWaitingForPower=true;
						  if (Bot.Combat.powerPrime.iForceWaitLoopsBefore>=1)
								Bot.Combat.powerPrime.iForceWaitLoopsBefore--;
						  return Zeta.TreeSharp.RunStatus.Running;
					 }
					 Bot.Combat.bWaitingForPower=false;

					 // Pick the item up the usepower way, and "blacklist" for a couple of loops
					 WaitWhileAnimating(15, true);
					 ZetaDia.Me.UsePower(SNOPower.Axe_Operate_Gizmo, Vector3.Zero, 0, this.AcdGuid.Value);
					 Bot.Combat.lastChangedZigZag=DateTime.Today;
					 Bot.Combat.vPositionLastZigZagCheck=Vector3.Zero;


					 Bot.Combat.ShouldCheckItemLooted=true;

					 WaitWhileAnimating(5, true);
					 return Zeta.TreeSharp.RunStatus.Running;
				}

				public override bool WithinInteractionRange()
				{
					 float fRangeRequired=0f;
					 float fDistanceReduction=0f;

					 if (this.targetType.Value==TargetType.Item)
					 {
						  fRangeRequired=5f;
						  fDistanceReduction=0f;

						  // If we're having stuck issues, try forcing us to get closer to this item
						  if (Bot.Combat.bForceCloseRangeTarget)
								fRangeRequired-=1f;

						  if (Bot.Character.Position.Distance(Bot.Combat.vCurrentDestination)<=1.5f)
								fDistanceReduction+=1f;
					 }
					 else
					 {
						  if (targetType.Value==TargetType.Gold)
						  {
								fRangeRequired=Bot.Character.PickupRadius;
								if (fRangeRequired<2f)
									 fRangeRequired=2f;
						  }
						  else
						  {
								fRangeRequired=Bot.Character.PickupRadius;
								if (fRangeRequired<2f)
									 fRangeRequired=2f;
								if (fRangeRequired>5f)
									 fRangeRequired=5f;
						  }
					 }

					 base.DistanceFromTarget=Vector3.Distance(Bot.Character.Position, this.Position)-fDistanceReduction;
					 return (fRangeRequired<=0f||base.DistanceFromTarget<=fRangeRequired);
				}

				public override string DebugString
				{
					 get
					 {
						  return String.Format("{0}\r\n InteractAttempts={1} {2} {3}",
								base.DebugString, this.InteractionAttempts.ToString(),
								this.GoldAmount.HasValue?"Gold:"+this.GoldAmount.Value.ToString():"",
								this.ShouldPickup.HasValue?"PickUp="+this.ShouldPickup.Value.ToString():"");
					 }
				}

		  }
	 }
}