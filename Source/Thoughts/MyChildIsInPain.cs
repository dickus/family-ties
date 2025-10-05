using RimWorld;
using Verse;
using System.Collections.Generic;

namespace FamilyTies
{
    public class Thought_MyChildIsInPain : Thought_Situational
    {
        public override string LabelCap
        {
            get
            {
                if (this.CurStageIndex == 0)
                {
                    Pawn sufferingChild = FindFirstSufferingChild();
                    if (sufferingChild != null)
                    {
                        return "MyChildIsInPain_Label".Translate(sufferingChild.Named("CHILDNAME"));
                    }
                }

                return base.LabelCap;
            }
        }
        
        private Pawn FindFirstSufferingChild()
        {
            if (this.pawn.Map == null) return null;

            List<Pawn> allPawnsOnMap = this.pawn.Map.mapPawns.AllPawns;
            foreach (Pawn otherPawn in allPawnsOnMap)
            {
                if (otherPawn == this.pawn || otherPawn.relations == null) continue;

                if (otherPawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Parent) == this.pawn)
                {
                    Pawn child = otherPawn;

                    if (!child.Dead && child.health.hediffSet.PainTotal > 0.01f)
                    {
                        int ageLimit = FamilyTiesMod.settings.ageOfCaring;

                        if (ageLimit == 0 || child.ageTracker.AgeBiologicalYears <= ageLimit) return child;
                    }
                }
            }
            return null;
        }
    }
}

