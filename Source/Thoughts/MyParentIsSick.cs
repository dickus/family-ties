using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;

namespace FamilyTies
{
    public class Thought_MyParentIsSick : Thought_Situational
    {
        public override string LabelCap
        {
            get
            {
                if (this.CurStageIndex == 0)
                {
                    Pawn sickParent = FindSingleSickParent();

                    if (sickParent != null)
                    {
                        string genderKey = (sickParent.gender == Gender.Male) ? "Male" : "Female";
                        string key = $"MyParentIsSick_Label_{genderKey}";

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
                    Pawn sickParent = FindSingleSickParent();

                    if (sickParent != null)
                    {
                        string genderKey = (sickParent.gender == Gender.Male) ? "Male" : "Female";
                        string key = $"MyParentIsSick_Desc_{genderKey}";

                        return key.Translate();
                    }
                }

                return base.Description;
            }
        }

        private Pawn FindSingleSickParent()
        {
            Pawn foundParent = null;
            int sickCount = 0;

            IEnumerable<Pawn> parents = this.pawn.relations.DirectRelations.Where(r => r.def == PawnRelationDefOf.Parent).Select(r => r.otherPawn);

            foreach (var parent in parents)
            {
                if (!PawnRelationUtil.HasGoodRelation(this.pawn, parent)) continue;

                if (parent != null && !parent.Dead && parent.Map == this.pawn.Map && SickPawnUtil.IsSick(parent))
                {
                    sickCount++;
                    foundParent = parent;
                }
            }

            return (sickCount == 1) ? foundParent : null;
        }
    }
}

