using BradyCodeChallenge;
using System.Configuration;
using System.Xml;
using System.Xml.Serialization;

string inputFilePath = ConfigurationManager.AppSettings["inputFolderPath"];
string outputFilePath = ConfigurationManager.AppSettings["outputFolderPath"];
string fileName = "";

if (inputFilePath == null || outputFilePath == null)
{
    return;
}

Console.WriteLine("Waiting for a new file in the input folder...");

FileSystemWatcher watcher = new FileSystemWatcher()
{
    Path = inputFilePath,
    EnableRaisingEvents = true,
    NotifyFilter = NotifyFilters.FileName,
    Filter = "*.xml",
};
watcher.Created += new FileSystemEventHandler(OnChanged);

void OnChanged(object sender, FileSystemEventArgs e)
{
    Console.WriteLine($"New file added in the input folder: {e.Name}");
    using (FileStream fs = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read))
    {
        fileName = e.Name.Split(".")[0];
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(e.FullPath);
        string xmlContent = xmlDoc.InnerXml;

        var serializer = new XmlSerializer(typeof(GenerationReport));
        using (StringReader reader = new StringReader(xmlContent))
        {
            GenerationReport generationReport = (GenerationReport)serializer.Deserialize(reader);
            GenerateOutput(generationReport);
        }
    }
}

void GenerateOutput(GenerationReport generationReport)
{
    Console.WriteLine("Generating output...");
    GenerationOutput generationOutput = new GenerationOutput();
    CalculateTotalGenerationValue(generationReport, generationOutput);
    CalculateMaximumEmissionsValue(generationReport, generationOutput);
    CalculateActualHeatRate(generationReport, generationOutput);
    ConvertToXMLAndDownload(generationOutput);
}

void CalculateTotalGenerationValue(GenerationReport generationReport, GenerationOutput generationOutput)
{
    Totals generationTotals = new Totals
    {
        Generators = new List<Generator>()
    };

    foreach (var generator in generationReport.Wind.WindGenerators)
    {
        var generatorName = generator.Name;
        var totalValue = Math.Round(CalculateGeneratorTotal(generator.Generation, generatorName == "Wind[Offshore]" ? ReferenceData.OffShoreWind_VF : ReferenceData.OnShoreWind_VF), 9);

        generationTotals.Generators.Add(new Generator
        {
            Name = generator.Name,
            Total = totalValue
        });
    }

    var gasGenerator = generationReport.Gas.GasGenerators.First();
    generationTotals.Generators.Add(new Generator
    {
        Name = gasGenerator.Name,
        Total = Math.Round(CalculateGeneratorTotal(gasGenerator.Generation, ReferenceData.Gas_VF), 9)
    });

    var coalGenerator = generationReport.Coal.CoalGenerators.First();
    generationTotals.Generators.Add(new Generator
    {
        Name = coalGenerator.Name,
        Total = Math.Round(CalculateGeneratorTotal(coalGenerator.Generation, ReferenceData.Coal_VF), 9)
    });

    generationOutput.Totals = generationTotals;
}

double CalculateGeneratorTotal(Generation generation, double valueFactor)
{
    var totalValue = 0.0;
    foreach (var day in generation.Days)
    {
        totalValue += day.Energy * day.Price * valueFactor;
    }
    return totalValue;
}

void CalculateMaximumEmissionsValue(GenerationReport generationReport, GenerationOutput generationOutput)
{
    var maxEmissions = new MaxEmissionGenerators
    {
        Days = new List<MEGDay>()
    };
    var gasGenerator = generationReport.Gas.GasGenerators.First();
    var coalGenerator = generationReport.Coal.CoalGenerators.First();

    foreach (var day in gasGenerator.Generation.Days)
    {
        maxEmissions.Days.Add(new MEGDay
        {
            Name = gasGenerator.Name,
            Date = day.Date,
            Emission = Math.Round(day.Energy * gasGenerator.EmissionsRating * ReferenceData.Gas_EF, 9)
        });
    }

    foreach (var day in coalGenerator.Generation.Days)
    {
        var emission = Math.Round(day.Energy * coalGenerator.EmissionsRating * ReferenceData.Coal_EF, 9);
        
        var currentData = maxEmissions.Days.Find(e => e.Date == day.Date);
        if (currentData == null)
        {
            maxEmissions.Days.Add(new MEGDay
            {
                Name = coalGenerator.Name,
                Date = day.Date,
                Emission = emission
            });
        }
        else
        {
            if (currentData.Emission < emission)
            {
                currentData.Name = coalGenerator.Name;
                currentData.Emission = emission;
            }
        }
    }

    generationOutput.MaxEmissionGenerators = maxEmissions;
}

void CalculateActualHeatRate(GenerationReport generationReport, GenerationOutput generationOutput)
{
    var heatRate = new ActualHeatRates
    {
        ActualHeatRate = new List<ActualHeatRate>()
    };

    foreach (var generator in generationReport.Coal.CoalGenerators)
    {
        heatRate.ActualHeatRate.Add(new ActualHeatRate
        {
            Name = generator.Name,
            HeatRate = generator.TotalHeatInput / generator.ActualNetGeneration
        });
    }
    generationOutput.ActualHeatRates = heatRate;
}

void ConvertToXMLAndDownload(GenerationOutput generationOutput)
{
    Console.WriteLine("Downloading output file in output folder...");

    XmlDocument document = new XmlDocument();
    var serializer = new XmlSerializer(typeof(GenerationOutput));

    if (!Directory.Exists(outputFilePath))
    {
        Directory.CreateDirectory(outputFilePath);
    }
    using (StreamWriter writer = new StreamWriter($"{outputFilePath}/{fileName}-Result.xml"))
    {
        serializer.Serialize(writer, generationOutput);
    }

    Console.WriteLine($"Download Complete: {fileName}-Result.xml");
    Console.WriteLine("Waiting for a new file in the input folder...");
}



Console.ReadLine();