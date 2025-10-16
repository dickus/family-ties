using RimWorld;
using HarmonyLib;
using Verse;
using System.Linq;
using System.Collections.Generic;

namespace FamilyTies
{
    public static class LimbRestored_Patch
    {
        public static void Postfix(Hediff __instance, Pawn ___pawn)
        {
            if (!(__instance is Hediff_MissingPart)) return;

            Pawn child = ___pawn;

            if (child == null || child.relations == null) return;

            IEnumerable<Pawn> parents = child.relations.DirectRelations
                .Where(r => r.def == PawnRelationDefOf.Parent)
                .Select(r => r.otherPawn);

            if (!parents.Any()) return;

            ThoughtDef thoughtDefToRemove = (child.gender == Gender.Male)
                ? ThoughtDef.Named("MySonLostLimb")
                : ThoughtDef.Named("MyDaughterLostLimb");

            foreach (var parent in parents)
            {
                if (parent != null && !parent.Dead && parent.needs?.mood?.thoughts?.memories != null)
                {
                    Thought_Memory thought = parent.needs.mood.thoughts.memories.GetFirstMemoryOfDef(thoughtDefToRemove);

                    if (thought != null) parent.needs.mood.thoughts.memories.RemoveMemory(thought);
                }
            }
        }
    }
}

