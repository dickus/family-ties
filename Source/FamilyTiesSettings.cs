using Verse;
using RimWorld;

namespace FamilyTies
{
    public class FamilyTiesSettings : ModSettings
    {
        public bool cannibalsCareAboutChildernPain = false;

        public bool patchFamilyDiedThought = true;
        public bool patchFamilyDiedSocial = true;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref cannibalsCareAboutChildernPain, "cannibalsCareAboutChildernPain", false);

            Scribe_Values.Look(ref patchFamilyDiedThought, "patchFamilyDiedThought", true);

            Scribe_Values.Look(ref patchFamilyDiedSocial, "patchFamilyDiedSocial", true);
        }
    }
}

