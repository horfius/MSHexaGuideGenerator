using MSHexaGuideGen.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSHexaGuideGen.Processors
{
    public class SkillsOrderingProcessor
    {
        private List<SkillOrder> InputSkillOrder {  get; set; }

        public SkillsOrderingProcessor(List<SkillOrder> inputSkillOrder) 
        {
            InputSkillOrder = inputSkillOrder;
        }

        public SkillOrder[] Process()
        {
            var skillsMapped = InputSkillOrder
                .GroupBy(s => s.SkillName)
                .ToDictionary(g => g.Key, g => g.ToList());

            var optimizedOrder = new List<SkillOrder>();
            for (var i = 0; i < InputSkillOrder.Count; i++)
            {
                var lowestName = "";
                var lowestLevel = 0;
                var greatestVal = 0.0;
                foreach(var skill in skillsMapped) 
                {
                    if (skill.Value.Count > 0 && skill.Value[0].SkillName != lowestName && skill.Value[0].PercentGain > greatestVal) 
                    {
                        lowestName = skill.Key;
                        lowestLevel = skill.Value[0].Level;
                        greatestVal = skill.Value[0].PercentGain;
                    }
                    else if (skill.Value.Count > 0 && optimizedOrder.Count > 0 && skill.Value[0].SkillName == optimizedOrder[optimizedOrder.Count-1].SkillName &&
                        skill.Value[0].PercentGain + (optimizedOrder[optimizedOrder.Count - 1].PercentGain > 1 ? 
                            Math.Sqrt(optimizedOrder[optimizedOrder.Count-1].PercentGain)/3 :
                            optimizedOrder[optimizedOrder.Count - 1].PercentGain/2) > greatestVal)
                    {
                        lowestName = skill.Key;
                        lowestLevel = skill.Value[0].Level;
                        greatestVal = skill.Value[0].PercentGain + 0.05;
                    }
                }
                optimizedOrder.Add(skillsMapped[lowestName][0]);
                skillsMapped[lowestName].RemoveAt(0);
            }

            const double fuzzyThreshold = 0.3; 
            for (var i = 1; i < optimizedOrder.Count - 1; i++)
            {
                if (optimizedOrder[i].SkillName != optimizedOrder[i-1].SkillName &&
                    optimizedOrder[i].SkillName != optimizedOrder[i+1].SkillName &&
                    optimizedOrder[i-1].SkillName == optimizedOrder[i+1].SkillName)
                {
                    if (optimizedOrder[i].PercentGain - fuzzyThreshold < optimizedOrder[i+1].PercentGain)
                    {
                        var origSwap = optimizedOrder[i];
                        optimizedOrder[i] = optimizedOrder[i+1];
                        optimizedOrder[i+1] = origSwap;
                    }
                }
            }

            var compressedOrder = new List<SkillOrder>();
            var lastSkill = optimizedOrder[0];
            foreach (var skill in optimizedOrder.Skip(1))
            {
                if(skill.SkillName != lastSkill.SkillName)
                    compressedOrder.Add(lastSkill);
                lastSkill = skill;
            }
            compressedOrder.Add(lastSkill);

            return compressedOrder.ToArray();
        }
    }
}
