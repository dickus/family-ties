using UnityEngine;
using Verse;

namespace FamilyTies
{
    public class FamilyTiesMod : Mod
    {
        public static FamilyTiesSettings settings;

        public FamilyTiesMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<FamilyTiesSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.CheckboxLabeled(
                    "SettingLabel_CannibalsCare".Translate(),
                    ref settings.cannibalsCare,
                    "SettingTooltip_CannibalsCare".Translate()
            );

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Family Ties";
        }
    }
}

