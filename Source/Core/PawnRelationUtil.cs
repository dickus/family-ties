using RimWorld;
using Verse;

namespace FamilyTies
{
    public static class PawnRelationUtil
    {
        public static bool HasGoodRelation(Pawn p1, Pawn p2, float minOpinion = 30f)
        {
            if (p1 == null || p2 == null) return false;

            return p1.relations.OpinionOf(p2) >= minOpinion;
        }
    }
}

