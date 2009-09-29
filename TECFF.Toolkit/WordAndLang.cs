namespace TECFF.Toolkit
{
    /// <summary>
    /// Utility class holding a pair of {word, language}.
    /// </summary>
    public class WordAndLang
    {
        private string word;
        private string lang;

        public WordAndLang(string word, string lang)
        {
            this.word = word;
            this.lang = lang;
        }

        public string Word
        {
            get
            {
                return this.word;
            }
        }

        public string Lang
        {
            get
            {
                return this.lang;
            }
        }
    }
}
