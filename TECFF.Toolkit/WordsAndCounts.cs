using System;
using System.Collections.Generic;
using System.Text;

namespace TECFF.Toolkit
{
    [Serializable]
    public class WordsAndCounts
    {
        private Dictionary<string, double> wordsCounts =
            new Dictionary<string, double>();

        public void Add(string word, double count)
        {
            if (!this.wordsCounts.ContainsKey(word))
            {
                this.wordsCounts.Add(word, count);
            }
            else
            {
                double oldCount = this.wordsCounts[word];
                this.wordsCounts[word] = count + oldCount;
            }
        }

        public void AddAll(WordsAndCounts wordsAndCounts)
        {
            foreach (string word in wordsAndCounts.wordsCounts.Keys)
            {
                double count = wordsAndCounts.wordsCounts[word];
                this.Add(word, count);
            }
        }

        public WordAndCount[] AsSortedArray
        {
            get
            {
                WordAndCount[] wordsAndCounts = new WordAndCount[this.wordsCounts.Count];
                int index = 0;
                foreach (KeyValuePair<string, double> entry in this.wordsCounts)
                {
                    wordsAndCounts[index] = new WordAndCount(entry.Key, entry.Value);
                    index++;
                }
                Array.Sort<WordAndCount>(wordsAndCounts);

                return wordsAndCounts;
            }
        }

        public double this[string word]
        {
            get
            {
                if (this.wordsCounts.ContainsKey(word))
                {
                    double count = this.wordsCounts[word];
                    return count;
                }
                else
                {
                    return 0;
                }
            }

            set
            {
                this.wordsCounts[word] = value;
            }
        }

        public Dictionary<string, double>.KeyCollection Words
        {
            get
            {
                return this.wordsCounts.Keys;
            }
        }

        public Dictionary<string, double>.ValueCollection Counts
        {
            get
            {
                return this.wordsCounts.Values;
            }
        }

        public void RemoveWord(string word)
        {
            this.wordsCounts.Remove(word);
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("word; count\r\n");
            foreach (WordAndCount wc in this.AsSortedArray)
            {
                result.Append(String.Format("{0}; {1}\r\n", wc.Word, wc.Count));
            }
            return result.ToString();
        }

    }
}
