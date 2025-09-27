using HarmonyLib;
using RimWorld;
using Verse;
using System;
using System.Linq;
using System.Collections.Generic;

namespace FamilyTies
{
    public static class TraitUtil
    {
        public static bool IsUnempathetic(Pawn pawn)
        {
            if (pawn?.story?.traits == null) return false;
            if (pawn.story.traits.HasTrait(TraitDefOf.Psychopath) ||
                pawn.story.traits.HasTrait(TraitDefOf.Bloodlust)) return true;
            if (ModLister.IdeologyInstalled)
            {
                if (pawn.story.traits.HasTrait(TraitDef.Named("Cannibal"))) return true;
            }
            return false;
        }
    }

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

                IEnumerable<Pawn> parents = victim.relations.DirectRelations.Where(r => r.def == PawnRelationDefOf.Parent).Select(r => r.otherPawn);
                if (!parents.Any()) return;

                foreach (var parent in parents) {
                    if (parent == null || parent.Dead) continue;

                    if (parent == attacker)
                    {
                        if (!TraitUtil.IsUnempathetic(parent))
                        {
                            parent.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("HarmedMyOwnChild"));
                        }
                    }
                    else
                    {
                        if (!TraitUtil.IsUnempathetic(parent))
                        {
                            parent.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("HarmedMyChild"), attacker);
                        }
                    }
                }
            } catch (Exception ex) {
                Log.Error($"[FamilyTies] An error occurred in TakeDamage patch: {ex.Message}");
            }
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

        public override float MoodOffset()
        {
            if (TraitUtil.IsUnempathetic(this.pawn))
            {
                this.pawn.needs.mood.thoughts.memories.RemoveMemory(this);

                return 0f;
            }

            return base.MoodOffset();
        }
    }

    public class Thought_MyChildIsInPain : Thought_Situational
    {
        public override string LabelCap
        {
            get
            {
                Pawn sufferingChild = FindFirstSufferingChild();
                if (sufferingChild != null)
                {
                    // Используем перевод с именованным аргументом
                    return "MyChildIsInPain_Label".Translate(sufferingChild.Named("CHILDNAME"));
                }
                return base.LabelCap;
            }
        }
        
        private Pawn FindFirstSufferingChild()
        {
            if (this.pawn.Map == null) return null;
            List<Pawn> allPawnsOnMap = this.pawn.Map.mapPawns.AllPawns;
            foreach (Pawn otherPawn in allPawnsOnMap)
            {
                if (otherPawn == this.pawn || otherPawn.relations == null) continue;
                if (otherPawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Parent) == this.pawn)
                {
                    if (!otherPawn.Dead && otherPawn.health.hediffSet.PainTotal > 0.01f)
                    {
                        return otherPawn;
                    }
                }
            }
            return null;
        }
    }

    public class ThoughtWorker_MyChildIsInPain : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (TraitUtil.IsUnempathetic(p)) return ThoughtState.Inactive;
            if (p.Map == null) return ThoughtState.Inactive;

            int sufferingChildrenCount = 0;
            List<Pawn> allPawnsOnMap = p.Map.mapPawns.AllPawns;
            foreach (Pawn otherPawn in allPawnsOnMap)
            {
                if (otherPawn == p || otherPawn.relations == null) continue;
                Pawn parent = otherPawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Parent);
                if (parent == p)
                {
                    if (!otherPawn.Dead && otherPawn.health.hediffSet.PainTotal > 0.01f)
                    {
                        sufferingChildrenCount++;
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

