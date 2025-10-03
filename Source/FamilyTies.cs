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

            if (pawn.story.traits.HasTrait(TraitDefOf.Psychopath)) return true;

            if (!FamilyTiesMod.settings.bloodlustCareAboutChildrenPain)
            {
                if (pawn.story.traits.HasTrait(TraitDefOf.Bloodlust)) return true;
            }

            if (ModLister.IdeologyInstalled)
            {
                if (!FamilyTiesMod.settings.cannibalsCareAboutChildrenPain)
                {
                    if (pawn.story.traits.HasTrait(TraitDef.Named("Cannibal"))) return true;
                }
            }

            return false;
        }
    }

    public static class SickPawnUtil
    {
        public static bool IsSick(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs.Any(IsWorrisomeDisease);
        }

        public static bool IsCriticallyIll(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs.Any(h => IsWorrisomeDisease(h) && h.def.lethalSeverity > 0 && h.Severity > 0.8f);
        }

        public static bool IsWorrisomeDisease(Hediff h)
        {
            if (h is Hediff_Injury) return false;

            bool isImmunizable = h.def.HasComp(typeof(HediffComp_Immunizable));
            bool makesSickThought = h.def.makesSickThought;
            bool isTendable = h.def.HasComp(typeof(HediffComp_TendDuration));

            return (isImmunizable || makesSickThought || isTendable) && h.Severity > 0.1f;
        }
    }

    public static class StoicismUtil
    {
        public static float GetPainThreshold(Pawn parent)
        {
            float threshold = FamilyTiesMod.settings.childWorryPainThreshold;

            if (parent.story.traits.HasTrait(TraitDef.Named("Wimp"))) return 0.01f;

            Trait nervesTrait = parent.story.traits.GetTrait(TraitDef.Named("Nerves"));

            if (nervesTrait != null && nervesTrait.Degree == 2) return 0.8f;

            return threshold;
        }

        public static float GetSicknessThreshold(Pawn parent)
        {
            float threshold = FamilyTiesMod.settings.childWorrySickThreshold;

            if (parent.story.traits.HasTrait(TraitDef.Named("Wimp"))) return 0.1f;

            Trait nervesTrait = parent.story.traits.GetTrait(TraitDef.Named("Nerves"));

            if (nervesTrait != null && nervesTrait.Degree == 2) return 0.9f;

            return threshold;
        }
    }

    public static class MasterpieceUtil
    {
        public static void CheckAndRewardParent(Pawn child, Thing product)
        {
            if (child == null || product == null) return;

            if (product.TryGetQuality(out QualityCategory qc) && (qc == QualityCategory.Masterwork || qc == QualityCategory.Legendary))
            {
                int ageLimit = FamilyTiesMod.settings.ageOfCaring;

                if (ageLimit != 0 && child.ageTracker.AgeBiologicalYears > ageLimit) return;

                IEnumerable<Pawn> parents = child.relations?.DirectRelations.Where(r => r.def == PawnRelationDefOf.Parent).Select(r => r.otherPawn);

                if (parents == null || !parents.Any()) return;

                foreach (var parent in parents)
                {
                    if (parent != null && !parent.Dead && !TraitUtil.IsUnempathetic(parent)) parent.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("MyChildCreatedMasterpiece"));
                }
            }
        }
    }

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("com.dickus.familyties");

            var targetDamageMethod = AccessTools.Method(typeof(Thing), "TakeDamage", new Type[] { typeof(DamageInfo) });
            if (targetDamageMethod != null) {
                var takeDamagePostfix = new HarmonyMethod(typeof(Pawn_TakeDamage_Patch), nameof(Pawn_TakeDamage_Patch.Postfix));

                harmony.Patch(targetDamageMethod, postfix: takeDamagePostfix);
            }

            var makeRecipeMethod = AccessTools.Method(typeof(GenRecipe), "MakeRecipeProducts");
            if (makeRecipeMethod != null)
            {
                var recipePostfix = new HarmonyMethod(typeof(FinishRecipe_Patch), nameof(FinishRecipe_Patch.Postfix));

                harmony.Patch(makeRecipeMethod, postfix: recipePostfix);
            }

            var completeConstructionMethod = AccessTools.Method(typeof(Frame), nameof(Frame.CompleteConstruction));
            if (completeConstructionMethod != null)
            {
                var constructPrefix = new HarmonyMethod(typeof(ConstructFinish_Patch), nameof(ConstructFinish_Patch.Prefix));
                var constructPostfix = new HarmonyMethod(typeof(ConstructFinish_Patch), nameof(ConstructFinish_Patch.Postfix));

                harmony.Patch(completeConstructionMethod, prefix: constructPrefix, postfix: constructPostfix);
            }

            var learnMethod = AccessTools.Method(typeof(SkillRecord), nameof(SkillRecord.Learn));
            if (learnMethod != null)
            {
                var learnPrefix = new HarmonyMethod(typeof(SkillLearn_Patch), nameof(SkillLearn_Patch.Prefix));
                var learnPostfix = new HarmonyMethod(typeof(SkillLearn_Patch), nameof(SkillLearn_Patch.Postfix));

                harmony.Patch(learnMethod, prefix: learnPrefix, postfix: learnPostfix);
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

                int ageLimit = FamilyTiesMod.settings.ageOfCaring;
                if (ageLimit != 0 && victim.ageTracker.AgeBiologicalYears > ageLimit) return;

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

    public class Thought_HarmedMyChild : Thought_MemorySocial
    {
        public override string LabelCap
        {
            get
            {
                Pawn attacker = this.otherPawn;
                if (attacker == null) return base.LabelCap;

                if (attacker.gender == Gender.Male)
                {
                    return "HarmedMyChild_Male".Translate();
                }
                else
                {
                    return "HarmedMyChild_Female".Translate();
                }
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

        public override string Description
        {
            get
            {
                if (this.pawn.gender == Gender.Male)
                {
                    return "HarmedMyOwnChild_Desc_Male".Translate();
                }
                else
                {
                    return "HarmedMyOwnChild_Desc_Female".Translate();
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
                if (this.CurStageIndex == 0)
                {
                    Pawn sufferingChild = FindFirstSufferingChild();
                    if (sufferingChild != null)
                    {
                        return "MyChildIsInPain_Label".Translate(sufferingChild.Named("CHILDNAME"));
                    }
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
                    Pawn child = otherPawn;

                    if (!child.Dead && child.health.hediffSet.PainTotal > 0.01f)
                    {
                        int ageLimit = FamilyTiesMod.settings.ageOfCaring;

                        if (ageLimit == 0 || child.ageTracker.AgeBiologicalYears <= ageLimit) return child;
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

            int suffereingChildrenCount = 0;
            List<Pawn> allPawnsOnMap = p.Map.mapPawns.AllPawns;

            foreach (Pawn otherPawn in allPawnsOnMap)
            {
                if (otherPawn == p || otherPawn.relations == null) continue;

                if (otherPawn.relations.DirectRelations.Any(rel => rel.def == PawnRelationDefOf.Parent && rel.otherPawn == p))
                {
                    Pawn child = otherPawn;
                    int ageLimit = FamilyTiesMod.settings.ageOfCaring;

                    if (ageLimit == 0 || child.ageTracker.AgeBiologicalYears <= ageLimit)
                    {
                        if (!child.Dead && child.health.hediffSet.PainTotal > 0.01f) suffereingChildrenCount++;
                    }
                }
            }

            if (suffereingChildrenCount == 1) return ThoughtState.ActiveAtStage(0);
            if (suffereingChildrenCount > 1) return ThoughtState.ActiveAtStage(1);

            return ThoughtState.Inactive;
        }
    }

    public class Thought_MyChildIsSick : Thought_Situational
    {
        public override string LabelCap
        {
            get
            {
                if (this.CurStageIndex == 0 || this.CurStageIndex == 1)
                {
                    Pawn sickChild = FindSingleSickChild();
                    if (sickChild != null)
                    {
                        string key = $"MyChildIsSick_Label_Stage{this.CurStageIndex}";

                        return key.Translate(sickChild.Named("CHILDNAME"));
                    }
                }

                return base.LabelCap;
            }
        }

        private Pawn FindSingleSickChild()
        {
            if (this.pawn.Map == null) return null;

            Pawn foundChild = null;
            int sickCount = 0;
            List<Pawn> allPawnsOnMap = this.pawn.Map.mapPawns.AllPawns;

            foreach (Pawn otherPawn in allPawnsOnMap)
            {
                if (otherPawn == this.pawn || otherPawn.relations == null) continue;

                if (otherPawn.relations.DirectRelations.Any(rel => rel.def == PawnRelationDefOf.Parent && rel.otherPawn == this.pawn))
                {
                    if (SickPawnUtil.IsSick(otherPawn))
                    {
                        sickCount++;

                        foundChild = otherPawn;
                    }
                }
            }

            return (sickCount == 1) ? foundChild : null;
        }

        public override string Description
        {
            get
            {
                Pawn sickChild = FindSingleSickChild();

                if (this.CurStageIndex == 0 && sickChild != null)
                {
                    string genderKey = (sickChild.gender == Gender.Male) ? "Male" : "Female";
                    string key = $"MyChildIsSick_Stage0_Desc_{genderKey}";

                    return key.Translate(sickChild.Named("CHILDNAME"));
                }

                if (this.CurStageIndex == 1 && sickChild != null)
                {
                    string parentGenderKey = (this.pawn.gender == Gender.Male) ? "ParentMale" : "ParentFemale";
                    string childGenderKey = (sickChild.gender == Gender.Male) ? "ChildMale" : "ChildFemale";
                    string key = $"MyChildIsSick_Stage1_Desc_{parentGenderKey}_{childGenderKey}";

                    return key.Translate(sickChild.Named("CHILDNAME"));
                }

                return base.Description;
            }
        }
    }

    public class ThoughtWorker_MyChildIsSick : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (TraitUtil.IsUnempathetic(p)) return ThoughtState.Inactive;
            if (p.Map == null) return ThoughtState.Inactive;

            List<Pawn> sickChildren = new List<Pawn>();
            List<Pawn> allPawnsOnMap = p.Map.mapPawns.AllPawns;

            foreach (Pawn otherPawn in allPawnsOnMap)
            {
                if (otherPawn == p || otherPawn.relations == null) continue;

                if (otherPawn.relations.DirectRelations.Any(rel => rel.def == PawnRelationDefOf.Parent && rel.otherPawn == p))
                {
                    Pawn child = otherPawn;
                    int ageLimit = FamilyTiesMod.settings.ageOfCaring;

                    if (ageLimit == 0 || child.ageTracker.AgeBiologicalYears <= ageLimit)
                    {
                        if (SickPawnUtil.IsSick(child)) sickChildren.Add(child);
                    }
                }
            }

            if (sickChildren.Count == 1)
            {
                Pawn theChild = sickChildren.First();

                if (SickPawnUtil.IsCriticallyIll(theChild))
                {
                    return ThoughtState.ActiveAtStage(1);
                }
                else
                {
                    return ThoughtState.ActiveAtStage(0);
                }
            }
            else if (sickChildren.Count > 1)
            {
                if (sickChildren.All(child => SickPawnUtil.IsCriticallyIll(child)))
                {
                    return ThoughtState.ActiveAtStage(3);
                }
                else
                {
                    return ThoughtState.ActiveAtStage(2);
                }
            }

            return ThoughtState.Inactive;
        }
    }

    public class Thought_MyParentIsInPain : Thought_Situational
    {
        public override string LabelCap
        {
            get
            {
                if (this.CurStageIndex == 0)
                {
                    Pawn sufferingParent = FindSingleSufferingParent();

                    if (sufferingParent != null)
                    {
                        string genderKey = (sufferingParent.gender == Gender.Male) ? "Male" : "Female";
                        string key = $"MyParentIsInPain_Label_{genderKey}";

                        return key.Translate();
                    }
                }

                return base.LabelCap;
            }
        }

        public override string Description
        {
            get
            {
                if (this.CurStageIndex == 0)
                {
                    Pawn sufferingParent = FindSingleSufferingParent();

                    if (sufferingParent != null)
                    {
                        string genderKey = (sufferingParent.gender == Gender.Male) ? "Male" : "Female";
                        string key = $"MyParentIsInPain_Desc_{genderKey}";

                        return key.Translate();
                    }
                }

                return base.Description;
            }
        }

        private Pawn FindSingleSufferingParent()
        {
            Pawn foundParent = null;
            int sufferingCount = 0;

            IEnumerable<Pawn> parents = this.pawn.relations.DirectRelations.Where(r => r.def == PawnRelationDefOf.Parent).Select(r => r.otherPawn);

            foreach (var parent in parents)
            {
                if (parent != null && !parent.Dead && parent.Map == this.pawn.Map && parent.health.hediffSet.PainTotal > 0.01f)
                {
                    sufferingCount++;
                    foundParent = parent;
                }
            }

            return (sufferingCount == 1) ? foundParent : null;
        }
    }

    public class ThoughtWorker_MyParentIsInPain : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!FamilyTiesMod.settings.childrenWorryAboutParents) return ThoughtState.Inactive;

            int currentAge = p.ageTracker.AgeBiologicalYears;
            int minAge = FamilyTiesMod.settings.childMinEmpathyAge;
            int maxAge = FamilyTiesMod.settings.childMaxEmpathyAge;

            if (currentAge < minAge) return ThoughtState.Inactive;
            if (maxAge != 0 && currentAge > maxAge) return ThoughtState.Inactive;

            if (p.ageTracker.AgeBiologicalYears < FamilyTiesMod.settings.childMinEmpathyAge) return ThoughtState.Inactive;
            if (TraitUtil.IsUnempathetic(p)) return ThoughtState.Inactive;
            if (p.Map == null) return ThoughtState.Inactive;

            int sufferingParentsCount = 0;

            IEnumerable<Pawn> parents = p.relations.DirectRelations.Where(r => r.def == PawnRelationDefOf.Parent).Select(r => r.otherPawn);

            foreach (var parent in parents)
            {
                float painThreshold = StoicismUtil.GetPainThreshold(parent);

                if (parent.health.hediffSet.PainTotal >= painThreshold) sufferingParentsCount++;
            }

            if (sufferingParentsCount == 1)
            {
                return ThoughtState.ActiveAtStage(0);
            }
            else if (sufferingParentsCount > 1)
            {
                return ThoughtState.ActiveAtStage(1);
            }

            return ThoughtState.Inactive;
        }
    }

    public class Thought_MyParentIsSick : Thought_Situational
    {
        public override string LabelCap
        {
            get
            {
                if (this.CurStageIndex == 0)
                {
                    Pawn sickParent = FindSingleSickParent();

                    if (sickParent != null)
                    {
                        string genderKey = (sickParent.gender == Gender.Male) ? "Male" : "Female";
                        string key = $"MyParentIsSick_Label_{genderKey}";

                        return key.Translate();
                    }
                }

                return base.LabelCap;
            }
        }

        public override string Description
        {
            get
            {
                if (this.CurStageIndex == 0)
                {
                    Pawn sickParent = FindSingleSickParent();

                    if (sickParent != null)
                    {
                        string genderKey = (sickParent.gender == Gender.Male) ? "Male" : "Female";
                        string key = $"MyParentIsSick_Desc_{genderKey}";

                        return key.Translate();
                    }
                }

                return base.Description;
            }
        }

        private Pawn FindSingleSickParent()
        {
            Pawn foundParent = null;
            int sickCount = 0;

            IEnumerable<Pawn> parents = this.pawn.relations.DirectRelations.Where(r => r.def == PawnRelationDefOf.Parent).Select(r => r.otherPawn);

            foreach (var parent in parents)
            {
                if (parent != null && !parent.Dead && parent.Map == this.pawn.Map && SickPawnUtil.IsSick(parent))
                {
                    sickCount++;
                    foundParent = parent;
                }
            }

            return (sickCount == 1) ? foundParent : null;
        }
    }

    public class ThoughtWorker_MyParentIsSick : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!FamilyTiesMod.settings.childrenWorryAboutParents) return ThoughtState.Inactive;

            int currentAge = p.ageTracker.AgeBiologicalYears;
            int minAge = FamilyTiesMod.settings.childMinEmpathyAge;
            int maxAge = FamilyTiesMod.settings.childMaxEmpathyAge;

            if (currentAge < minAge) return ThoughtState.Inactive;
            if (maxAge != 0 && currentAge > maxAge) return ThoughtState.Inactive;

            if (p.ageTracker.AgeBiologicalYears < FamilyTiesMod.settings.childMinEmpathyAge) return ThoughtState.Inactive;
            if (TraitUtil.IsUnempathetic(p)) return ThoughtState.Inactive;
            if (p.Map == null) return ThoughtState.Inactive;

            int sickParentsCount = 0;

            IEnumerable<Pawn> parents = p.relations.DirectRelations.Where(r => r.def == PawnRelationDefOf.Parent).Select(r => r.otherPawn);

            foreach (var parent in parents)
            {
                float sicknessThreshold = StoicismUtil.GetSicknessThreshold(parent);

                if (IsSickAboveThreshold(parent, sicknessThreshold)) sickParentsCount++;
            }

            if (sickParentsCount == 1)
            {
                return ThoughtState.ActiveAtStage(0);
            }
            else if (sickParentsCount > 1)
            {
                return ThoughtState.ActiveAtStage(1);
            }

            return ThoughtState.Inactive;
        }

        private bool IsSickAboveThreshold(Pawn pawn, float threshold)
        {
            return pawn.health.hediffSet.hediffs.Any(h => (h.def.HasComp(typeof(HediffComp_Immunizable)) || h.def.makesSickThought || h.def.HasComp(typeof(HediffComp_TendDuration))) && h.Severity >= threshold);
        }
    }

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
    
    public static class SkillLearn_Patch
    {
        public static void Prefix(SkillRecord __instance, ref int __state, Pawn ___pawn)
        {
            __state = __instance.Level;
        }

        public static void Postfix(SkillRecord __instance, int __state, Pawn ___pawn)
        {
            if (!FamilyTiesMod.settings.proudForSkillUp) return;

            if (__state < __instance.Level)
            {
                Pawn child = ___pawn;

                if (child == null) return;

                int ageLimit = FamilyTiesMod.settings.ageOfCaring;
                bool isAgeOk = (ageLimit == 0 || child.ageTracker.AgeBiologicalYears <= ageLimit);

                if (!isAgeOk) return;

                IEnumerable<Pawn> parents = child.relations?.DirectRelations.Where(r => r.def == PawnRelationDefOf.Parent).Select(r => r.otherPawn);

                if (parents == null || !parents.Any()) return;

                foreach (var parent in parents)
                {
                    if (parent != null && !parent.Dead)
                    {
                        bool isUnempathetic = TraitUtil.IsUnempathetic(parent);

                        if (!isUnempathetic) parent.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("MyChildLeveledUpSkill"));
                    }
                }
            }
        }
    }
}

