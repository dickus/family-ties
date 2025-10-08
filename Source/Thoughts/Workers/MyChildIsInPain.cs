using RimWorld;
using Verse;
using System.Collections.Generic;

namespace FamilyTies
{
    public class ThoughtWorker_MyChildIsInPain : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (TraitUtil.IsUnempathetic(p)) return ThoughtState.Inactive;
            if (p.Map == null) return ThoughtState.Inactive;

            int suffereingChildrenCount = 0;
            List<Pawn> allPawnsOnMap = p.Map.mapPawns.AllPawns;

            foreach (Pawn otherPawn in allPawnsOnMap)
            {
                if (otherPawn == p || otherPawn.relations == null) continue;

                if (otherPawn.relations.DirectRelations.Any(rel => rel.def == PawnRelationDefOf.Parent && rel.otherPawn == p))
                {
                    Pawn child = otherPawn;

                    if (!PawnRelationUtil.HasGoodRelation(p, child)) continue;

                    int ageLimit = FamilyTiesMod.settings.ageOfCaring;

                    if (ageLimit == 0 || child.ageTracker.AgeBiologicalYears <= ageLimit)
                    {
                        if (!child.Dead && child.health.hediffSet.PainTotal > 0.01f) suffereingChildrenCount++;
                    }
                }
            }

            if (suffereingChildrenCount == 1)
            {
                if (FamilyPersonUtil.IsFamilyPerson(p))
                {
                    return ThoughtState.ActiveAtStage(2);
                }
                else
                {
                    return ThoughtState.ActiveAtStage(0);
                }
            }
            if (suffereingChildrenCount > 1)
            {
                if (FamilyPersonUtil.IsFamilyPerson(p))
                {
                    return ThoughtState.ActiveAtStage(3);
                }
                else
                {
                    return ThoughtState.ActiveAtStage(1);
                }
            }

            return ThoughtState.Inactive;
        }
    }
}

