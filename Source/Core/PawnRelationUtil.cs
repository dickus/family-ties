using RimWorld;
using Verse;

namespace FamilyTies
{
    public static class PawnRelationUtil
    {
        public static bool HasGoodRelation(Pawn p1, Pawn p2)
        {
            if (p1 == null || p2 == null) return false;

            return p1.relations.OpinionOf(p2) >= FamilyTiesMod.settings.opinionThreshold;
        }
    }
}

