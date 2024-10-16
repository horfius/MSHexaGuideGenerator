// See https://aka.ms/new-console-template for more information
using MSHexaGuideGen.Models;
using MSHexaGuideGen.Processors;
using System.Drawing.Imaging;
using System.Text.Json;

if (!Path.Exists(WebImageResource.LocalFileStorePath))
    Directory.CreateDirectory(WebImageResource.LocalFileStorePath);

var guide = JsonSerializer.Deserialize<GuideCanvas>(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Design.json")));
if (guide == null)
{
    Console.WriteLine("Some error occurred while reading in Design.json, please check it's structure");
    return;
}

var allCsvFiles = Directory.GetFiles(Environment.CurrentDirectory, "*.csv");
foreach(var file in allCsvFiles)
{
    var orderingProcessor = new SkillsOrderingProcessor(new SkillsCsvProcessor(file).Process());
    var compressedOrdering = orderingProcessor.Process();
    Console.WriteLine($"Compressed skill count to {compressedOrdering.Length}");
    using var image = await new GuideImageProcessor(guide, compressedOrdering).Process();
    image.Save(Path.Combine(Environment.CurrentDirectory, "Guide.jpg"), ImageFormat.Jpeg);
}