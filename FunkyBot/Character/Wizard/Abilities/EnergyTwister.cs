﻿using System;
using FunkyBot.Cache;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace FunkyBot.AbilityFunky.Abilities.Wizard
{
	 public class EnergyTwister : Ability, IAbility
	 {
		  public EnergyTwister()
				: base()
		  {
		  }



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

				FcriteriaCombat=new Func<bool>(() =>
				{
					 return (!HasSignatureAbility()||Bot.Class.HotBar.GetBuffStacks(SNOPower.Wizard_EnergyTwister)<1)&&
							  (Bot.Targeting.Environment.iElitesWithinRange[(int)RangeIntervals.Range_30]>=1||
								Bot.Targeting.Environment.iAnythingWithinRange[(int)RangeIntervals.Range_25]>=1||
								Bot.Targeting.CurrentTarget.RadiusDistance<=12f)&&
							  (!Bot.Class.HotBar.HotbarPowers.Contains(SNOPower.Wizard_Electrocute)||
								!CacheIDLookup.hashActorSNOFastMobs.Contains(Bot.Targeting.CurrentTarget.SNOID))&&
								  ((this.UsingCriticalMass()&&(!HasSignatureAbility()||Bot.Character.dCurrentEnergy>=35))||
									 (!this.UsingCriticalMass()&&Bot.Character.dCurrentEnergy>=35));
				});
		  }

		  private bool HasSignatureAbility()
		  {
				return (Bot.Class.HotBar.HotbarPowers.Contains(SNOPower.Wizard_MagicMissile)||Bot.Class.HotBar.HotbarPowers.Contains(SNOPower.Wizard_ShockPulse)||
										Bot.Class.HotBar.HotbarPowers.Contains(SNOPower.Wizard_SpectralBlade)||Bot.Class.HotBar.HotbarPowers.Contains(SNOPower.Wizard_Electrocute));
		  }
		  private bool UsingCriticalMass()
		  {
				return Bot.Class.HotBar.PassivePowers.Contains(SNOPower.Wizard_Passive_CriticalMass); ;
		  }

		  #region IAbility

		  public override int RuneIndex
		  {
				get { return Bot.Class.HotBar.RuneIndexCache.ContainsKey(this.Power)?Bot.Class.HotBar.RuneIndexCache[this.Power]:-1; }
		  }

		  public override int GetHashCode()
		  {
				return (int)this.Power;
		  }

		  public override bool Equals(object obj)
		  {
				//Check for null and compare run-time types. 
				if (obj==null||this.GetType()!=obj.GetType())
				{
					 return false;
				}
				else
				{
					 Ability p=(Ability)obj;
					 return this.Power==p.Power;
				}
		  }

		  #endregion

		  public override SNOPower Power
		  {
				get { return SNOPower.Wizard_EnergyTwister; }
		  }
	 }
}
