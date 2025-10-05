using RimWorld;
using Verse;

namespace FamilyTies
{
    public class Thought_HarmedMyOwnChild : Thought_Memory
    {
        public override string LabelCap
        {
            get
            {
                if (this.pawn.gender == Gender.Male)
                {
                    return "HarmedMyOwnChild_Male".Translate();
                }
                else
                {
                    return "HarmedMyOwnChild_Female".Translate();
                }
            }
        }

        public override string Description
        {
            get
            {
                if (this.pawn.gender == Gender.Male)
                {
                    return "HarmedMyOwnChild_Desc_Male".Translate();
                }
                else
                {
                    return "HarmedMyOwnChild_Desc_Female".Translate();
                }
            }
        }

        public override float MoodOffset()
        {
            if (TraitUtil.IsUnempathetic(this.pawn))
            {
                this.pawn.needs.mood.thoughts.memories.RemoveMemory(this);

                return 0f;
            }

            return base.MoodOffset();
        }
    }
}

