using RimWorld;
using Verse;

namespace FamilyTies
{
    public static class TraitUtil
    {
        public static bool IsUnempathetic(Pawn pawn, bool forPositiveThought = false)
        {
            if (pawn?.story?.traits == null) return false;

            if (pawn.story.traits.HasTrait(TraitDefOf.Psychopath)) return true;

            if (!forPositiveThought)
            {
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
            }

            return false;
        }
    }
}

