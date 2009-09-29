namespace TECFF.Toolkit
{
    /// <summary>
    /// Utility class - provides a data structure holding a pair of words (Bulgarian and Russian).
    /// </summary>
    public struct WordsPair
    {
        private string bgWord;
        private string ruWord;

        public WordsPair(string bgWord, string ruWord)
        {
            this.bgWord = bgWord;
            this.ruWord = ruWord;
        }

        public string BgWord
        {
            get
            {
                return this.bgWord;
            }
        }

        public string RuWord
        {
            get
            {
                return this.ruWord;
            }
        }
    }
}
