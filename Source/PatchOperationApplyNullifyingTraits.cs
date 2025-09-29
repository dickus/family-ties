using System.Collections.Generic;
using System.Xml;
using Verse;

namespace FamilyTies
{
    public class PatchOperationApplyNullifyingTraits : PatchOperation
    {
        protected override bool ApplyWorker(XmlDocument xml)
        {
            if (FamilyTiesMod.settings.patchFamilyDiedThought) ApplyTraitsToDef(xml, $"Defs/ThoughtDef[@Name='DeathMemoryFamily']");

            if (FamilyTiesMod.settings.patchFamilyDiedSocial)
            {
                var familyDiedSocial = new List<string>
                {
                    "SoldMyLovedOne",
                    "SoldMyBondedAnimal",
                    "SoldMyBondedAnimalMood",
                    "KilledMyFriend",
                    "KilledMyLover",
                    "KilledMyFiance",
                    "KilledMySpouse",
                    "KilledMyFather",
                    "KilledMyMother",
                    "KilledMySon",
                    "KilledMyDaughter",
                    "KilledMyBrother",
                    "KilledMySister",
                    "KilledMyKin",
                    "KilledMyBondedAnimal"
                };

                foreach (string defName in familyDiedSocial)
                {
                    ApplyTraitsToDef(xml, $"Defs/ThoughtDef[defName='{defName}']");
                }
            }

            return true;
        }

        private void ApplyTraitsToDef(XmlDocument xml, string xpath)
        {
            XmlNode thoughtDefNode = xml.SelectSingleNode(xpath);

            if (thoughtDefNode == null) return;

            XmlNode nullifyingTraitsNode = thoughtDefNode.SelectSingleNode("nullifyingTraits");

            if (nullifyingTraitsNode == null)
            {
                XmlElement newTraitsNode = xml.CreateElement("nullifyingTraits");
                thoughtDefNode.AppendChild(newTraitsNode);
                nullifyingTraitsNode = newTraitsNode;
            }

            var traitsToAdd = new List<string>();
            traitsToAdd.Add("Psychopath");

            foreach (string trait in traitsToAdd)
            {
                if (nullifyingTraitsNode.SelectSingleNode($"li[text()='{trait}']") == null)
                {
                    XmlElement newTraitLi = xml.CreateElement("li");
                    newTraitLi.InnerText = trait;
                    nullifyingTraitsNode.AppendChild(newTraitLi);
                }
            }
        }
    }
}

