using HarmonyLib;
using RimWorld;
using Verse;

namespace FamilyTies
{
    [HarmonyPatch(typeof(Thought_Memory), nameof(Thought_Memory.MoodOffset))]
    public static class ThoughtAmplifier_Patch
    {
        public static void Postfix(Thought_Memory __instance, ref float __result)
        {
            Pawn pawn = __instance.pawn;

            if (pawn == null || __instance.def == null) return;

            if (!pawn.story?.traits?.HasTrait(FamilyTiesDefOf.FamilyPerson) ?? true) return;

            var amplifier = FamilyTiesDefOf.FamilyPerson_ThoughtAmplifier;

            if (amplifier?.amplifiers == null) return;

            foreach (var entry in amplifier.amplifiers)
            {
                if (entry.thoughtDef == __instance.def.defName)
                {
                    if (entry.onlyIfPositive && __result <= 0) return;
                    if (entry.onlyIfNegative && __result >= 0) return;

                    __result *= entry.multiplier;

                    break;
                }
            }
        }
    }
}

