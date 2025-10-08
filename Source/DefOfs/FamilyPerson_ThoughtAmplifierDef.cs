using RimWorld;
using Verse;
using System.Collections.Generic;

namespace FamilyTies
{
    public class FamilyPerson_ThoughtAmplifierDef : Def
    {
        public List<FamilyPerson_ThoughtAmplifierEntry> amplifiers;
    }

    public class FamilyPerson_ThoughtAmplifierEntry
    {
        public string thoughtDef;
        public float multiplier = 1f;
        public bool onlyIfPositive = false;
        public bool onlyIfNegative = false;
    }
}

