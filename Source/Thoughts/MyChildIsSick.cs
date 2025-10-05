using RimWorld;
using Verse;
using System.Collections.Generic;

namespace FamilyTies
{
    public class Thought_MyChildIsSick : Thought_Situational
    {
        public override string LabelCap
        {
            get
            {
                if (this.CurStageIndex == 0 || this.CurStageIndex == 1)
                {
                    Pawn sickChild = FindSingleSickChild();
                    if (sickChild != null)
                    {
                        string key = $"MyChildIsSick_Label_Stage{this.CurStageIndex}";

                        return key.Translate(sickChild.Named("CHILDNAME"));
                    }
                }

                return base.LabelCap;
            }
        }

        private Pawn FindSingleSickChild()
        {
            if (this.pawn.Map == null) return null;

            Pawn foundChild = null;
            int sickCount = 0;
            List<Pawn> allPawnsOnMap = this.pawn.Map.mapPawns.AllPawns;

            foreach (Pawn otherPawn in allPawnsOnMap)
            {
                if (otherPawn == this.pawn || otherPawn.relations == null) continue;

                if (otherPawn.relations.DirectRelations.Any(rel => rel.def == PawnRelationDefOf.Parent && rel.otherPawn == this.pawn))
                {
                    if (SickPawnUtil.IsSick(otherPawn))
                    {
                        sickCount++;

                        foundChild = otherPawn;
                    }
                }
            }

            return (sickCount == 1) ? foundChild : null;
        }

        public override string Description
        {
            get
            {
                Pawn sickChild = FindSingleSickChild();

                if (this.CurStageIndex == 0 && sickChild != null)
                {
                    string genderKey = (sickChild.gender == Gender.Male) ? "Male" : "Female";
                    string key = $"MyChildIsSick_Stage0_Desc_{genderKey}";

                    return key.Translate(sickChild.Named("CHILDNAME"));
                }

                if (this.CurStageIndex == 1 && sickChild != null)
                {
                    string parentGenderKey = (this.pawn.gender == Gender.Male) ? "ParentMale" : "ParentFemale";
                    string childGenderKey = (sickChild.gender == Gender.Male) ? "ChildMale" : "ChildFemale";
                    string key = $"MyChildIsSick_Stage1_Desc_{parentGenderKey}_{childGenderKey}";

                    return key.Translate(sickChild.Named("CHILDNAME"));
                }

                return base.Description;
            }
        }
    }
}

