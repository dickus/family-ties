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

            var ageOfCaring = settings.ageOfCaring;

            listingStandard.Label(
                    "FT_SettingLabel_AgeOfCaring".Translate() + ": " + ageOfCaring,
                    -1f,
                    new TipSignal("FT_SettingTooltip_AgeOfCaring".Translate())
            );

            settings.ageOfCaring = (int)listingStandard.Slider(settings.ageOfCaring, 0, 100);

            listingStandard.CheckboxLabeled(
                    "FT_SettingLabel_CannibalsCareAboutChildrenPain".Translate(),
                    ref settings.cannibalsCareAboutChildernPain,
                    "FT_SettingTooltip_CannibalsCareAboutChildrenPain".Translate()
            );

            listingStandard.CheckboxLabeled(
                    "FT_SettingLabel_FamilyDiedThought".Translate(),
                    ref settings.patchFamilyDiedThought,
                    "FT_SettingTooltip_FamilyDiedThought".Translate()
            );

            if (settings.patchFamilyDiedThought)
            {
                Listing_Standard familyDied_Section = listingStandard.BeginSection(Text.LineHeight + 1.1f);

                familyDied_Section.CheckboxLabeled(
                        "FT_SettingLabel_FamilyDiedSocial".Translate(),
                        ref settings.patchFamilyDiedSocial,
                        "FT_SettingTooltip_FamilyDiedSocial".Translate()
                        );

                listingStandard.EndSection(familyDied_Section);
                listingStandard.Gap(4f);
            }

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Family Ties";
        }
    }
}

