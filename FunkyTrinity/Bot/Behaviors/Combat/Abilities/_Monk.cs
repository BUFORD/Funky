﻿using System;
using Zeta;
using Zeta.Common;
using Zeta.Internals.Actors;
using Zeta.CommonBot;


namespace FunkyTrinity
{
	 public partial class Funky
	 {
		  internal static Ability MonkAbility(bool bCurrentlyAvoiding=false, bool bOOCBuff=false, bool bDestructiblePower=false)
		  {
	
				// Pick the best destructible power available
				if (bDestructiblePower)
				{
					 //Tempest Rush used recently..
					 if (HotbarAbilitiesContainsPower(SNOPower.Monk_TempestRush))
					 {
						  //Check if we are still using..
						  Bot.Character.UpdateAnimationState(false, true);
						  if (Bot.Character.CurrentSNOAnim.HasFlag(SNOAnim.Monk_Female_Hobble_Run|SNOAnim.Monk_Male_HTH_Hobble_Run))
								return new Ability(SNOPower.Monk_TempestRush, 4f, vNullLocation, Bot.Character.iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
					 }

					 SNOPower destructiblePower=Bot.Class.DestructiblePower();
					 return new Ability(destructiblePower, 10f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
				}

				CacheUnit thisCacheUnitObj;

				if (!bOOCBuff&&Bot.Target.CurrentTarget!=null&&Bot.Target.CurrentTarget.targetType.Value==TargetType.Unit)
					 thisCacheUnitObj=(CacheUnit)Bot.Target.CurrentTarget;
				else
					 thisCacheUnitObj=null;

				// Monks need 80 for special spam like tempest rushing
				Bot.Class.iWaitingReservedAmount=80;
				#region Mantras
				// 4 Mantras for the initial buff (slow-use)
				if (HotbarAbilitiesContainsPower(SNOPower.Monk_MantraOfEvasion)&&!HasBuff(SNOPower.Monk_MantraOfEvasion)&&
					Bot.Character.dCurrentEnergy>50&&AbilityUseTimer(SNOPower.Monk_MantraOfEvasion, true))
				{
					 return new Ability(SNOPower.Monk_MantraOfEvasion, 0f, vNullLocation, Bot.Character.iCurrentWorldID, -1, 0, 1, USE_SLOWLY);
				}
				if (HotbarAbilitiesContainsPower(SNOPower.Monk_MantraOfConviction)&&!HasBuff(SNOPower.Monk_MantraOfConviction)&&
					Bot.Character.dCurrentEnergy>50&&AbilityUseTimer(SNOPower.Monk_MantraOfConviction, true))
				{
					 return new Ability(SNOPower.Monk_MantraOfConviction, 0f, vNullLocation, Bot.Character.iCurrentWorldID, -1, 0, 1, USE_SLOWLY);
				}
				if (HotbarAbilitiesContainsPower(SNOPower.Monk_MantraOfHealing)&&!HasBuff(SNOPower.Monk_MantraOfHealing)&&
					Bot.Character.dCurrentEnergy>50&&AbilityUseTimer(SNOPower.Monk_MantraOfHealing, true))
				{
					 return new Ability(SNOPower.Monk_MantraOfHealing, 0f, vNullLocation, Bot.Character.iCurrentWorldID, -1, 0, 1, USE_SLOWLY);
				}
				if (HotbarAbilitiesContainsPower(SNOPower.Monk_MantraOfRetribution)&&!HasBuff(SNOPower.Monk_MantraOfRetribution)&&
					Bot.Character.dCurrentEnergy>50&&AbilityUseTimer(SNOPower.Monk_MantraOfRetribution, true))
				{
					 return new Ability(SNOPower.Monk_MantraOfRetribution, 0f, vNullLocation, Bot.Character.iCurrentWorldID, -1, 0, 1, USE_SLOWLY);
				} 
				#endregion
				#region Mystic ally
				// Mystic ally
				if (HotbarAbilitiesContainsPower(SNOPower.Monk_MysticAlly)&&Bot.Character.dCurrentEnergy>=90&&Bot.Character.PetData.MysticAlly==0&&
					AbilityUseTimer(SNOPower.Monk_MysticAlly)&&PowerManager.CanCast(SNOPower.Monk_MysticAlly))
				{
					 return new Ability(SNOPower.Monk_MysticAlly, 0f, vNullLocation, Bot.Character.iCurrentWorldID, -1, 2, 2, USE_SLOWLY);
				} 
				#endregion
				#region InnerSanctuary
				// InnerSanctuary
				if (!bOOCBuff&&Bot.Character.dCurrentHealthPct<=0.45&&HotbarAbilitiesContainsPower(SNOPower.Monk_InnerSanctuary)&&
					AbilityUseTimer(SNOPower.Monk_InnerSanctuary, true)&&
					Bot.Character.dCurrentEnergy>=30&&PowerManager.CanCast(SNOPower.Monk_InnerSanctuary))
				{
					 return new Ability(SNOPower.Monk_InnerSanctuary, 0f, vNullLocation, Bot.Character.iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
				} 
				#endregion
				#region Serenity
				// Serenity if health is low
				if ((Bot.Character.dCurrentHealthPct<=0.50||(Bot.Character.bIsIncapacitated&&Bot.Character.dCurrentHealthPct<=0.75))&&HotbarAbilitiesContainsPower(SNOPower.Monk_Serenity)&&
					AbilityUseTimer(SNOPower.Monk_Serenity, true)&&
					Bot.Character.dCurrentEnergy>=10&&PowerManager.CanCast(SNOPower.Monk_Serenity))
				{
					 return new Ability(SNOPower.Monk_Serenity, 0f, vNullLocation, Bot.Character.iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
				} 
				#endregion
				#region Breath of heaven
				// Breath of heaven when needing healing or the buff
				if (!bOOCBuff&&
					(Bot.Character.dCurrentHealthPct<=0.6||!HasBuff(SNOPower.Monk_BreathOfHeaven))&&
							HotbarAbilitiesContainsPower(SNOPower.Monk_BreathOfHeaven)&&
					(Bot.Character.dCurrentEnergy>=25||(!HotbarAbilitiesContainsPower(SNOPower.Monk_Serenity)&&Bot.Character.dCurrentEnergy>=25))&&
							AbilityUseTimer(SNOPower.Monk_BreathOfHeaven)&&PowerManager.CanCast(SNOPower.Monk_BreathOfHeaven))
				{
					 return new Ability(SNOPower.Monk_BreathOfHeaven, 0f, vNullLocation, Bot.Character.iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
				}

				// Breath of heaven OOC healing when 35% or below.
				if (bOOCBuff&&Bot.Character.dCurrentHealthPct<=0.35&&HotbarAbilitiesContainsPower(SNOPower.Monk_BreathOfHeaven)&&
						 Bot.Character.dCurrentEnergy>=25&&AbilityUseTimer(SNOPower.Monk_BreathOfHeaven)&&PowerManager.CanCast(SNOPower.Monk_BreathOfHeaven))
				{
					 return new Ability(SNOPower.Monk_BreathOfHeaven, 0f, vNullLocation, Bot.Character.iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
				} 
				#endregion

				#region Blinding Flash
				// Blinding Flash
				if (!bOOCBuff&&Bot.Character.dCurrentEnergy>10&&HotbarAbilitiesContainsPower(SNOPower.Monk_BlindingFlash)&&
					(
						Bot.Combat.iElitesWithinRange[RANGE_15]>=1||Bot.Character.dCurrentHealthPct<=0.4||
						(Bot.Combat.iAnythingWithinRange[RANGE_20]>=5&&Bot.Combat.iElitesWithinRange[RANGE_50]==0)||
						(Bot.Combat.iAnythingWithinRange[RANGE_15]>=3&&Bot.Character.dCurrentEnergyPct<=0.5)||
						(Bot.Target.CurrentTarget.IsBoss&&Bot.Target.CurrentTarget.RadiusDistance<=15f)||
						(SettingsFunky.Class.bMonkInnaSet&&Bot.Combat.iAnythingWithinRange[RANGE_15]>=1&&HotbarAbilitiesContainsPower(SNOPower.Monk_SweepingWind)&&!HasBuff(SNOPower.Monk_SweepingWind))
					)&&
					 // Check if we don't have breath of heaven
					(!HotbarAbilitiesContainsPower(SNOPower.Monk_BreathOfHeaven)||
					(HotbarAbilitiesContainsPower(SNOPower.Monk_BreathOfHeaven)&&(!SettingsFunky.Class.bMonkInnaSet||
					HasBuff(SNOPower.Monk_BreathOfHeaven))))&&
					 // Check if either we don't have sweeping winds, or we do and it's ready to cast in a moment
					(!HotbarAbilitiesContainsPower(SNOPower.Monk_SweepingWind)||
					(HotbarAbilitiesContainsPower(SNOPower.Monk_SweepingWind)&&(Bot.Character.dCurrentEnergy>=95||
					(SettingsFunky.Class.bMonkInnaSet&&Bot.Character.dCurrentEnergy>=25)||HasBuff(SNOPower.Monk_SweepingWind)))||
					Bot.Character.dCurrentHealthPct<=0.4)&&
					AbilityUseTimer(SNOPower.Monk_BlindingFlash)&&PowerManager.CanCast(SNOPower.Monk_BlindingFlash))
				{
					 return new Ability(SNOPower.Monk_BlindingFlash, 0f, vNullLocation, Bot.Character.iCurrentWorldID, -1, 0, 1, USE_SLOWLY); //intell -- 11f -- 1, 2
				}
				// Blinding Flash as a DEFENSE
				if (!bOOCBuff&&Bot.Character.dCurrentEnergy>10&&HotbarAbilitiesContainsPower(SNOPower.Monk_BlindingFlash)&&
					Bot.Character.dCurrentHealthPct<=0.25&&Bot.Combat.iAnythingWithinRange[RANGE_12]>0&&
					AbilityUseTimer(SNOPower.Monk_BlindingFlash)&&PowerManager.CanCast(SNOPower.Monk_BlindingFlash))
				{
					 return new Ability(SNOPower.Monk_BlindingFlash, 0f, vNullLocation, Bot.Character.iCurrentWorldID, -1, 0, 1, USE_SLOWLY); //intell -- 11f -- 1, 2
				} 
				#endregion
				#region Sweeping wind
				// Sweeping wind
				//intell -- inna
				if (!bOOCBuff&&HotbarAbilitiesContainsPower(SNOPower.Monk_SweepingWind)&&!HasBuff(SNOPower.Monk_SweepingWind)&&
					(Bot.Combat.iElitesWithinRange[RANGE_25]>0||Bot.Combat.iAnythingWithinRange[RANGE_20]>=2||(Bot.Combat.iAnythingWithinRange[RANGE_20]>=1&&SettingsFunky.Class.bMonkInnaSet)||(thisCacheUnitObj!=null&&(thisCacheUnitObj.IsEliteRareUnique||Bot.Target.CurrentTarget.IsBoss)&&Bot.Target.CurrentTarget.RadiusDistance<=25f))&&
					 // Check if either we don't have blinding flash, or we do and it's been cast in the last 6000ms
					 //DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Monk_BlindingFlash]).TotalMilliseconds <= 6000)) &&
					(!HotbarAbilitiesContainsPower(SNOPower.Monk_BlindingFlash)||
					(HotbarAbilitiesContainsPower(SNOPower.Monk_BlindingFlash)&&
					((!SettingsFunky.Class.bMonkInnaSet&&Bot.Combat.iElitesWithinRange[RANGE_50]==0&&thisCacheUnitObj!=null&&!thisCacheUnitObj.IsEliteRareUnique&&!Bot.Target.CurrentTarget.IsBoss)||HasBuff(SNOPower.Monk_BlindingFlash))))&&
					 // Check our mantras, if we have them, are up first
					(!HotbarAbilitiesContainsPower(SNOPower.Monk_MantraOfEvasion)||(HotbarAbilitiesContainsPower(SNOPower.Monk_MantraOfEvasion)&&HasBuff(SNOPower.Monk_MantraOfEvasion)))&&
					(!HotbarAbilitiesContainsPower(SNOPower.Monk_MantraOfConviction)||(HotbarAbilitiesContainsPower(SNOPower.Monk_MantraOfConviction)&&HasBuff(SNOPower.Monk_MantraOfConviction)))&&
					(!HotbarAbilitiesContainsPower(SNOPower.Monk_MantraOfRetribution)||(HotbarAbilitiesContainsPower(SNOPower.Monk_MantraOfRetribution)&&HasBuff(SNOPower.Monk_MantraOfRetribution)))&&
					 // Check the re-use timer and energy costs
					(Bot.Character.dCurrentEnergy>=75||(SettingsFunky.Class.bMonkInnaSet&&Bot.Character.dCurrentEnergy>=5)))
				{
					 return new Ability(SNOPower.Monk_SweepingWind, 0f, vNullLocation, Bot.Character.iCurrentWorldID, -1, 0, 1, USE_SLOWLY); //intell -- 2,2
				} 
				#endregion
				#region Seven Sided Strike
				// Seven Sided Strike
				if (!bOOCBuff&&!bCurrentlyAvoiding&&!Bot.Character.bIsIncapacitated&&
					(Bot.Combat.iElitesWithinRange[RANGE_15]>=1||((thisCacheUnitObj!=null&&thisCacheUnitObj.IsEliteRareUnique||Bot.Target.CurrentTarget.IsBoss)&&Bot.Target.CurrentTarget.RadiusDistance<=15f)||Bot.Character.dCurrentHealthPct<=0.55)&&
					HotbarAbilitiesContainsPower(SNOPower.Monk_SevenSidedStrike)&&((Bot.Character.dCurrentEnergy>=50&&!Bot.Character.bWaitingForReserveEnergy)||Bot.Character.dCurrentEnergy>=Bot.Class.iWaitingReservedAmount)&&
					AbilityUseTimer(SNOPower.Monk_SevenSidedStrike, true)&&PowerManager.CanCast(SNOPower.Monk_SevenSidedStrike))
				{
					 return new Ability(SNOPower.Monk_SevenSidedStrike, 16f, Bot.Target.CurrentTarget.Position, Bot.Character.iCurrentWorldID, -1, 2, 3, USE_SLOWLY);
				} 
				#endregion

				#region Exploding Palm
				// Exploding Palm
				if (!bOOCBuff&&!bCurrentlyAvoiding&&!Bot.Character.bIsIncapacitated&&
					(Bot.Combat.iElitesWithinRange[RANGE_25]>0||Bot.Combat.iAnythingWithinRange[RANGE_15]>=3||((thisCacheUnitObj!=null&&thisCacheUnitObj.IsEliteRareUnique||Bot.Target.CurrentTarget.IsBoss)&&Bot.Target.CurrentTarget.RadiusDistance<=14f))&&
					HotbarAbilitiesContainsPower(SNOPower.Monk_ExplodingPalm)&&
					((Bot.Character.dCurrentEnergy>=40&&!Bot.Character.bWaitingForReserveEnergy)||Bot.Character.dCurrentEnergy>=Bot.Class.iWaitingReservedAmount)&&
					AbilityUseTimer(SNOPower.Monk_ExplodingPalm)&&PowerManager.CanCast(SNOPower.Monk_ExplodingPalm))
				{
					 return new Ability(SNOPower.Monk_ExplodingPalm, 14f, vNullLocation, -1, Bot.Target.CurrentTarget.AcdGuid.Value, 1, 1, USE_SLOWLY);
				} 
				#endregion
				#region Cyclone Strike
				// Cyclone Strike
				if (!bOOCBuff&&!bCurrentlyAvoiding&&!Bot.Character.bIsIncapacitated&&
					(Bot.Combat.iElitesWithinRange[RANGE_20]>=1||Bot.Combat.iAnythingWithinRange[RANGE_20]>=2||Bot.Character.dCurrentEnergyPct>=0.5||((thisCacheUnitObj!=null&&thisCacheUnitObj.IsEliteRareUnique||Bot.Target.CurrentTarget.IsBoss)&&Bot.Target.CurrentTarget.RadiusDistance<=18f))&&
					HotbarAbilitiesContainsPower(SNOPower.Monk_CycloneStrike)&&
					((Bot.Character.dCurrentEnergy>=70&&!Bot.Character.bWaitingForReserveEnergy)||Bot.Character.dCurrentEnergy>=Bot.Class.iWaitingReservedAmount)&&
					AbilityUseTimer(SNOPower.Monk_CycloneStrike)&&PowerManager.CanCast(SNOPower.Monk_CycloneStrike))
				{
					 return new Ability(SNOPower.Monk_CycloneStrike, 0f, vNullLocation, Bot.Character.iCurrentWorldID, -1, 2, 2, USE_SLOWLY);
				} 
				#endregion

				#region Mantra spam
				// 4 Mantra spam for the 4 second buff
				if (!bOOCBuff&&Bot.Character.dCurrentEnergy>50&&(
						Bot.Character.dCurrentEnergy>=135||
						(HasBuff(SNOPower.Monk_SweepingWind)&&(Bot.Character.dCurrentEnergy>=75||
						(Bot.Character.dCurrentEnergy>=65&&Bot.Character.dCurrentHealthPct<=0.8))&&
					 // Checking we have no expensive finishers
						!HotbarAbilitiesContainsPower(SNOPower.Monk_SevenSidedStrike)&&!HotbarAbilitiesContainsPower(SNOPower.Monk_LashingTailKick)&&
						!HotbarAbilitiesContainsPower(SNOPower.Monk_WaveOfLight)&&!HotbarAbilitiesContainsPower(SNOPower.Monk_CycloneStrike)&&
						!HotbarAbilitiesContainsPower(SNOPower.Monk_ExplodingPalm)))&&
					(Bot.Combat.iElitesWithinRange[RANGE_30]>=1||Bot.Combat.iAnythingWithinRange[RANGE_30]>=3))
				{

					 if (HotbarAbilitiesContainsPower(SNOPower.Monk_MantraOfEvasion)&&Bot.Character.dCurrentEnergy>50&&AbilityUseTimer(SNOPower.Monk_MantraOfEvasion))
					 {
						  return new Ability(SNOPower.Monk_MantraOfEvasion, 0f, vNullLocation, Bot.Character.iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
					 }
					 if (HotbarAbilitiesContainsPower(SNOPower.Monk_MantraOfConviction)&&Bot.Character.dCurrentEnergy>50&&AbilityUseTimer(SNOPower.Monk_MantraOfConviction))
					 {
						  return new Ability(SNOPower.Monk_MantraOfConviction, 0f, vNullLocation, Bot.Character.iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
					 }
					 if (HotbarAbilitiesContainsPower(SNOPower.Monk_MantraOfRetribution)&&Bot.Character.dCurrentEnergy>50&&AbilityUseTimer(SNOPower.Monk_MantraOfRetribution))
					 {
						  return new Ability(SNOPower.Monk_MantraOfRetribution, 0f, vNullLocation, Bot.Character.iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
					 }
					 if (HotbarAbilitiesContainsPower(SNOPower.Monk_MantraOfHealing)&&Bot.Character.dCurrentEnergy>50&&AbilityUseTimer(SNOPower.Monk_MantraOfHealing))
					 {
						  return new Ability(SNOPower.Monk_MantraOfHealing, 0f, vNullLocation, Bot.Character.iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
					 }
				} 
				#endregion

				#region Lashing Tail Kick
				// Lashing Tail Kick
				if (!bOOCBuff&&!bCurrentlyAvoiding&&HotbarAbilitiesContainsPower(SNOPower.Monk_LashingTailKick)&&!Bot.Character.bIsIncapacitated&&
					(Bot.Combat.iElitesWithinRange[RANGE_15]>0||Bot.Combat.iAnythingWithinRange[RANGE_15]>4||((thisCacheUnitObj!=null&&thisCacheUnitObj.IsEliteRareUnique||Bot.Target.CurrentTarget.IsBoss)&&Bot.Target.CurrentTarget.RadiusDistance<=10f))&&
					 // Either doesn't have sweeping wind, or does but the buff is already up
					(!HotbarAbilitiesContainsPower(SNOPower.Monk_SweepingWind)||(HotbarAbilitiesContainsPower(SNOPower.Monk_SweepingWind)&&HasBuff(SNOPower.Monk_SweepingWind)))&&
					AbilityUseTimer(SNOPower.Monk_LashingTailKick)&&
					((Bot.Character.dCurrentEnergy>=65&&!Bot.Character.bWaitingForReserveEnergy)||Bot.Character.dCurrentEnergy>=Bot.Class.iWaitingReservedAmount))
				{
					 return new Ability(SNOPower.Monk_LashingTailKick, 10f, vNullLocation, -1, Bot.Target.CurrentTarget.AcdGuid.Value, 1, 1, USE_SLOWLY);
				} 
				#endregion
				#region Wave of light
				// Wave of light
				if (!bOOCBuff&&!bCurrentlyAvoiding&&HotbarAbilitiesContainsPower(SNOPower.Monk_WaveOfLight)
					 &&!Bot.Character.bIsIncapacitated&&AbilityUseTimer(SNOPower.Monk_WaveOfLight)
					 &&(Bot.Character.dCurrentEnergy>=75||Bot.Class.RuneIndexCache[SNOPower.Monk_WaveOfLight]==3&&Bot.Character.dCurrentEnergy>=40)
					 &&Bot.Combat.iAnythingWithinRange[RANGE_12]>2)
				{
					 System.Collections.Generic.List<Cluster> clusters=ObjectCache.Objects.Clusters(7d, 35f, 3, true);
					 if (clusters.Count>0)
					 {
						  Vector3 Center=clusters[0].ListUnits[0].Position;
						  float Distance=Center.Distance(Bot.Character.Position);
						  Vector3 AdjustedV3=MathEx.GetPointAt(Center, Distance*0.75f, FindDirection(Center, Bot.Character.Position, true));
						  return new Ability(SNOPower.Monk_WaveOfLight, 16f, AdjustedV3, Bot.Character.iCurrentWorldID, -1, 2, 2, USE_SLOWLY);
					 }
				} 
				#endregion
				#region tempest rush
				// For tempest rush re-use
				if (!bOOCBuff&&Bot.Character.dCurrentEnergy>=15&&HotbarAbilitiesContainsPower(SNOPower.Monk_TempestRush)&&
					 AbilityLastUseMS(SNOPower.Monk_TempestRush)<150&&Bot.Combat.iAnythingWithinRange[RANGE_50]>0&&thisCacheUnitObj!=null)
				{
					 float fExtraDistance=Bot.Target.CurrentTarget.CentreDistance<=20f?5f:1f;
					 Bot.Combat.vSideToSideTarget=FindZigZagTargetLocation(Bot.Target.CurrentTarget.Position, Bot.Target.CurrentTarget.CentreDistance+fExtraDistance);
					 // Resetting this to ensure the "no-spam" is reset since we changed our target location
					 Bot.Combat.powerLastSnoPowerUsed=SNOPower.None;
					 Bot.Combat.iACDGUIDLastWhirlwind=Bot.Target.CurrentTarget.AcdGuid.Value;
					 Bot.Combat.lastChangedZigZag=DateTime.Now;
					 return new Ability(SNOPower.Monk_TempestRush, 23f, Bot.Combat.vSideToSideTarget, Bot.Character.iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
				}

				
				// Tempest rush at elites or groups of mobs
				if (!bOOCBuff&&!bCurrentlyAvoiding&&!Bot.Character.bIsIncapacitated&&!Bot.Character.bIsRooted&&
					(Bot.Combat.iElitesWithinRange[RANGE_25]>0||(thisCacheUnitObj!=null&&(thisCacheUnitObj.IsEliteRareUnique||Bot.Target.CurrentTarget.IsBoss)&&Bot.Target.CurrentTarget.RadiusDistance<=14f)||Bot.Combat.iAnythingWithinRange[RANGE_15]>2)&&
					HotbarAbilitiesContainsPower(SNOPower.Monk_TempestRush)&&((Bot.Character.dCurrentEnergy>=20&&!Bot.Character.bWaitingForReserveEnergy)||Bot.Character.dCurrentEnergy>=Bot.Class.iWaitingReservedAmount)&&
					PowerManager.CanCast(SNOPower.Monk_TempestRush))
				{
					 bool bGenerateNewZigZag=(DateTime.Now.Subtract(Bot.Combat.lastChangedZigZag).TotalMilliseconds>=1500||
						 (Bot.Combat.vPositionLastZigZagCheck!=vNullLocation&&Bot.Character.Position==Bot.Combat.vPositionLastZigZagCheck&&DateTime.Now.Subtract(Bot.Combat.lastChangedZigZag).TotalMilliseconds>=200)||
						 Vector3.Distance(Bot.Character.Position, Bot.Combat.vSideToSideTarget)<=4f||
						 Bot.Target.CurrentTarget.AcdGuid.Value!=Bot.Combat.iACDGUIDLastWhirlwind);
					 Bot.Combat.vPositionLastZigZagCheck=Bot.Character.Position;
					 if (bGenerateNewZigZag)
					 {
						  float fExtraDistance=Bot.Target.CurrentTarget.CentreDistance<=20f?5f:1f;
						  Bot.Combat.vSideToSideTarget=FindZigZagTargetLocation(Bot.Target.CurrentTarget.Position, Bot.Target.CurrentTarget.CentreDistance+fExtraDistance);
						  // Resetting this to ensure the "no-spam" is reset since we changed our target location
						  Bot.Combat.powerLastSnoPowerUsed=SNOPower.None;
						  Bot.Combat.iACDGUIDLastWhirlwind=Bot.Target.CurrentTarget.AcdGuid.Value;
						  Bot.Combat.lastChangedZigZag=DateTime.Now;
					 }
					 return new Ability(SNOPower.Monk_TempestRush, 23f, Bot.Combat.vSideToSideTarget, Bot.Character.iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
				} 
				#endregion
				#region Dashing Strike
				// Dashing Strike
				if (!bOOCBuff&&!bCurrentlyAvoiding&&!Bot.Character.bIsIncapacitated&&
					(Bot.Combat.iElitesWithinRange[RANGE_25]>0||((thisCacheUnitObj!=null&&thisCacheUnitObj.IsEliteRareUnique||Bot.Target.CurrentTarget.IsBoss)&&Bot.Target.CurrentTarget.RadiusDistance<=14f)||Bot.Combat.iAnythingWithinRange[RANGE_15]>2)&&
					HotbarAbilitiesContainsPower(SNOPower.Monk_DashingStrike)&&((Bot.Character.dCurrentEnergy>=30&&!Bot.Character.bWaitingForReserveEnergy)||Bot.Character.dCurrentEnergy>=Bot.Class.iWaitingReservedAmount))
				{
					 return new Ability(SNOPower.Monk_DashingStrike, 14f, vNullLocation, -1, Bot.Target.CurrentTarget.AcdGuid.Value, 0, 1, USE_SLOWLY);
				} 
				#endregion

				#region Fists of thunder
				// Fists of thunder as the primary, repeatable attack
				if (!bOOCBuff&&!bCurrentlyAvoiding&&HotbarAbilitiesContainsPower(SNOPower.Monk_FistsofThunder))
				{
					 return new Ability(SNOPower.Monk_FistsofThunder, 30f, vNullLocation, -1, Bot.Target.CurrentTarget.AcdGuid.Value, 0, 1, SIGNATURE_SPAM);
				} 
				#endregion
				#region Deadly reach
				// Deadly reach
				if (!bOOCBuff&&!bCurrentlyAvoiding&&HotbarAbilitiesContainsPower(SNOPower.Monk_DeadlyReach))
				{
					 return new Ability(SNOPower.Monk_DeadlyReach, 16f, vNullLocation, -1, Bot.Target.CurrentTarget.AcdGuid.Value, 0, 1, USE_SLOWLY);
				} 
				#endregion
				#region Crippling wave
				// Crippling wave
				if (!bOOCBuff&&!bCurrentlyAvoiding&&HotbarAbilitiesContainsPower(SNOPower.Monk_CripplingWave))
				{
					 return new Ability(SNOPower.Monk_CripplingWave, 14f, vNullLocation, -1, Bot.Target.CurrentTarget.AcdGuid.Value, 0, 1, USE_SLOWLY);
				} 
				#endregion
				#region Way of hundred fists
				// Way of hundred fists
				if (!bOOCBuff&&!bCurrentlyAvoiding&&HotbarAbilitiesContainsPower(SNOPower.Monk_WayOfTheHundredFists))
				{
					 return new Ability(SNOPower.Monk_WayOfTheHundredFists, 14f, vNullLocation, -1, Bot.Target.CurrentTarget.AcdGuid.Value, 0, 1, SIGNATURE_SPAM);
				} 
				#endregion
				#region Default attacks
				// Default attacks
				if (!bOOCBuff&&!bCurrentlyAvoiding&&!Bot.Character.bIsIncapacitated)
				{
					 return new Ability(SNOPower.Weapon_Melee_Instant, 10f, vNullLocation, -1, Bot.Target.CurrentTarget.AcdGuid.Value, 1, 1, USE_SLOWLY);
				} 
				#endregion
				return new Ability(SNOPower.None, 0, vNullLocation, -1, -1, 0, 0, false);
		
		  }
	 }
}