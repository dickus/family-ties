using UnityEngine;
using Verse;
using System;

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

            string ageOfCaringLabel = (settings.ageOfCaring == 0) ? "Unlimited".TranslateSimple() : settings.ageOfCaring.ToString();

            listingStandard.Label(
                    "FT_SettingLabel_AgeOfCaring".Translate() + ": " + ageOfCaringLabel,
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
            }

            listingStandard.Gap(8f);

            listingStandard.CheckboxLabeled(
                    "FT_SettingLabel_ChildrenWorry".Translate(),
                    ref settings.childrenWorryAboutParents,
                    "FT_SettingTooltip_ChildrenWorry".Translate()
            );

            if (settings.childrenWorryAboutParents)
            {
                Listing_Standard childrenWorry_Section = listingStandard.BeginSection(Text.LineHeight + 104f);

                childrenWorry_Section.ColumnWidth = (childrenWorry_Section.ColumnWidth - 17f) / 2f;

                childrenWorry_Section.Label(
                        "FT_SettingLabel_ChildMinEmpathyAge".Translate() + ": " + settings.childMinEmpathyAge,
                        -1f,
                        new TipSignal("FT_SettingTooltip_ChildMinEmpathyAge".Translate())
                        );
                settings.childMinEmpathyAge = (int)childrenWorry_Section.Slider(settings.childMinEmpathyAge, 0, 18);

                float painThresholdRounded = (float)Math.Round(settings.childWorryPainThreshold, 2);

                childrenWorry_Section.Label(
                        "FT_SettingLabel_ChildrenWorry_PainThreshold".Translate() + ": " + painThresholdRounded.ToStringPercent(),
                        -1f,
                        new TipSignal("FT_SettingTooltip_ChildrenWorry_PainThreshold".Translate())
                );
                settings.childWorryPainThreshold = childrenWorry_Section.Slider(settings.childWorryPainThreshold, 0f, 1f);

                childrenWorry_Section.NewColumn();

                string maxAgeLabel = (settings.childMaxEmpathyAge == 0) ? "Unlimited".TranslateSimple() : settings.childMaxEmpathyAge.ToString();

                childrenWorry_Section.Label(
                        "FT_SettingLabel_ChildMaxEmpathyAge".Translate() + ": " + maxAgeLabel,
                        -1f,
                        new TipSignal("FT_SettingTooltip_ChildMaxEmpathyAge".Translate())
                );
                settings.childMaxEmpathyAge = (int)childrenWorry_Section.Slider(settings.childMaxEmpathyAge, 0, 100);

                float sickThresholdRounded = (float)Math.Round(settings.childWorrySickThreshold, 2);

                childrenWorry_Section.Label(
                        "FT_SettingLabel_ChildrenWorry_SickThreshold".Translate() + ": " + sickThresholdRounded.ToStringPercent(),
                        -1f,
                        new TipSignal("FT_SettingTooltip_ChildrenWorry_SickThreshold".Translate())
                );
                settings.childWorrySickThreshold = childrenWorry_Section.Slider(settings.childWorrySickThreshold, 0f, 1f);

                if (childrenWorry_Section.ButtonText("Reset".TranslateSimple()))
                {
                    settings.childMinEmpathyAge = 6;
                    settings.childMaxEmpathyAge = 18;
                    settings.childWorryPainThreshold = 0.15f;
                    settings.childWorrySickThreshold = 0.25f;
                }

                listingStandard.EndSection(childrenWorry_Section);
            }

            listingStandard.Gap(8f);

            Rect proudParentsRect = listingStandard.GetRect(32f);
            Listing_Standard proudParents_Section = new Listing_Standard();

            proudParents_Section.Begin(proudParentsRect);

            proudParents_Section.ColumnWidth = (proudParentsRect.width - 17f) / 2f;

            proudParents_Section.CheckboxLabeled(
                    "FT_SettingLabel_ProudForMasterpiece".Translate(),
                    ref settings.proudForMasterpiece,
                    "FT_SettingTooltip_ProudForMasterpiece".Translate()
            );

            proudParents_Section.NewColumn();

            proudParents_Section.CheckboxLabeled(
                    "FT_SettingLabel_ProudForSkillUp".Translate(),
                    ref settings.proudForSkillUp,
                    "FT_SettingTooltip_ProudForSkillUp".Translate()
            );

            proudParents_Section.End();

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);

            if (settings.childMinEmpathyAge > settings.childMaxEmpathyAge) settings.childMaxEmpathyAge = settings.childMinEmpathyAge;
        }

        public override string SettingsCategory()
        {
            return "Family Ties";
        }
    }
}

