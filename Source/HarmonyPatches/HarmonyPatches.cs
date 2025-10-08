using RimWorld;
using HarmonyLib;
using Verse;
using System;
using System.Collections.Generic;

namespace FamilyTies
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("com.dickus.familyties");

            var targetDamageMethod = AccessTools.Method(
                    typeof(Thing),
                    "TakeDamage",
                    new Type[] { typeof(DamageInfo) }
            );
            if (targetDamageMethod != null) {
                var takeDamagePostfix = new HarmonyMethod(
                        typeof(Pawn_TakeDamage_Patch),
                        nameof(Pawn_TakeDamage_Patch.Postfix)
                );

                harmony.Patch(targetDamageMethod, postfix: takeDamagePostfix);
            }

            var lostLimbMethod = AccessTools.Method(
                    typeof(Pawn_HealthTracker),
                    nameof(Pawn_HealthTracker.AddHediff),
                    new Type[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult) }
            );
            if (lostLimbMethod != null)
            {
                var lostLimbPostfix = new HarmonyMethod(
                        typeof(LimbLoss_Patch),
                        nameof(LimbLoss_Patch.Postfix)
                );

                harmony.Patch(lostLimbMethod, postfix: lostLimbPostfix);
            }

            var makeRecipeMethod = AccessTools.Method(
                    typeof(GenRecipe),
                    "MakeRecipeProducts",
                    new Type[] { typeof(RecipeDef), typeof(Pawn), typeof(List<Thing>), typeof(ThingWithComps), typeof(IBillGiver), typeof(string) });
            if (makeRecipeMethod != null)
            {
                var recipePostfix = new HarmonyMethod(
                        typeof(FinishRecipe_Patch),
                        nameof(FinishRecipe_Patch.Postfix)
                );

                harmony.Patch(makeRecipeMethod, postfix: recipePostfix);
            }

            var completeConstructionMethod = AccessTools.Method(
                    typeof(Frame),
                    nameof(Frame.CompleteConstruction)
            );
            if (completeConstructionMethod != null)
            {
                var constructPrefix = new HarmonyMethod(
                        typeof(ConstructFinish_Patch),
                        nameof(ConstructFinish_Patch.Prefix)
                );
                var constructPostfix = new HarmonyMethod(
                        typeof(ConstructFinish_Patch),
                        nameof(ConstructFinish_Patch.Postfix)
                );

                harmony.Patch(completeConstructionMethod, prefix: constructPrefix, postfix: constructPostfix);
            }

            var learnMethod = AccessTools.Method(
                    typeof(SkillRecord),
                    nameof(SkillRecord.Learn),
                    new Type[] { typeof(float), typeof(bool), typeof(bool) }
            );
            if (learnMethod != null)
            {
                var learnPrefix = new HarmonyMethod(
                        typeof(SkillLearn_Patch),
                        nameof(SkillLearn_Patch.Prefix)
                );
                var learnPostfix = new HarmonyMethod(
                        typeof(SkillLearn_Patch),
                        nameof(SkillLearn_Patch.Postfix)
                );

                harmony.Patch(learnMethod, prefix: learnPrefix, postfix: learnPostfix);
            }

            var moodOffsetMethod = AccessTools.Method(
                    typeof(Thought_Memory),
                    nameof(Thought_Memory.MoodOffset)
            );
            if (moodOffsetMethod != null)
            {
                var moodOffsetPostfix = new HarmonyMethod(
                        typeof(ThoughtAmplifier_Patch),
                        nameof(ThoughtAmplifier_Patch.Postfix)
                );

                harmony.Patch(moodOffsetMethod, postfix: moodOffsetPostfix);
            }

            var workerMoodOffsetMethod = AccessTools.Method(
                    typeof(ThoughtWorker),
                    nameof(ThoughtWorker.MoodMultiplier)
            );
            if (workerMoodOffsetMethod != null)
            {
                var workerMoodOffsetPostfix = new HarmonyMethod(
                        typeof(ThoughtWorker_MoodMultiplier_Patch),
                        nameof(ThoughtWorker_MoodMultiplier_Patch.Postfix)
                );

                harmony.Patch(workerMoodOffsetMethod, postfix: workerMoodOffsetPostfix);
            }
        }
    }
}

