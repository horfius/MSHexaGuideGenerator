using MSHexaGuideGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MSHexaGuideGen.Models
{
    public record GuideCanvas(WebImageResource Background, string HeaderText, string ClassName, string Version, GuideLegend Legend)
    {
        public List<WebImageResource> SkillImages { get; set; } = new List<WebImageResource>();

        public WebImageResource GetImage(string skillName)
        {
            return SkillImages.Find(res => res.Name == skillName) ?? throw new Exception($"Could not find image matching skill name '{skillName}'");
        }
    }
}
