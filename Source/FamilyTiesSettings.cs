using Verse;
using RimWorld;

namespace FamilyTies
{
    public class FamilyTiesSettings : ModSettings
    {
        public bool cannibalsCare = false;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref cannibalsCare, "cannibalsCare", false);
        }
    }
}

