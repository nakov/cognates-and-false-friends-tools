using System.Collections.Generic;

namespace TECFF.Toolkit
{
    /// <summary>
    /// Utility class holding a pair of sentences (Bulgarian and Russian).
    /// </summary>
    public class SentencePair
    {
        private List<string> bgWords;
        private List<string> ruWords;

        public SentencePair(List<string> bgWords, List<string> ruWords)
        {
            this.bgWords = bgWords;
            this.ruWords = ruWords;
        }

        public List<string> BgWords
        {
            get { return this.bgWords; }
        }

        public List<string> RuWords
        {
            get { return this.ruWords; }
        }
    }
}
