using RimWorld;

namespace FamilyTies
{
    [DefOf]
    public static class FamilyTiesDefOf
    {
        public static TraitDef FamilyPerson;
        public static FamilyPerson_ThoughtAmplifierDef FamilyPerson_ThoughtAmplifier;

        static FamilyTiesDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(FamilyTiesDefOf));
        }
    }
}

