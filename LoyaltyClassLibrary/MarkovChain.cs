using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MarkovReviews
{
    public class Review
    {
        public string reviewText { get; set; }

        public decimal reviewRating { get; set; }
    }

    public class WordPair
    {
        public int count { get; set; }
        public decimal probability { get; set; }
    }

    public class MarkovChain
    {
        private StringBuilder sentenceOut;

        //persist the random class throughout the life of the chain.
        //use the GUID hashcode to improve the default seed
        private Random rnd = new Random(Guid.NewGuid().GetHashCode());

        private Dictionary<string, Dictionary<string, WordPair>> chain { get; set; } = new Dictionary<string, Dictionary<string, WordPair>>();

        //especially on large lists, we want to calculate probabilities when the chain is fully constructed
        //the Generate method will check if we've trained with more words since the last calculation
        private bool recalcNeeded = true;

        public static List<string> Split(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            return Regex.Replace(text.ToLower(), @"[^0-9a-zA-Z']", " ").Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        }

        public void LoadTrainingJSON(IEnumerable<string> trainingJSON)
        {
            foreach(var line in trainingJSON)
            {
                ProcessLine(line);
            }
        }

        public void LoadTrainingJSON(string trainingJSON)
        {
            var reader = new StringReader(trainingJSON);
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                ProcessLine(line);
            }
        }

        private void ProcessLine(string line)
        {
            var jObj = (JObject)JsonConvert.DeserializeObject(line);
            var review = jObj.Children().Cast<JProperty>().Where(jp => jp.Name == "reviewText").FirstOrDefault();

            if (review != null)
                AddToChain(Split(review.Value.ToString()));
        }

        public void AddToChain(List<string> words)
        {
            //doing more training, reset recalc flag
            recalcNeeded = true;

            //Earlier, we stripped empty entries.  We are adding these now so we can determine probability of the first and last words of sentences.
            //The, A, An, etc. should be the most frequent and in logical sentence position by doing this
            words.Insert(0, "");
            words.Add("");

            var index = 0;

            foreach (var word in words)
            {
                if (index < words.Count - 1)
                {
                    Dictionary<string, WordPair> wordPairDictionary;

                    var firstWordFound = chain.TryGetValue(word, out wordPairDictionary);
                    var secondWordFound = false;
                    var secondWord = words.ElementAt(index + 1);

                    var wordPair = new WordPair();

                    if (firstWordFound == true)
                        secondWordFound = wordPairDictionary.TryGetValue(secondWord, out wordPair);

                    if (secondWordFound == true)
                    {
                        wordPairDictionary[secondWord].count++;
                    }
                    else
                    {
                        wordPair = new WordPair()
                        {
                            count = 1,
                            probability = 0
                        };

                        if (firstWordFound == false)
                        {
                            var dict = new Dictionary<string, WordPair>();
                            dict.Add(secondWord, wordPair);

                            chain.Add(word, dict);
                        }
                        else
                        {
                            wordPairDictionary.Add(secondWord, wordPair);
                        }
                    }

                    index++;
                }
            }
        }

        public Dictionary<string, Dictionary<string, WordPair>> GetChain()
        {
            return chain;
        }

        public string Generate(int minLength, int maxLength)
        {
            if (recalcNeeded)
                recalculateProbabilities(chain);

            sentenceOut = new StringBuilder();
            Generate("", rnd.Next(minLength, maxLength));
            return sentenceOut.ToString();
        }

        private void Generate(string firstWord, int length)
        {            
            sentenceOut.Append(sentenceOut.Length > 0 ? 
                                    " " + firstWord : 
                                    CultureInfo.CurrentCulture.TextInfo.ToTitleCase(firstWord));

            if (length > 0)
            {
                var diceRoll = (decimal)rnd.NextDouble();
                var probability = 0m;

                foreach (var link in chain[firstWord])
                {
                    probability += link.Value.probability;

                    if (probability > diceRoll)
                    {
                        //sentences will either terminate when length = 0
                        //or when the second word is blank (words during training at the end of sentences)
                        //a blank second word will make the Generate method believe it is starting a new sentence
                        //multiple ways to handle that scenario, but if (link.Key != "") should suffice
                        if (link.Key != "")
                            Generate(link.Key, length - 1);
                        
                        break;
                    }
                }
            }
        }

        public void recalculateProbabilities(Dictionary<string, Dictionary<string, WordPair>> chain)
        {
            foreach(var firstWord in chain.Keys)
            {
                var links = chain[firstWord];
                var total = links.Values.Sum(c => c.count);

                foreach(var link in links)
                {
                    link.Value.probability = (decimal)link.Value.count / (decimal)total;
                }
            }

            recalcNeeded = false;
        }
    }
}
