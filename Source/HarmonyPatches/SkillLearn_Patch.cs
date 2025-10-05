using RimWorld;
using HarmonyLib;
using Verse;
using System.Linq;
using System.Collections.Generic;

namespace FamilyTies
{
    public static class SkillLearn_Patch
    {
        public static void Prefix(SkillRecord __instance, ref int __state, Pawn ___pawn)
        {
            __state = __instance.Level;
        }

        public static void Postfix(SkillRecord __instance, int __state, Pawn ___pawn, float xp, bool direct, bool ignoreLearnRate)
        {
            if (!FamilyTiesMod.settings.proudForSkillUp) return;

            if (__instance.Level > __state)
            {
                Pawn child = ___pawn;

                if (child == null) return;

                int ageLimit = FamilyTiesMod.settings.ageOfCaring;
                bool isAgeOk = (ageLimit == 0 || child.ageTracker.AgeBiologicalYears <= ageLimit);

                if (!isAgeOk) return;

                IEnumerable<Pawn> parents = child.relations?.DirectRelations.Where(r => r.def == PawnRelationDefOf.Parent && r.otherPawn != null).Select(r => r.otherPawn);

                if (parents == null || !parents.Any()) return;

                foreach (var parent in parents)
                {
                    if (!PawnRelationUtil.HasGoodRelation(parent, child)) continue;

                    if (parent != null && !parent.Dead && !TraitUtil.IsUnempathetic(parent, true)) parent.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("MyChildLeveledUpSkill"));
                }
            }
        }
    }
}

