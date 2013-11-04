﻿using System;

using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace FunkyBot.AbilityFunky.Abilities.Wizard
{
	 public class DiamondSkin : Ability, IAbility
	 {
		  public DiamondSkin()
				: base()
		  {
		  }



		  public override void Initialize()
		  {
				Cooldown=15000;
				ExecutionType=AbilityExecuteFlags.Buff;
				WaitVars=new WaitLoops(0, 1, true);
				Cost=10;
				Counter=1;
				Range=0;
				UseageType=AbilityUseage.Anywhere;
				Priority=AbilityPriority.High;
				PreCastFlags=(AbilityPreCastFlags.CheckCanCast|
											AbilityPreCastFlags.CheckExisitingBuff);

				FcriteriaCombat=new Func<bool>(() =>
				{
					 return (Bot.Targeting.Environment.iElitesWithinRange[(int)RangeIntervals.Range_25]>0||
								Bot.Targeting.Environment.iAnythingWithinRange[(int)RangeIntervals.Range_25]>0||
								Bot.Character.dCurrentHealthPct<=0.90||Bot.Character.bIsIncapacitated||Bot.Character.bIsRooted||
								(Bot.Targeting.CurrentTarget.RadiusDistance<=40f));
				});
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
				get { return SNOPower.Wizard_DiamondSkin; }
		  }
	 }
}
