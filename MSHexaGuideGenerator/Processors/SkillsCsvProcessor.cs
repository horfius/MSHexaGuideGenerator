using Microsoft.VisualBasic.FileIO;
using MSHexaGuideGen.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSHexaGuideGen.Processors
{
    public class SkillsCsvProcessor
    {
        private string CsvFilePath {  get; set; }

        public SkillsCsvProcessor(string csvFilePath)
        {
            if (!Path.Exists(csvFilePath)) 
                throw new ArgumentException(nameof(csvFilePath));

            CsvFilePath = csvFilePath;
        }

        public List<SkillOrder> Process()
        {
            var skills = new List<SkillOrder>();
            using (TextFieldParser parser = new TextFieldParser(CsvFilePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                var line = 1;
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields() ?? [];
                    if (fields.Length != SkillOrder.RowFieldsCount)
                        throw new Exception($"Incorrect number in file '{CsvFilePath}' at line {line}");

                    skills.Add(new SkillOrder(fields[0].Trim(), int.Parse(fields[1].Trim()), double.Parse(fields[2].Trim())));
                    line++;
                }
            }

            return skills;
        }
    }
}
