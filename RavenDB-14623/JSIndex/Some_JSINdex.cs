using Raven.Client.Documents.Indexes;
using System.Collections.Generic;

namespace RavenDB_14623.JSIndex
{
    public class Some_JSINdex : AbstractJavaScriptIndexCreationTask
    {
        private const string filenameprefix = nameof(Some_JSINdex);

        public class Result
        {
            public object NewPropertyName { get; set; } // supposed to be string[] but that will result in runtime JSON parsing errors
            public string NameButDifferentName { get; set; }
        }

        public Some_JSINdex()
        {
            Maps = new HashSet<string>()
            {
                ResourceUtils.GetResourceContent($"{filenameprefix}_Map.js")
            };

            Fields = new Dictionary<string, IndexFieldOptions>
            {
                {
                    "__all_fields", new IndexFieldOptions
                    {
                        Storage = FieldStorage.Yes,
                    }
                },
                {
                    "NewPropertyName", new IndexFieldOptions
                    {
                        Analyzer = "StandardAnalyzer",
                        Indexing = FieldIndexing.Search,
                    }
                },
            };

        }
    }
}
