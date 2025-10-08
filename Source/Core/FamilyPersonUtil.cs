using RimWorld;
using Verse;

namespace FamilyTies
{
    public static class FamilyPersonUtil
    {
        public static bool IsFamilyPerson(Pawn pawn)
        {
            return pawn.story.traits.HasTrait(FamilyTiesDefOf.FamilyPerson);
        }
    }
}

