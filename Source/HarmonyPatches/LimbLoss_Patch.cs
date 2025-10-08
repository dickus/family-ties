using RimWorld;
using HarmonyLib;
using Verse;
using System.Linq;
using System.Collections.Generic;

namespace FamilyTies
{
    public static class LimbLoss_Patch
    {
        public static void Postfix(Pawn_HealthTracker __instance, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo, DamageWorker.DamageResult result)
        {
            if (!(hediff is Hediff_MissingPart)) return;

            Pawn child = hediff.pawn;

            if (child == null || child.relations == null) return;

            int ageLimit = FamilyTiesMod.settings.ageOfCaring;

            if (ageLimit != 0 && child.ageTracker.AgeBiologicalYears > ageLimit) return;

            IEnumerable<Pawn> parents = child.relations.DirectRelations.Where(r => r.def == PawnRelationDefOf.Parent).Select(r => r.otherPawn);

            if (!parents.Any()) return;

            foreach (var parent in parents)
            {
                if (!PawnRelationUtil.HasGoodRelation(parent, child)) continue;

                if (parent != null && !parent.Dead && !TraitUtil.IsUnempathetic(parent))
                {
                    ThoughtDef thoughtDef = null;
                    Thought_Memory thought = null;

                    if (child.gender == Gender.Male)
                    {
                        thoughtDef = ThoughtDef.Named("MySonLostLimb");
                    }
                    else
                    {
                        thoughtDef = ThoughtDef.Named("MyDaughterLostLimb");
                    }

                    if (FamilyPersonUtil.IsFamilyPerson(parent))
                    {
                        thought = (Thought_Memory)ThoughtMaker.MakeThought(thoughtDef, 1);
                    }
                    else
                    {
                        thought = (Thought_Memory)ThoughtMaker.MakeThought(thoughtDef, 0);
                    }

                    parent.needs.mood.thoughts.memories.TryGainMemory(thought, child);
                }
            }
        }
    }
}

