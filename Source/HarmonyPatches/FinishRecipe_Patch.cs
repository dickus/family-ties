using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;

namespace FamilyTies
{
    public static class FinishRecipe_Patch
    {
        public static void Postfix(IEnumerable<Thing> __result, Pawn worker)
        {
            if (!FamilyTiesMod.settings.proudForMasterpiece) return;

            foreach (var product in __result)
            {
                MasterpieceUtil.CheckAndRewardParent(worker, product);
            }
        }
    }
}

