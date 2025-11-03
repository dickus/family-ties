using RimWorld;
using Verse;

namespace FamilyTies
{
    public static class SickPawnUtil
    {
        public static bool IsSick(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs.Any(IsWorrisomeDisease);
        }

        public static bool IsCriticallyIll(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs.Any(h => IsWorrisomeDisease(h) && h.def.lethalSeverity > 0 && h.Severity > 0.8f);
        }

        public static bool IsWorrisomeDisease(Hediff h)
        {
            if (h is Hediff_MissingPart) return false;
            if (h is Hediff_Injury) return false;
            if (h.def == HediffDefOf.InfantIllness) return false;

            bool isImmunizable = h.def.HasComp(typeof(HediffComp_Immunizable));
            bool makesSickThought = h.def.makesSickThought;
            bool isTendable = h.def.HasComp(typeof(HediffComp_TendDuration));

            return (isImmunizable || makesSickThought || isTendable) && h.Severity > 0.1f;
        }

        public static bool IsWorrisomeDiseaseAboveThreshold(Pawn pawn, float threshold)
        {
            if (pawn?.health?.hediffSet == null) return false;

            return pawn.health.hediffSet.hediffs.Any(h => IsWorrisomeDisease(h) && h.Severity >= threshold);
        }
    }
}

