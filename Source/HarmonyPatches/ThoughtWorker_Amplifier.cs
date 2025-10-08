using HarmonyLib;
using RimWorld;
using Verse;

namespace FamilyTies
{
    [HarmonyPatch(typeof(ThoughtWorker), nameof(ThoughtWorker.MoodMultiplier))]
    public static class ThoughtWorker_MoodMultiplier_Patch
    {
        public static void Postfix(ThoughtWorker __instance, Pawn p, ref float __result)
        {
            if (!p.story.traits.HasTrait(FamilyTiesDefOf.FamilyPerson)) return;

            var amplifierDef = FamilyTiesDefOf.FamilyPerson_ThoughtAmplifier;

            if (amplifierDef?.amplifiers == null) return;

            foreach (var entry in amplifierDef.amplifiers)
            {
                if (entry.thoughtDef == __instance.def.defName)
                {
                    __result *= entry.multiplier;
                    
                    break;
                }
            }
        }
    }
}

