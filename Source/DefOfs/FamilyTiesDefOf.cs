using RimWorld;
using Verse;

namespace FamilyTies
{
    [DefOf]
    public static class FamilyTiesDefOf
    {
        public static ThoughtDef FamilyPerson_NearChildren;

        public static TraitDef FamilyPerson;

        static FamilyTiesDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(FamilyTiesDefOf));
        }
    }
}

