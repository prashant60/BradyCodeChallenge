using System.Xml.Serialization;

namespace BradyCodeChallenge
{
    [XmlRoot("GenerationOutput")]
    public class GenerationOutput
    {
        [XmlElement("Totals")]
        public Totals Totals { get; set; }

        [XmlElement("MaxEmissionGenerators")]
        public MaxEmissionGenerators MaxEmissionGenerators { get; set; }

        [XmlElement("ActualHeatRates")]
        public ActualHeatRates ActualHeatRates { get; set; }
    }

    public class Totals
    {
        [XmlElement("Generator")]
        public List<Generator> Generators { get; set; }
    }

    public class Generator
    {
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Total")]
        public double Total { get; set; }
    }

    public class MaxEmissionGenerators
    {
        [XmlElement("Day")]
        public List<MEGDay> Days { get; set; }
    }

    public class MEGDay
    {
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Date")]
        public string Date { get; set; }

        [XmlElement("Emission")]
        public double Emission { get; set; }
    }

    public class ActualHeatRates
    {
        [XmlElement("ActualHeatRate")]
        public List<ActualHeatRate> ActualHeatRate { get; set; }
    }

    public class ActualHeatRate
    {
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("HeatRate")]
        public double HeatRate { get; set; }
    }

}
