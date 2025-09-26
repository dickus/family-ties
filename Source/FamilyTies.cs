using HarmonyLib;
using RimWorld;
using Verse;
using System;
using System.Linq;
using System.Collections.Generic;

namespace FamilyTies
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("com.dickus.familyties");
            var targetMethod = AccessTools.Method(typeof(Thing), "TakeDamage", new Type[] { typeof(DamageInfo) });
            if (targetMethod != null) {
                var postfix = new HarmonyMethod(typeof(Pawn_TakeDamage_Patch), nameof(Pawn_TakeDamage_Patch.Postfix));
                harmony.Patch(targetMethod, postfix: postfix);
            }
        }
    }

    public static class Pawn_TakeDamage_Patch
    {
        public static void Postfix(Thing __instance, DamageInfo dinfo)
        {
             if (!(__instance is Pawn victim)) return;
             try {
                if (dinfo.Amount <= 0) return;
                if (!(dinfo.Instigator is Pawn attacker) || attacker == victim) return;
                if (!victim.RaceProps.Humanlike || victim.relations == null) return;

                if (victim.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Parent) == attacker)
                {
                    attacker.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("HarmedMyOwnChild"));
                }

                IEnumerable<Pawn> parents = victim.relations.DirectRelations.Where(r => r.def == PawnRelationDefOf.Parent).Select(r => r.otherPawn);
                if (!parents.Any()) return;
                foreach (var parent in parents) {
                    if (parent != null && !parent.Dead && parent != attacker) {
                        parent.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("HarmedMyChild"), attacker);
                    }
                }
            } catch (Exception) { }
        }
    }

    public class Thought_HarmedMyOwnChild : Thought_Memory
    {
        public override string LabelCap
        {
            get
            {
                if (this.pawn.gender == Gender.Male)
                {
                    return "HarmedMyOwnChild_Male".Translate();
                }
                else
                {
                    return "HarmedMyOwnChild_Female".Translate();
                }
            }
        }
    }

    public class ThoughtWorker_MyChildIsInPain : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.Map == null) return ThoughtState.Inactive;

            int sufferingChildrenCount = 0;
            Pawn firstSufferingChild = null;

            List<Pawn> allPawnsOnMap = p.Map.mapPawns.AllPawns;

            foreach (Pawn otherPawn in allPawnsOnMap)
            {
                if (otherPawn == p || otherPawn.relations == null) continue;

                Pawn parent = otherPawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Parent);

                if (parent == p)
                {
                    Pawn child = otherPawn;

                    if (!child.Dead && child.health.hediffSet.PainTotal > 0.01f)
                    {
                        sufferingChildrenCount++;
                        if (firstSufferingChild == null)
                        {
                            firstSufferingChild = child;
                        }
                    }
                }
            }

            if (sufferingChildrenCount == 1)
            {
                return ThoughtState.ActiveAtStage(0);
            }
            else if (sufferingChildrenCount > 1)
            {
                return ThoughtState.ActiveAtStage(1);
            }

            return ThoughtState.Inactive;
        }
    }
}
