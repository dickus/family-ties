using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;

namespace FamilyTies
{
    public class Thought_MyParentIsInPain : Thought_Situational
    {
        public override string LabelCap
        {
            get
            {
                if (this.CurStageIndex == 0)
                {
                    Pawn sufferingParent = FindSingleSufferingParent();

                    if (sufferingParent != null)
                    {
                        string genderKey = (sufferingParent.gender == Gender.Male) ? "Male" : "Female";
                        string key = $"MyParentIsInPain_Label_{genderKey}";

                        return key.Translate();
                    }
                }

                return base.LabelCap;
            }
        }

        public override string Description
        {
            get
            {
                if (this.CurStageIndex == 0)
                {
                    Pawn sufferingParent = FindSingleSufferingParent();

                    if (sufferingParent != null)
                    {
                        string genderKey = (sufferingParent.gender == Gender.Male) ? "Male" : "Female";
                        string key = $"MyParentIsInPain_Desc_{genderKey}";

                        return key.Translate();
                    }
                }

                return base.Description;
            }
        }

        private Pawn FindSingleSufferingParent()
        {
            Pawn foundParent = null;
            int sufferingCount = 0;

            IEnumerable<Pawn> parents = this.pawn.relations.DirectRelations.Where(r => r.def == PawnRelationDefOf.Parent).Select(r => r.otherPawn);

            foreach (var parent in parents)
            {
                if (parent != null && !parent.Dead && parent.Map == this.pawn.Map && parent.health.hediffSet.PainTotal > 0.01f)
                {
                    sufferingCount++;
                    foundParent = parent;
                }
            }

            return (sufferingCount == 1) ? foundParent : null;
        }
    }
}

