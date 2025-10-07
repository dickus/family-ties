using RimWorld;

namespace FamilyTies
{
    [DefOf]
    public static class FamilyTiesDefOf
    {
        public static TraitDef FamilyPerson;

        static FamilyTiesDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(FamilyTiesDefOf));
        }
    }
}

