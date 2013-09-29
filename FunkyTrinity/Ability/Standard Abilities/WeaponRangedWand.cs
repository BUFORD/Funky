﻿using System;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace FunkyTrinity.AbilityFunky.Abilities
{
	public class WeaponRangedWand : Ability, IAbility
	{
		public WeaponRangedWand() : base()
		{
		}

		public override SNOPower Power
		{
			 get { return SNOPower.Weapon_Ranged_Wand; }
		}

		public override void Initialize()
		{
			Range = 25;
			IsRanged=true;
			IsProjectile=true;
			Priority = AbilityPriority.None;
			ExecutionType = AbilityExecuteFlags.Target;
			WaitVars = new WaitLoops(0, 0, true);
			PreCastFlags=AbilityPreCastFlags.None;
			UseageType=AbilityUseage.Combat;
		}

		#region IAbility
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
	}
}
