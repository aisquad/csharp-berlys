using ImapX;
using ImapX.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

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
        internal System.DateTime deliveryDate;
        internal int deliveries;
        internal Dictionary<int, Customer> customers;
        internal float volumeAmt;
        internal float weightAmt;
        internal float truckCapacity;
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

        public DateTime? docDate = null;

        private Dictionary<int, Customer> FetchCustomers(string content)
        {
            Dictionary<int, Customer> customers = new Dictionary<int, Customer> { };
            Regex regexp = new Regex(@"(?<custID>\d{10}) (?<custName>.{35}) (?<town>.{20}) (?<ordNum>\d{10}) (?<vol>[\d,.\s]{11})(?: PVL)?");
            MatchCollection matches = regexp.Matches(content);
            Convert convert = Convert.GetInstance;
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

        private Dictionary<int, Route> FetchRoutes(string content)
        {
            Dictionary<int, Route> routes = new Dictionary<int, Route> { };
            Regex regexp = new Regex(Pattern, RegexOptions.Singleline);
            MatchCollection matches = regexp.Matches(content);
            Convert convert = Convert.GetInstance;
            foreach (Match match in matches)
            {
                GroupCollection group = match.Groups;
                int routeID = convert.ToInt(group["routeID"]);
                if (!knownRouteIDs.Contains(routeID)) continue;

                Dictionary<int, Customer> customers = new Dictionary<int, Customer> { };
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
                if (docDate == null)
                {
                    docDate = route.deliveryDate;
                }
                routes.Add(routeID, route);
            }
            return routes;
        }

        public void Run()
        {
            FilenameHandler filenameHandler = new FilenameHandler();
            string filename = filenameHandler.FromDownloadsDir();
            Boolean saveFile = false;

            if (File.Exists(filename)) { saveFile = true; } else { filename = filenameHandler.FromDataDir(); }
            Console.WriteLine(filename);

            string[] lines = File.ReadAllLines(filename);
            Console.WriteLine($"This file contains {lines.Length} lines");

            string content = File.ReadAllText(filename);
            Dictionary<int, Route> routes = FetchRoutes(content);
            foreach (int routeID in routes.Keys)
            {
                Route route = routes[routeID];
                Console.WriteLine(
                    $"Route: {route.id} {route.name} DeliveryDate: {route.deliveryDate:dd.mm.yyyy} Customers: {route.deliveries}\n"
                );
                var i = 0;
                foreach (int custID in route.customers.Keys)
                {
                    Customer customer = route.customers[custID];
                    Console.WriteLine($"{++i}\t{customer.name}\t{customer.volume}\t{customer.town}");
                }
                Console.WriteLine("\n");
            }
            if (saveFile)
            {
                string oldFilename = filenameHandler.FromDownloadsDir();
                filenameHandler.Filename = $"{docDate:yyyy-mm-dd}.txt";
                filenameHandler.AppendToDataDirPath($@"{docDate:yyyy}\{docDate:mm}\");
                string newFilename = filenameHandler.ToDataDir();
                File.Move(oldFilename, newFilename, true);
            }

        }

        public void DownloadAttachments()
        {
            GetMail gmail = new GetMail();
            gmail.Run();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Berlys berlys = new Berlys();
            berlys.DownloadAttachments();
            //berlys.Run();

            // Suspend the screen.  
            System.Console.ReadLine();
        }

    }
}
