using RimWorld;
using Verse;
using System.Collections.Generic;

namespace FamilyTies
{
    public class ThoughtWorker_FamilyPersonMoodIndependent_Family : ThoughtWorker
    {
        private const int CheckInterval = 180;
        private static Dictionary<Pawn, int> pawnFamilyMembersCount = new Dictionary<Pawn, int>();

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p == null || !p.Spawned || p.Dead || p.Downed || p.story == null) return ThoughtState.Inactive;

            if (!p.story.traits.HasTrait(FamilyTiesDefOf.FamilyPerson)) return ThoughtState.Inactive;

            int familyMembersCount = GetFamilyMembersCountAround(p);

            if (familyMembersCount == 1) return ThoughtState.ActiveAtStage(0);
            if (familyMembersCount >= 2) return ThoughtState.ActiveAtStage(1);

            return ThoughtState.Inactive;
        }

        private int GetFamilyMembersCountAround(Pawn pawn)
        {
            if (pawn.Map == null) return 0;

            int currentFamilyMembersCount = 0;

            foreach (Thing thing in GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, 10f, true))
            {
                if (thing is Pawn otherPawn && otherPawn != pawn && otherPawn.Spawned && otherPawn.RaceProps.Humanlike)
                {
                    if (IsFamilyMember(pawn, otherPawn)) currentFamilyMembersCount++;
                }
            }

            pawnFamilyMembersCount[pawn] = currentFamilyMembersCount;

            return pawnFamilyMembersCount[pawn];
        }

        private bool IsFamilyMember(Pawn pawn, Pawn otherPawn)
        {
            foreach (DirectPawnRelation rel in pawn.relations.DirectRelations)
            {
                if (rel.otherPawn == otherPawn) return true;
            }

            foreach (DirectPawnRelation rel in otherPawn.relations.DirectRelations)
            {
                if (rel.otherPawn == pawn) return true;
            }

            return false;
        }
    }
}

