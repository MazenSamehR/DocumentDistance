using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DocumentDistance
{
    class DocDistance
    {
        // *****************************************
        // DON'T CHANGE CLASS OR FUNCTION NAME
        // YOU CAN ADD FUNCTIONS IF YOU NEED TO
        // *****************************************
        /// <summary>
        /// Write an efficient algorithm to calculate the distance between two documents
        /// </summary>
        /// <param name="doc1FilePath">File path of 1st document</param>
        /// <param name="doc2FilePath">File path of 2nd document</param>
        /// <returns>The angle (in degree) between the 2 documents</returns>

        private static List<string> Preprocessing(string input)
        {
            List<string> finalWords = new List<string>();
            StringBuilder cleanedWord = new StringBuilder();

            foreach (char c in input)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    cleanedWord.Append(char.ToLower(c));
                }
                else if (cleanedWord.Length > 0)
                {
                    finalWords.Add(Base64Encode(cleanedWord.ToString())); // Encode the word and add to the list
                    cleanedWord.Clear();
                }
            }

            if (cleanedWord.Length > 0)
            {
                finalWords.Add(Base64Encode(cleanedWord.ToString())); // Encode the last word and add to the list
            }

            return finalWords;
        }

        // Function to encode a string using Base64
        private static string Base64Encode(string plainText)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }


        private static Dictionary<string, double> CountFrequency(List<string> wordList)
        {
            Dictionary<string, double> wordFrequencies = new Dictionary<string, double>();

            foreach (string newWord in wordList)
            {
                wordFrequencies[newWord] = wordFrequencies.TryGetValue(newWord, out double value) ? value + 1 : 1;
            }

            return wordFrequencies;
        }



        private static double InnerProduct(Dictionary<string, double> D1, Dictionary<string, double> D2)
        {
            var commonKeys = D1.Keys.Intersect(D2.Keys);

            double sumOfProducts = commonKeys.Sum(key => D1[key] * D2[key]);

            return sumOfProducts;
        }


        public static double CalculateDistance(string doc1FilePath, string doc2FilePath)
        {
            // TODO comment the following line THEN fill your code here
            //throw new NotImplementedException();
            //read the documents in multiple threads
            string document1 = null;
            string document2 = null;

            Thread thread1 = new Thread(() => { document1 = File.ReadAllText(doc1FilePath); });
            Thread thread2 = new Thread(() => { document2 = File.ReadAllText(doc2FilePath); });

            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();

            // Corner Case 1: Identical Documents
            if (document1.Equals(document2, StringComparison.OrdinalIgnoreCase))
            {
                return 0.0;
            }

            List<string> wordList1 = null;
            List<string> wordList2 = null;

            Thread thread3 = new Thread(() => { wordList1 = Preprocessing(document1); });
            Thread thread4 = new Thread(() => { wordList2 = Preprocessing(document2); });

            thread3.Start();
            thread4.Start();

            thread3.Join();
            thread4.Join();

            // Corner Case 2: No Common Words
            if (!wordList1.Any() || !wordList2.Any())
            {
                return 90.0;
            }

            Dictionary<string, double> freqMapping1 = null;
            Dictionary<string, double> freqMapping2 = null;

            Thread thread5 = new Thread(() => { freqMapping1 = CountFrequency(wordList1); });
            Thread thread6 = new Thread(() => { freqMapping2 = CountFrequency(wordList2); });

            thread5.Start();
            thread6.Start();

            thread5.Join();
            thread6.Join();

            double innerProductValue = InnerProduct(freqMapping1, freqMapping2);

            double magnitude1 = Math.Sqrt(freqMapping1.Values.Sum(x => (double)x * (double)x));
            double magnitude2 = Math.Sqrt(freqMapping2.Values.Sum(x => (double)x * (double)x));

            double cosineSimilarity = innerProductValue / (magnitude1 * magnitude2);

            double distance = Math.Acos(Math.Max(-1.0, Math.Min(1.0, cosineSimilarity))) * (180.0 / Math.PI);
            return distance;
        }
    }
}