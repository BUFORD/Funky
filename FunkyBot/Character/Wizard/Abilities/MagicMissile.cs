﻿using System;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace FunkyBot.AbilityFunky.Abilities.Wizard
{
	 public class MagicMissile : Ability, IAbility
	 {
		  public MagicMissile()
				: base()
		  {
		  }



		  public override void Initialize()
		  {
				Cooldown=5;
				ExecutionType=AbilityExecuteFlags.Target;
				WaitVars=new WaitLoops(0, 1, true);
				Range=40;
				IsRanged=true;
				IsProjectile=true;
				UseageType=AbilityUseage.Combat;
				Priority=AbilityPriority.None;
				PreCastFlags=(AbilityPreCastFlags.CheckPlayerIncapacitated);
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
				get { return SNOPower.Wizard_MagicMissile; }
		  }
	 }
}