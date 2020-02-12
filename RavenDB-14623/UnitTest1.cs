using Raven.Client.Documents;
using Raven.TestDriver;
using Xunit;
using System.Linq;
using Raven.Client.Documents.Indexes;
using RavenDB_14623.JSIndex;

namespace RavenDB_14623
{
    public class RavenDBTestDriver : RavenTestDriver
    {
        //This allows us to modify the conventions of the store we get from 'GetDocumentStore'
        protected override void PreInitialize(IDocumentStore documentStore)
        {
            documentStore.Conventions.MaxNumberOfRequestsPerSession = 50;
        }

        [Fact]
        public void StoreAllFields_JS_Index_Test()
        {
            ConfigureServer(new TestServerOptions
            {
                DataDirectory = "C:\\RavenDBTestDir"
            });

            TestDocument documentInInterest = new TestDocument { Name = "Hello world!", ArrayOfStrings = new string[] { "123", "234", "345" } };

            using (var store = GetDocumentStore())
            {
                store.ExecuteIndex(new Some_JSINdex());
                using (var session = store.OpenSession())
                {
                    session.Store(documentInInterest);
                    session.Store(new TestDocument { Name = "Goodbye...", ArrayOfStrings = new string[] { "qwe", "asd", "zxc" } });
                    session.SaveChanges();
                }
                WaitForIndexing(store); //If we want to query documents sometime we need to wait for the indexes to catch up
                
                // WaitForUserToContinueTheTest(store);//Sometimes we want to debug the test itself, this redirect us to the studio
                
                using (var session = store.OpenSession())
                {
                    var query = session.Query<Some_JSINdex.Result, Some_JSINdex>()
                        .Where(x => x.NameButDifferentName == "Hello world!")
                        .ProjectInto<Some_JSINdex.Result>(); // we are interested in the mapped Document, our original document is actually quite big and not interesting

                    // Execute DB call
                    var result = query.ToList();
                    
                    // we are able to query by the NameButDifferentName property, which is indexed
                    Assert.Single(query);

                    var documentReturned = query.First();

                    // First Issue, the NameButDifferentName is not stored, despite the __all_fields config in the index
                    Assert.Equal(documentInInterest.Name, documentReturned.NameButDifferentName);
                    
                    // Second Issue, since we added a search config on this property it is stored, but with an incorrect type
                    // we expect it to be array of string, however it is only string
                    var typeAfterStore = documentReturned.NewPropertyName.GetType();
                    Assert.Equal(typeof(string[]), typeAfterStore);

                }
            }
        }
    }

    public class TestDocument
    {
        public string Name { get; set; }
        public string[] ArrayOfStrings { get; set; }
    }
}