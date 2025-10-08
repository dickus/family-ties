using RimWorld;
using HarmonyLib;
using Verse;
using System;
using System.Linq;
using System.Collections.Generic;

namespace FamilyTies
{
    public static class Pawn_TakeDamage_Patch
    {
        public static void Postfix(Thing __instance, DamageInfo dinfo)
        {
             if (!(__instance is Pawn victim)) return;
             try {
                if (dinfo.Amount <= 0) return;
                if (!(dinfo.Instigator is Pawn attacker) || attacker == victim) return;
                if (!victim.RaceProps.Humanlike || victim.relations == null) return;

                int ageLimit = FamilyTiesMod.settings.ageOfCaring;
                if (ageLimit != 0 && victim.ageTracker.AgeBiologicalYears > ageLimit) return;

                IEnumerable<Pawn> parents = victim.relations.DirectRelations.Where(r => r.def == PawnRelationDefOf.Parent).Select(r => r.otherPawn);
                if (!parents.Any()) return;

                foreach (var parent in parents) {
                    if (!PawnRelationUtil.HasGoodRelation(parent, victim)) continue;

                    if (parent == null || parent.Dead) continue;

                    if (parent == attacker)
                    {
                        if (!TraitUtil.IsUnempathetic(parent))
                        {
                            ThoughtDef thoughtDef = ThoughtDef.Named("HarmedMyOwnChild");
                            Thought_Memory thought = null;

                            if (FamilyPersonUtil.IsFamilyPerson(parent))
                            {
                                thought = (Thought_Memory)ThoughtMaker.MakeThought(thoughtDef, 1);
                            }
                            else
                            {
                                thought = (Thought_Memory)ThoughtMaker.MakeThought(thoughtDef, 0);
                            }

                            parent.needs.mood.thoughts.memories.TryGainMemory(thought, victim);
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
}

