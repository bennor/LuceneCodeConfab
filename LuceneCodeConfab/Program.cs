using System;
using System.Collections;
using System.Collections.Generic;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace LuceneCodeConfab
{
    internal class Program
    {
        /// <summary>
        /// Our example "documents".
        /// </summary>
        protected static string[] SourceDocuments = new[]
                {
                    "mary had a little lamb",
                    "the quick brown fox jumped over the lazy dog",
                    "the fox ate mary's lamb"
                };

        private static void Main(string[] args)
        {
            Console.WriteLine("==> HandMadeInvertedIndexExample");
            HandMadeInvertedIndexExample();
            Console.WriteLine();

            Console.WriteLine("==> GeneratedInvertedIndexExample");
            GeneratedInvertedIndexExample();
            Console.WriteLine();

            Console.WriteLine("==> BasicLuceneExample");
            BasicLuceneExample();

            Console.ReadLine();
        }

        private static void HandMadeInvertedIndexExample()
        {
            // example of the theoretical structure of an inverted index (based on example text in PPT)
            var invertedIndex = new Dictionary<string, BitArray>
                {
                    //"a":      {2}
                    {"a", new BitArray(new[] {false, true, false})},

                    //"banana": {2}
                    {"banana", new BitArray(new[] {false, true, false})},

                    //"is":     {0, 1, 2}
                    {"is", new BitArray(new[] {true, true, true})},

                    //"it":     {0, 1, 2}
                    {"it", new BitArray(new[] {true, true, true})},

                    //"what":   {0, 1}
                    {"what", new BitArray(new[] {true, true, false})},
                };

            // "what is it"
            var matches = invertedIndex["what"].And(invertedIndex["is"]).And(invertedIndex["it"]);

            for (var i = 0; i < matches.Length; i++)
            {
                if (matches[i])
                {
                    Console.WriteLine("  matched doc - {0}", i);
                }
            }
        }

        private static void GeneratedInvertedIndexExample()
        {
            // Create an inverted index from the example source documents
            var invertedIndex = new Dictionary<string, BitArray>();

            for (var i = 0; i < SourceDocuments.Length; i++)
            {
                foreach (var term in SourceDocuments[i].Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!invertedIndex.ContainsKey(term))
                    {
                        invertedIndex[term] = new BitArray(200); // it's a demo, 200 because I know it's big enough :)
                    }
                    invertedIndex[term].Set(i, true);
                }
            }

            // example query over the inverted index
            // "fox lamb" - AND query
            Console.WriteLine("Searching for documents containing 'fox' AND 'lamb'");
            var matches = invertedIndex["fox"].And(invertedIndex["lamb"]);
            for (var i = 0; i < matches.Length; i++)
            {
                if (matches[i])
                {
                    Console.WriteLine("  matched doc - {0}", i);
                }
            }
        }

        private static void BasicLuceneExample()
        {
            // Create an index, first we need a place to store it (a Directory), then a writer to put documents in it
            var directory = new RAMDirectory(); // Usually use new FilesystemDirectory(...) to persist to disk
            using (var writer = new IndexWriter(directory, new WhitespaceAnalyzer(), create:true, mfl:IndexWriter.MaxFieldLength.UNLIMITED))
            {
                foreach (var source in SourceDocuments)
                {
                    // an IndexWriter stores a Document
                    var document = new Document();
                    // a documents consists of a set of fields which are broken into terms in the inverted index
                    document.Add(new Field("MyFieldName", source, Field.Store.NO, Field.Index.ANALYZED));

                    writer.AddDocument(document);
                }
                writer.Optimize();
            }

            // the basic building blocks of the inverted index powered document matching is available in an IndexReader which reads from a Directory
            var indexReader = IndexReader.Open(directory, readOnly: true);

            Console.WriteLine("Searching for documents containing 'the'");
            // Look up document numbers which match a given term from inverted index
            var termDocs = indexReader.TermDocs(new Term("MyFieldName", "the"));

            while (termDocs.Next())
            {
                Console.WriteLine("  matched document - {0}", termDocs.Doc);
            }


            Console.WriteLine("All terms stored in index");
            var termEnum = indexReader.Terms();

            while (termEnum.Next())
            {
                Console.WriteLine("  term - {0}", termEnum.Term.Text);
            }

            // Later we'll take a look at what an IndexSearcher on top of an IndexReader gives us (Query's) 
            // var searcher = new IndexSearcher(indexReader);
            //.....
        }
    }
}
