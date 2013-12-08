﻿using FunkyBot.Cache;
using Zeta.Internals.Actors;

namespace FunkyBot.Player.HotBar.Skills.Wizard
{
	 public class EnergyTwister : Skill
	 {
		 public override void Initialize()
		  {
				Cooldown=5;
				ExecutionType=AbilityExecuteFlags.Location;
				WaitVars=new WaitLoops(0, 0, true);
				Cost=35;
				Range=UsingCriticalMass()?9:28;
				IsRanged=true;
				UseageType=AbilityUseage.Combat;
				Priority=AbilityPriority.Low;
				PreCastFlags=(AbilityPreCastFlags.CheckPlayerIncapacitated|AbilityPreCastFlags.CheckEnergy|
											AbilityPreCastFlags.CheckCanCast);

				FcriteriaCombat=() => (!HasSignatureAbility()||Bot.Character.Class.HotBar.GetBuffStacks(SNOPower.Wizard_EnergyTwister)<1)&&
				                      (Bot.Targeting.Environment.iElitesWithinRange[(int)RangeIntervals.Range_30]>=1||
				                       Bot.Targeting.Environment.iAnythingWithinRange[(int)RangeIntervals.Range_25]>=1||
				                       Bot.Targeting.CurrentTarget.RadiusDistance<=12f)&&
				                      (!Bot.Character.Class.HotBar.HotbarPowers.Contains(SNOPower.Wizard_Electrocute)||
				                       !CacheIDLookup.hashActorSNOFastMobs.Contains(Bot.Targeting.CurrentTarget.SNOID))&&
				                      ((UsingCriticalMass()&&(!HasSignatureAbility()||Bot.Character.Data.dCurrentEnergy>=35))||
				                       (!UsingCriticalMass()&&Bot.Character.Data.dCurrentEnergy>=35));
		  }

		  private bool HasSignatureAbility()
		  {
				return (Bot.Character.Class.HotBar.HotbarPowers.Contains(SNOPower.Wizard_MagicMissile)||Bot.Character.Class.HotBar.HotbarPowers.Contains(SNOPower.Wizard_ShockPulse)||
										Bot.Character.Class.HotBar.HotbarPowers.Contains(SNOPower.Wizard_SpectralBlade)||Bot.Character.Class.HotBar.HotbarPowers.Contains(SNOPower.Wizard_Electrocute));
		  }
		  private bool UsingCriticalMass()
		  {
				return Bot.Character.Class.HotBar.PassivePowers.Contains(SNOPower.Wizard_Passive_CriticalMass);
		  }

		  #region IAbility

		  public override int RuneIndex
		  {
				get { return Bot.Character.Class.HotBar.RuneIndexCache.ContainsKey(Power)?Bot.Character.Class.HotBar.RuneIndexCache[Power]:-1; }
		  }

		  public override int GetHashCode()
		  {
				return (int)Power;
		  }

		  public override bool Equals(object obj)
		  {
				//Check for null and compare run-time types. 
				if (obj==null||GetType()!=obj.GetType())
				{
					 return false;
				}
			  Skill p=(Skill)obj;
			  return Power==p.Power;
		  }

		  #endregion

		  public override SNOPower Power
		  {
				get { return SNOPower.Wizard_EnergyTwister; }
		  }
	 }
}
