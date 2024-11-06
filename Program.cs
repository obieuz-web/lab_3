using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace lab_3
{
    internal class Program
    {
        static XmlSerializer serializer;
        static string cars_file_path;
        static void Main(string[] args)
        {
            List<Car> myCars = CreateCarsList();

            #region ZAD1

            var query = myCars.Where(car => car.model == "A6").Select(car => new 
            { 
                engineType = car.motor.model == "TDI" ? "diesel" : "petrol",  
                hppl = car.motor.horsePower / car.motor.displacement
            });

            var query1 = query.GroupBy(e => e.engineType);

            foreach (var group in query1)
            {
                var averageHppl = group.Average(e => e.hppl);
                Console.WriteLine($"{group.Key}: {averageHppl}");
            }

            #endregion

            #region ZAD2

            cars_file_path = "cars.xml";

            if (!File.Exists(cars_file_path))
            {
                using (FileStream fs = File.Create(cars_file_path)) ;
            }

            XmlRootAttribute rootAttribute = new XmlRootAttribute("cars");
            serializer = new XmlSerializer(typeof(List<Car>), rootAttribute);

            SerializeCars(myCars);

            Console.WriteLine("Samochody z pliku");
            foreach (var item in DeserializeCars())
            {
                Console.WriteLine($"{item.model} \t {item.year} \t {item.motor.model}");
            }

            #endregion

            #region ZAD3

            Console.WriteLine($"Średnia ilość koni mechanicznych {CountAverageHorsePower()}");


            Console.WriteLine("Modele samochodów bez powtórzeń");
            foreach( var i in GetDistinctCars())
            {
                Console.WriteLine(i);
            }


            #endregion

            #region ZAD4

            createXmlFromLinq(myCars);

            #endregion

            #region ZAD5

            createTableFromLinq(myCars);

            #endregion

            Console.ReadKey();
        }
        static List<Car> CreateCarsList()
        {
            return new List<Car>(){
             new Car("E250", new Engine(1.8, 204, "CGI"), 2009),
             new Car("E350", new Engine(3.5, 292, "CGI"), 2009),
             new Car("A6", new Engine(2.5, 187, "FSI"), 2012),
             new Car("A6", new Engine(2.8, 220, "FSI"), 2012),
             new Car("A6", new Engine(3.0, 295, "TFSI"), 2012),
             new Car("A6", new Engine(2.0, 175, "TDI"), 2011),
             new Car("A6", new Engine(3.0, 309, "TDI"), 2011),
             new Car("S6", new Engine(4.0, 414, "TFSI"), 2012),
             new Car("S8", new Engine(4.0, 513, "TFSI"), 2012)
            };
        }
        static void SerializeCars(List<Car> cars)
        {
            using (StreamWriter writer = new StreamWriter(cars_file_path))
            {
                serializer.Serialize(writer, cars);
            }
        }
        static List<Car> DeserializeCars()
        {
            using (StreamReader reader = new StreamReader(cars_file_path))
            {
                return (List<Car>)serializer.Deserialize(reader);
            }
        }
        static double CountAverageHorsePower()
        {
            XElement rootNode = XElement.Load(cars_file_path);

            double sum = (double)rootNode.XPathEvaluate("sum(//car/engine[@model != 'TDI']/horsePower)");
            double count = (double)rootNode.XPathEvaluate("count(//car/engine[@model != 'TDI']/horsePower)");

            return sum / count;
        }
        static List<string> GetDistinctCars()
        {
            List<string> modelsDistinct = new List<string>();

            XElement rootNode = XElement.Load(cars_file_path);

            var models = rootNode.XPathSelectElements("//car/model");

            var mod = models.GroupBy(m => m.Value).Select(group => group.First()).ToList();

            foreach (var item in mod)
            {
                modelsDistinct.Add(item.Value);
            }

            return modelsDistinct;
        }
        static void createXmlFromLinq(List<Car> myCars)
        {
            IEnumerable<XElement> nodes = myCars.Select(car =>
            {
                return new XElement("car",
                    new XElement("model", car.model),
                    new XElement("engine",
                        new XAttribute("model",car.motor.model),
                        new XElement("displacement", car.motor.displacement),
                        new XElement("horsePower", car.motor.horsePower)
                    ),
                    new XElement("year", car.year)
                );
            });
            XElement rootNode = new XElement("cars", nodes);

            string filePath = "CarsFromLinq.xml";

            if(!File.Exists(filePath))
            {
                using (FileStream fs = File.Create(filePath)) ;
            }
            rootNode.Save(filePath);
            Console.WriteLine($"Stworzono plik {filePath}");
        }
        static void createTableFromLinq(List<Car> cars)
        {
            IEnumerable<XElement> nodes = cars.Select(car =>
            {
                return new XElement("tr",
                    new XElement("td", car.model),
                    new XElement("td", car.motor.model),
                    new XElement("td", car.motor.displacement),
                    new XElement("td", car.motor.horsePower),
                    new XElement("td", car.year)
                );
            });
            XElement rootNode = new XElement("table", nodes);

            string filePath = "tabela.xhtml";

            if (!File.Exists(filePath))
            {
                using (FileStream fs = File.Create(filePath)) ;
            }

            XNamespace xmlns = "http://www.w3.org/1999/xhtml";

            XDocument table = XDocument.Parse(
                "<!--?xml version=\"1.0\" encoding=\"utf-8\"?-->\r\n<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\"\r\n\"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"pl\" lang=\"pl\"><head>\r\n <title>Cars</title>\r\n <meta http-equiv=\"content-type\" content=\"text/html; charset=UTF-8\"/>\r\n </head>\r\n <body>\r\n </body>\r\n</html>"
                );

            table.Root.Element(xmlns + "body").Add(rootNode);

            table.Save(filePath);

            Console.WriteLine($"Stworzono tabele w pliku {filePath}");
        }
    }
    [XmlType("car")]
    public class Car
    {
        public string model { get; set; }
        public int year { get; set; }
        [XmlElement("engine")]
        public Engine motor { get; set; }
        public Car(string model, Engine motor, int year)
        {
            this.model = model;
            this.year = year;
            this.motor = motor;
        }
        public Car() { }
    }
    [XmlRoot("engine")]
    public class Engine
    {
        public double displacement { get; set; }
        public double horsePower { get; set; }
        [XmlAttribute("model")]
        public string model { get; set; }

        public Engine(double _displacement, double _horsePower, string _model)
        {
            displacement = _displacement;
            horsePower = _horsePower;
            model = _model;
        }
        public Engine() { }
    }
}
