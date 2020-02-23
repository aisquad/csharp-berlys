using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace capp1
{
    struct Customer
    {
        internal int id;
        internal string name;
        internal string town;
        internal float volume;
        internal int ordNum;
    }

    struct Route
    {
        internal int id;
        internal string name;
        internal DateTime deliveryDate;
        internal int deliveries;
        internal SortedDictionary<int, Customer> customers;
        internal float volumeAmt;
        internal float weightAmt;
        internal float truckCapacity;
    }

    class FilenameHandler
    {
        private string _path;

        public FilenameHandler()
        {
            DataDirPath = @"C:\Users\coetg\OneDrive\Scripts\data\berlys\";
            AttachmentsDirPath = @"C:\Users\coetg\OneDrive\Scripts\data\attachments\";
        }

        public string DataDirPath { get; private set; }

        public string AttachmentsDirPath { get; }

        public string Filename { get; set; }

        public string AbsoluteFilename { get; private set; }

        public void AppendToDataDirPath(string path)
        {
            DataDirPath += path;
            AbsoluteFilename = DataDirPath + Filename;
        }
        public string ToDataDir()
        {
            _path = DataDirPath;
            AbsoluteFilename = _path + Filename;
            return AbsoluteFilename;
        }

        public string ToAttachementsDir()
        {
            _path = AttachmentsDirPath;
            AbsoluteFilename = _path + Filename;
            return AbsoluteFilename;
        }
    }

    class Convert
    {
        public int ToInt(Group group)
        {
            return int.Parse(group.Value);
        }

        public float ToFloat(Group group)
        {
            return float.Parse(group.Value);
        }

        public string ToString(Group group)
        {
            return group.Value.Trim();
        }

        public DateTime ToDateTime(Group group)
        {
            return DateTime.ParseExact(group.Value, "dd.mm.yyyy", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
    class Berlys
    {
        private const string Pattern = @"25\s+BERLYS ALIMENTACION S\.A\.U\s+[\d:]+\s+[\d.]+\s+"
                + @"Volumen de pedidos de la ruta :\s+(?<routeID>\d+)\s+25 (?<routeName>.+?)\s+"
                + @"Día de entrega :\s+(?<deliveryDate>[^ ]{10})(?<customers>.+?)"
                + @"NUMERO DE CLIENTES\s+:\s+(?<custNum>\d+).+?"
                + @"SUMA VOLUMEN POR RUTA\s+:\s+(?<volAmt>[\d,.]+) (?<um1>(?:PVL|KG)).+?"
                + @"SUMA KG POR RUTA\s+:\s+(?<weightAmt>[\d,.]+) (?<um2>(?:PVL|KG)).+?"
                + @"(?:CAPACIDAD TOTAL CAMIÓN\s+:\s+(?<truckCap>[\d,.]+) (?<um3>(?:PVL|KG)))?";
        private List<int> knownRouteIDs = new List<int> { 678, 679, 680, 681, 682, 686, 688, 696 };

        private SortedDictionary<int, Customer> FetchCustomers(string content)
        {
            SortedDictionary<int, Customer> customers = new SortedDictionary<int, Customer> { };
            Regex regexp = new Regex(@"(?<custID>\d{10}) (?<custName>.{35}) (?<town>.{20}) (?<ordNum>\d{10}) (?<vol>[\d,.\s]{11})(?: PVL)?");
            MatchCollection matches = regexp.Matches(content);
            var i = 0;
            Convert convert = new Convert();
            foreach (Match match in matches)
            {
                GroupCollection group = match.Groups;
                int validID = convert.ToInt(group["custID"]);

                Customer customer = new Customer
                {
                    id = validID,
                    name = convert.ToString(group["custName"]),
                    town = convert.ToString(group["town"]),
                    volume = convert.ToFloat(group["vol"]),
                    ordNum = convert.ToInt(group["ordNum"])
                };

                if (customers.ContainsKey(validID))
                {
                    customer.volume += customers[validID].volume;
                    customers[validID] = customer;
                }
                else
                {
                    customers.Add(validID, customer);
                }

            }
            return customers;
        }

        public SortedDictionary<int, Route> FetchRoutes(string content)
        {
            SortedDictionary<int, Route> routes = new SortedDictionary<int, Route> { };
            Regex regexp = new Regex(Pattern, RegexOptions.Singleline);
            MatchCollection matches = regexp.Matches(content);
            Convert convert = new Convert();
            foreach (Match match in matches)
            {
                GroupCollection group = match.Groups;
                int routeID = convert.ToInt(group["routeID"]);
                if (!knownRouteIDs.Contains(routeID)) continue;
                
                SortedDictionary<int, Customer> customers = new SortedDictionary<int, Customer> { };
                customers = FetchCustomers(group["customers"].Value);

                float truckCap = group["truckCap"].Value == "" ? 0.0f : convert.ToFloat(group["truckCap"]);
                Route route = new Route
                {
                    id = routeID,
                    name = convert.ToString(group["routeName"]),
                    deliveryDate = convert.ToDateTime(group["deliveryDate"]),
                    deliveries = convert.ToInt(group["custNum"]),
                    customers = customers,
                    volumeAmt = convert.ToFloat(group["volAmt"]),
                    weightAmt = convert.ToFloat(group["weightAmt"]),
                    truckCapacity = truckCap
                };
                routes.Add(routeID, route);
            }
            return routes;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            FilenameHandler f = new FilenameHandler { Filename = "2020-02-20.txt" };
            f.AppendToDataDirPath(@"2020\02\");
            System.Console.WriteLine(f.ToDataDir());


            string[] lines = System.IO.File.ReadAllLines(f.ToDataDir());
            System.Console.WriteLine("This file has {0} lines", lines.Length);
            string content = System.IO.File.ReadAllText(f.ToDataDir());
            Berlys berlys = new Berlys();
            SortedDictionary<int, Route> routes = berlys.FetchRoutes(content);

            foreach (int routeID in routes.Keys)
            {
                Route route = routes[routeID];
                System.Console.WriteLine("Route: {0} DeliveryDate: {1} Customers: {2}\n", route.id, String.Format("{0:dd.mm.yyyy}", route.deliveryDate), route.deliveries);
                var i = 0;
                foreach(int custID in route.customers.Keys)
                {
                    Customer customer = route.customers[custID];
                    System.Console.WriteLine("{0}\t{1}\t{2}\t{3}", ++i, customer.name, customer.volume, customer.town);
                }
                System.Console.WriteLine("\n");
            }

            System.Console.WriteLine(float.Parse("0,002"));
            System.Console.WriteLine(float.Parse("0.002", CultureInfo.InvariantCulture.NumberFormat));
            // Suspend the screen.  
            System.Console.ReadLine();
        }

    }
}
