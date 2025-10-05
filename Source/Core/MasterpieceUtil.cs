using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;

namespace FamilyTies
{
    public static class MasterpieceUtil
    {
        public static void CheckAndRewardParent(Pawn child, Thing product)
        {
            if (child == null || product == null) return;

            if (product.TryGetQuality(out QualityCategory qc) && (qc == QualityCategory.Masterwork || qc == QualityCategory.Legendary))
            {
                int ageLimit = FamilyTiesMod.settings.ageOfCaring;

                if (ageLimit != 0 && child.ageTracker.AgeBiologicalYears > ageLimit) return;

                IEnumerable<Pawn> parents = child.relations?.DirectRelations.Where(r => r.def == PawnRelationDefOf.Parent).Select(r => r.otherPawn);

                if (parents == null || !parents.Any()) return;

                foreach (var parent in parents)
                {
                    if (!PawnRelationUtil.HasGoodRelation(parent, child)) continue;

                    if (parent != null && !parent.Dead && !TraitUtil.IsUnempathetic(parent, true)) parent.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("MyChildCreatedMasterpiece"));
                }
            }
        }
    }
}

