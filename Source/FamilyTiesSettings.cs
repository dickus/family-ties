using Verse;
using RimWorld;

namespace FamilyTies
{
    public class FamilyTiesSettings : ModSettings
    {
        public int ageOfCaring = 0;

        public bool cannibalsCareAboutChildernPain = false;

        public bool patchFamilyDiedThought = true;
        public bool patchFamilyDiedSocial = true;

        public bool childrenWorryAboutParents = true;
        public int childMinEmpathyAge = 6;
        public int childMaxEmpathyAge = 0;

        public float childWorryPainThreshold = 0.15f;
        public float childWorrySickThreshold = 0.25f;

        public bool proudForMasterpiece = true;
        public bool proudForSkillUp = true;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref ageOfCaring, "ageOfCaring", ageOfCaring, true);

            Scribe_Values.Look(ref cannibalsCareAboutChildernPain, "cannibalsCareAboutChildernPain", false);

            Scribe_Values.Look(ref patchFamilyDiedThought, "patchFamilyDiedThought", true);
            Scribe_Values.Look(ref patchFamilyDiedSocial, "patchFamilyDiedSocial", true);

            Scribe_Values.Look(ref childrenWorryAboutParents, "childrenWorryAboutParents", true);
            Scribe_Values.Look(ref childMinEmpathyAge, "childMinEmpathyAge", childMinEmpathyAge, true);
            Scribe_Values.Look(ref childMaxEmpathyAge, "childMaxEmpathyAge", childMaxEmpathyAge, true);

            Scribe_Values.Look(ref childWorryPainThreshold, "childWorryPainThreshold", childWorryPainThreshold, true);
            Scribe_Values.Look(ref childWorrySickThreshold, "childWorrySickThreshold", childWorrySickThreshold, true);

            Scribe_Values.Look(ref proudForMasterpiece, "proudForMasterpiece", true);
            Scribe_Values.Look(ref proudForSkillUp, "proudForSkillUp", true);
        }
    }
}

