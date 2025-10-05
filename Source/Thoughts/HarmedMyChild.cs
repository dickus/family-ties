using RimWorld;
using Verse;

namespace FamilyTies
{
    public class Thought_HarmedMyChild : Thought_MemorySocial
    {
        public override string LabelCap
        {
            get
            {
                Pawn attacker = this.otherPawn;
                if (attacker == null) return base.LabelCap;

                if (attacker.gender == Gender.Male)
                {
                    return "HarmedMyChild_Male".Translate();
                }
                else
                {
                    return "HarmedMyChild_Female".Translate();
                }
            }
        }
    }
}

