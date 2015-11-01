using System;
using System.Linq;
using System.Threading.Tasks;

// Needed for DocumentDB
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using DocumentDBDemo.Documents;

namespace DocumentDBDemo
{
    /// <summary>
    /// Author: Andrew Hoh
    /// Source: https://azure.microsoft.com/en-us/documentation/articles/documentdb-get-started/
    /// Modified by: Stamo Petkov
    /// Date modified: 30.10.2015
    /// SoftUni conference in Chepelare
    /// </summary>
    class FamilyRegistry
    {
        // Avoid keys in your code. Use connection strings instead!
        private const string EndpointUrl = "https://suconf.documents.azure.com:443/";
        private const string AuthorizationKey = "36RXJuTGzH+H/QrmF1c9wDKVaD1D3tgFuOMG4VHy+ke+u5k/14PgbG/u3OOMzovviAJJSvc2zqXUCDYGDmxzcQ==";

        private static DocumentClient client;
        private static Database database;
        private static DocumentCollection documentCollection;

        static void Main(string[] args)
        {
            try
            {
                GetStartedDemo().Wait();
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
        }

        private static async Task GetStartedDemo()
        {
            // Create a new instance of the DocumentClient
            client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey);

            database = await CreateDatabaseAsync();

            await CreateCollectionAsync();

            await SeedDocuments();

            await SampleSqlQuery();

            await SampleLinqQuery();

            await SampleLambdaQuery();

            Console.WriteLine("Press any key to delete database...");
            Console.ReadKey();
            
            await client.DeleteDatabaseAsync("dbs/" + database.Id);
            client.Dispose();
        }

        /// <summary>
        /// Check to verify a database with the id=FamilyRegistry does not exist
        /// If the database does not exist, create a new database
        /// </summary>
        /// <returns>Database object</returns>
        private static async Task<Database> CreateDatabaseAsync()
        {
            Database database = client
                .CreateDatabaseQuery()
                .Where(db => db.Id == "FamilyRegistry")
                .AsEnumerable()
                .FirstOrDefault();

            if (database == null)
            {
                database = await client.CreateDatabaseAsync(
                    new Database
                    {
                        Id = "FamilyRegistry"
                    });

                // Write the new database's id to the console
                Console.WriteLine("{0} created!", database.Id);
                Console.WriteLine("Press any key to continue ...");
                Console.ReadKey();
                Console.Clear();
            }

            return database;
        }

        /// <summary>
        /// Check to verify a document collection with the id=FamilyCollection does not exist
        /// If the document collection does not exist, create a new collection
        /// </summary>
        private static async Task CreateCollectionAsync()
        {
            documentCollection = client
                .CreateDocumentCollectionQuery("dbs/" + database.Id)
                .Where(c => c.Id == "FamilyCollection")
                .AsEnumerable()
                .FirstOrDefault();

            if (documentCollection == null)
            {
                documentCollection = await client.CreateDocumentCollectionAsync("dbs/" + database.Id,
                    new DocumentCollection
                    {
                        Id = "FamilyCollection"
                    });

                // Write the new collection's id to the console
                Console.WriteLine("{0} created!", documentCollection.Id);
                Console.WriteLine("Press any key to continue ...");
                Console.ReadKey();
                Console.Clear();
            }
        }
        /// <summary>
        /// Seeds two documents to DB
        /// </summary>
        private static async Task SeedDocuments()
        {
            // Check to verify a document with the id=FamilyPetkovi does not exist
            Document document = client
                .CreateDocumentQuery("dbs/" + database.Id + "/colls/" + documentCollection.Id)
                .Where(d => d.Id == "FamilyPetkovi")
                .AsEnumerable()
                .FirstOrDefault();

            // If the document does not exist, create a new document
            if (document == null)
            {
                // Create the Petkovi Family document
                Family petkoviFamily = new Family
                {
                    Id = "FamilyPetkovi",
                    LastName = "Petkov",
                    Parents = new Parent[] {
                        new Parent { FirstName = "Stamo", Gender = "male" },
                        new Parent { FirstName = "Inga", Gender = "female"}
                    },
                    Children = new Child[] {
                        new Child
                        {
                            FirstName = "George",
                            Gender = "male",
                            Grade = 6
                        },
                        new Child
                        {
                            FirstName = "Antonia",
                            Gender = "female",
                            Grade = 3,
                            Pets = new Pet[] {
                                new Pet { GivenName = "Rio" }
                            }
                        }
                    },
                    Address = new Address { County = "Bulgaria", City = "Sofia" },
                    IsRegistered = true
                };

                // id based routing for the first argument, "dbs/FamilyRegistry/colls/FamilyCollection"
                await client.CreateDocumentAsync("dbs/" + database.Id + "/colls/" + documentCollection.Id, petkoviFamily);
            }

            // Check to verify a document with the id=FamilyIvanovi does not exist
            document = client
                .CreateDocumentQuery("dbs/" + database.Id + "/colls/" + documentCollection.Id)
                .Where(d => d.Id == "FamilyIvanovi")
                .AsEnumerable()
                .FirstOrDefault();

            if (document == null)
            {
                // Create the Petkovi Family document
                Family ivanoviFamily = new Family
                {
                    Id = "FamilyIvanovi",
                    LastName = "Ivanov",
                    Parents = new Parent[] {
                        new Parent { FirstName = "Krum", Gender = "male" },
                        new Parent
                        {
                            FirstName = "Milena",
                            FamilyName = "Mineva",
                            Gender = "female"
                        }
                    },
                    Children = new Child[] {
                        new Child
                        {
                            FirstName = "Alexandra",
                            Gender = "female",
                            Grade = 9,
                            Pets = new Pet[] {
                                new Pet { GivenName = "Kenai" }
                            }
                        },
                        new Child
                        {
                            FirstName = "George",
                            Gender = "male",
                            Grade = 5
                        }
                    },
                    Address = new Address { County = "Englnd", City = "Coventry" },
                    IsRegistered = true
                };

                // id based routing for the first argument, "dbs/FamilyRegistry/colls/FamilyCollection"
                await client.CreateDocumentAsync("dbs/" + database.Id + "/colls/" + documentCollection.Id, ivanoviFamily);
            }
        }

        /// <summary>
        /// DocumentDB SQL Query
        /// </summary>
        private static async Task SampleSqlQuery()
        {
            var families = client
                .CreateDocumentQuery("dbs/" + database.Id + "/colls/" + documentCollection.Id,
                "SELECT * " +
                "FROM Families f " +
                "WHERE f.id = \"FamilyPetkovi\"");

            foreach (var family in families)
            {
                Console.WriteLine("\tRead {0} from SQL", family);
            }
        }

        /// <summary>
        /// DocumentDB Linq query
        /// </summary>
        /// <returns></returns>
        private static async Task SampleLinqQuery()
        {
            var families =
                from f in client
                    .CreateDocumentQuery("dbs/" + database.Id + "/colls/" + documentCollection.Id)
                where f.Id == "FamilyPetkovi"
                select f;

            foreach (var family in families)
            {
                Console.WriteLine("\tRead {0} from LINQ", family);
            }
        }

        /// <summary>
        /// DocumentDB Linq Lambda expressions
        /// </summary>
        /// <returns></returns>
        private static async Task SampleLambdaQuery()
        {
            var families = client
                .CreateDocumentQuery("dbs/" + database.Id + "/colls/" + documentCollection.Id)
                .Where(f => f.Id == "FamilyIvanovi")
                .Select(f => f);

            foreach (var family in families)
            {
                Console.WriteLine("\tRead {0} from LINQ Lambda", family);
            }
        }
    }
}
