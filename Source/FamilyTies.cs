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
                if (!FamilyTiesMod.settings.cannibalsCareAboutChildernPain)
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
                if (parent != null && !parent.Dead && parent.Map == p.Map && parent.health.hediffSet.PainTotal > 0.01f) sufferingParentsCount++;
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
                if (parent != null && !parent.Dead && parent.Map == p.Map && SickPawnUtil.IsSick(parent)) sickParentsCount++;
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
    }
}

