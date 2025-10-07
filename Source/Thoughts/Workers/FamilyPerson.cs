using RimWorld;
using Verse;
using System.Collections.Generic;

namespace FamilyTies
{
    public class ThoughtWorker_FamilyPersonMoodIndependent : ThoughtWorker
    {
        private const int CheckInterval = 180;
        private static Dictionary<Pawn, int> pawnChildrenCounts = new Dictionary<Pawn, int>();

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p == null || !p.Spawned || p.Dead || p.Downed || p.story == null) return ThoughtState.Inactive;

            if (!p.story.traits.HasTrait(FamilyTiesDefOf.FamilyPerson)) return ThoughtState.Inactive;

            int childrenCount = GetChildrenCountAround(p);

            if (childrenCount == 1) return ThoughtState.ActiveAtStage(0);
            if (childrenCount >= 2) return ThoughtState.ActiveAtStage(1);

            return ThoughtState.Inactive;
        }

        private int GetChildrenCountAround(Pawn pawn)
        {
            if (pawn.Map == null) return 0;

            int currentChildrenCount = 0;

            foreach (Thing thing in GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, 7f, true))
            {
                if (thing is Pawn otherPawn && otherPawn != pawn && otherPawn.Spawned && otherPawn.RaceProps.Humanlike)
                {
                    if (!otherPawn.DevelopmentalStage.Adult()) currentChildrenCount++;
                }
            }

            pawnChildrenCounts[pawn] = currentChildrenCount;

            return pawnChildrenCounts[pawn];
        }
    }
}

