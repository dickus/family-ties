using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;

namespace FamilyTies
{
    public class ThoughtWorker_MyParentIsSick : ThoughtWorker
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

            int sickParentsCount = 0;

            IEnumerable<Pawn> parents = p.relations.DirectRelations.Where(r => r.def == PawnRelationDefOf.Parent).Select(r => r.otherPawn);

            foreach (var parent in parents)
            {
                if (!PawnRelationUtil.HasGoodRelation(parent, p)) continue;

                float sicknessThreshold = StoicismUtil.GetSicknessThreshold(parent);

                if (IsSickAboveThreshold(parent, sicknessThreshold)) sickParentsCount++;
            }

            if (sickParentsCount == 1)
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
            else if (sickParentsCount > 1)
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

        private bool IsSickAboveThreshold(Pawn pawn, float threshold)
        {
            return pawn.health.hediffSet.hediffs.Any(h => (h.def.HasComp(typeof(HediffComp_Immunizable)) || h.def.makesSickThought || h.def.HasComp(typeof(HediffComp_TendDuration))) && h.Severity >= threshold);
        }
    }
}

