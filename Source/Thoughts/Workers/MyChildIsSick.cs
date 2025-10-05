using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;

namespace FamilyTies
{
    public class ThoughtWorker_MyChildIsSick : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (TraitUtil.IsUnempathetic(p)) return ThoughtState.Inactive;
            if (p.Map == null) return ThoughtState.Inactive;

            List<Pawn> sickChildren = new List<Pawn>();
            List<Pawn> allPawnsOnMap = p.Map.mapPawns.AllPawns;

            foreach (Pawn otherPawn in allPawnsOnMap)
            {
                if (otherPawn == p || otherPawn.relations == null) continue;

                if (otherPawn.relations.DirectRelations.Any(rel => rel.def == PawnRelationDefOf.Parent && rel.otherPawn == p))
                {
                    Pawn child = otherPawn;
                    int ageLimit = FamilyTiesMod.settings.ageOfCaring;

                    if (ageLimit == 0 || child.ageTracker.AgeBiologicalYears <= ageLimit)
                    {
                        if (SickPawnUtil.IsSick(child)) sickChildren.Add(child);
                    }
                }
            }

            if (sickChildren.Count == 1)
            {
                Pawn theChild = sickChildren.First();

                if (SickPawnUtil.IsCriticallyIll(theChild))
                {
                    return ThoughtState.ActiveAtStage(1);
                }
                else
                {
                    return ThoughtState.ActiveAtStage(0);
                }
            }
            else if (sickChildren.Count > 1)
            {
                if (sickChildren.All(child => SickPawnUtil.IsCriticallyIll(child)))
                {
                    return ThoughtState.ActiveAtStage(3);
                }
                else
                {
                    return ThoughtState.ActiveAtStage(2);
                }
            }

            return ThoughtState.Inactive;
        }
    }
}

