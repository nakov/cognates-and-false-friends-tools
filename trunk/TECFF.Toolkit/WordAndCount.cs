using System;

namespace TECFF.Toolkit
{
    /// <summary>
    /// Utility class holding a pair of {word, occurences}.
    /// </summary>
    public class WordAndCount : IComparable<WordAndCount>
    {
        private string word;
        private double count;

        public WordAndCount()
        {
        }

        public WordAndCount(string word, double count)
        {
            this.word = word;
            this.count = count;
        }

        public string Word
        {
            get { return word; }
            set { word = value; }
        }

        public double Count
        {
            get { return count; }
            set { count = value; }
        }

        public int CompareTo(WordAndCount otherWordAndCount)
        {
            int compareResult = otherWordAndCount.Count.CompareTo(this.Count);
            if (compareResult == 0)
            {
                compareResult = this.Word.CompareTo(otherWordAndCount.Word);
            }
            return compareResult;
        }
    }
}
