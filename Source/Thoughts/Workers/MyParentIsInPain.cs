using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;

namespace FamilyTies
{
    public class ThoughtWorker_MyParentIsInPain : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!FamilyTiesMod.settings.childrenWorryAboutParents) return ThoughtState.Inactive;

            int currentAge = p.ageTracker.AgeBiologicalYears;
            int minAge = FamilyTiesMod.settings.childMinEmpathyAge;
            int maxAge = FamilyTiesMod.settings.childMaxEmpathyAge;

            if (currentAge < minAge) return ThoughtState.Inactive;
            if (maxAge != 0 && currentAge > maxAge) return ThoughtState.Inactive;

            if (p.ageTracker.AgeBiologicalYears < FamilyTiesMod.settings.childMinEmpathyAge) return ThoughtState.Inactive;
            if (TraitUtil.IsUnempathetic(p)) return ThoughtState.Inactive;
            if (p.Map == null) return ThoughtState.Inactive;

            int sufferingParentsCount = 0;

            IEnumerable<Pawn> parents = p.relations.DirectRelations.Where(r => r.def == PawnRelationDefOf.Parent).Select(r => r.otherPawn);

            foreach (var parent in parents)
            {
                if (!PawnRelationUtil.HasGoodRelation(p, parent)) continue;

                float painThreshold = StoicismUtil.GetPainThreshold(parent);

                if (parent.health.hediffSet.PainTotal >= painThreshold) sufferingParentsCount++;
            }

            if (sufferingParentsCount == 1)
            {
                return ThoughtState.ActiveAtStage(0);
            }
            else if (sufferingParentsCount > 1)
            {
                return ThoughtState.ActiveAtStage(1);
            }

            return ThoughtState.Inactive;
        }
    }
}

