using RimWorld;
using HarmonyLib;
using Verse;

namespace FamilyTies
{
    public static class ConstructFinish_Patch
    {
        public static void Prefix(Frame __instance, ref (Map map, IntVec3 pos, BuildableDef buildable) __state)
        {
            __state = (__instance.Map, __instance.Position, __instance.def.entityDefToBuild);
        }

        public static void Postfix(Pawn worker, (Map map, IntVec3 pos, BuildableDef buildable) __state)
        {
            if (!FamilyTiesMod.settings.proudForMasterpiece) return;

            if (worker == null || __state.map == null) return;

            ThingDef thingDef = __state.buildable as ThingDef;

            if (thingDef == null) return;

            Thing product = __state.pos.GetFirstThing(__state.map, thingDef);

            MasterpieceUtil.CheckAndRewardParent(worker, product);
        }
    }
}

