﻿using System;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace FunkyTrinity.AbilityFunky.Abilities.DemonHunter
{
	public class ElementalArrow : Ability, IAbility
	{
		public ElementalArrow() : base()
		{
		}



		public override void Initialize()
		{
			ExecutionType = AbilityExecuteFlags.ClusterTarget | AbilityExecuteFlags.Target;
			WaitVars = new WaitLoops(0, 1, true);
			Cost = 10;
			Range = 48;
			IsRanged = true;
			IsProjectile=true;
			UseageType=AbilityUseage.Combat;
			Priority = AbilityPriority.Low;
			PreCastFlags = (AbilityPreCastFlags.CheckPlayerIncapacitated | AbilityPreCastFlags.CheckEnergy);
			ClusterConditions = new ClusterConditions(4d, 40, 2, true);
			FcriteriaCombat = new Func<bool>(() =>
			{
				return (!Bot.Target.CurrentTarget.IsTreasureGoblin &&
				        Bot.Target.CurrentTarget.SNOID != 5208 && Bot.Target.CurrentTarget.SNOID != 5209 &&
				        Bot.Target.CurrentTarget.SNOID != 5210);
			});
		}

		#region IAbility

		public override int RuneIndex
		{
			get { return Bot.Class.RuneIndexCache.ContainsKey(this.Power) ? Bot.Class.RuneIndexCache[this.Power] : -1; }
		}

		public override int GetHashCode()
		{
			return (int) this.Power;
		}

		public override bool Equals(object obj)
		{
			//Check for null and compare run-time types. 
			if (obj == null || this.GetType() != obj.GetType())
			{
				return false;
			}
			else
			{
				Ability p = (Ability) obj;
				return this.Power == p.Power;
			}
		}

		#endregion

		public override SNOPower Power
		{
			get { return SNOPower.DemonHunter_ElementalArrow; }
		}
	}
}
