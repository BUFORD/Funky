using Zeta.Internals.Actors;

namespace FunkyTrinity.Ability.Abilities.Barb
{
	public class Warcry : ability, IAbility
	{
        public Warcry()
            : base()
		{
		}

		public override SNOPower Power
		{
			get { return SNOPower.Barbarian_WarCry; }
		}

        public override int RuneIndex { get { return Bot.Class.RuneIndexCache.ContainsKey(this.Power) ? Bot.Class.RuneIndexCache[this.Power] : -1; } }

		public override void Initialize()
		{
			ExecutionType = AbilityExecuteFlags.Buff;
			WaitVars = new WaitLoops(1, 1, true);
			Cost = 0;
			Range = 0;
            IsBuff = true;
			UseageType = AbilityUseage.Anywhere;
            Priority = AbilityPriority.Highest;
            PreCastPreCastFlags = (AbilityPreCastFlags.CheckCanCast | AbilityPreCastFlags.CheckPlayerIncapacitated);
		}

		#region IAbility
		public override int GetHashCode()
		{
			 return (int)this.Power;
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
					ability p=(ability)obj;
					return this.Power==p.Power;
			 }
		}
	

		#endregion
	}
}
