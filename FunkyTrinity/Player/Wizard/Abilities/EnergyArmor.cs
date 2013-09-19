﻿using System;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace FunkyTrinity.Ability.Abilities.Wizard
{
	public class EnergyArmor : ability, IAbility
	{
		public EnergyArmor() : base()
		{
		}



		public override void Initialize()
		{
			 ExecutionType=AbilityUseType.Buff;
			 WaitVars=new WaitLoops(1, 2, true);
			 Cost=25;
			 Counter=1;
			 UseageType=AbilityUseage.Anywhere;
			 IsBuff=true;
			 Priority=AbilityPriority.High;
			 PreCastConditions=(AbilityConditions.CheckPlayerIncapacitated|AbilityConditions.CheckEnergy|
														AbilityConditions.CheckExisitingBuff);
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
				ability p = (ability) obj;
				return this.Power == p.Power;
			}
		}

		#endregion

		public override SNOPower Power
		{
			get { return SNOPower.Wizard_EnergyArmor; }
		}
	}
}
