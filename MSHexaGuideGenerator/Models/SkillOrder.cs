using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MSHexaGuideGen.Models
{
    public record SkillOrder(string SkillName, int Level, double PercentGain)
    {
        [JsonIgnore]
        public const int RowFieldsCount = 3;
    }
}
