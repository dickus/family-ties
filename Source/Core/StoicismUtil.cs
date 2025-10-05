using RimWorld;
using Verse;

namespace FamilyTies
{
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
}

