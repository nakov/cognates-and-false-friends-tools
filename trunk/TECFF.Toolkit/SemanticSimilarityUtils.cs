using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;

namespace TECFF.Toolkit
{
    /// <summary>
    /// Implemements the algorithms SemSim and CrossSim for measuring monolingual and
    /// cross-lingual semantic similarity. Semantic similarity between words is measured
    /// by comparing the words local contexts obtained by the excerpts retuned by search
    /// engine queries (from the first 1000 results for given word and language).
    /// 
    /// SemSim supports Bulgarian, English and Russian.
    /// CrossSim supports the pair of langiuages {Bulgarian, Russian} only.
    /// 
    /// Remarks: lemmatization is currently not supported because the lemmatization
    /// dictionaries are not available as open source. For Bulgarian and Russian grammatical
    /// dictionaries please contact the Linguistic Modelling Department: http://lml.bas.bg/
    /// </summary>
    public class SemanticSimilarityUtils
    {
        public const string LANG_CODE_EN = "en";
        public const string LANG_CODE_BG = "bg";
        public const string LANG_CODE_RU = "ru";
        private const double TOTAL_NUMBER_OF_WORDS_ON_THE_WEB = 8000000000;
        private const string BULLET_OPERATOR = "\u2219";

        // Algorithm parameters (and their default values)
        private static SearchEngine searchEngine = SearchEngine.GOOGLE;
        private static VectorDiffAlgorithm vectorDiffAlgorithm = VectorDiffAlgorithm.COSINE;
        private static bool removeShortAndStopWords = true;
        private static int minWordsLength = 3;
        private static int contextSize = 3;
        private static bool useLemmatization = false;
        private static bool useHugeDictionary = false;
        private static bool useTFIDF = false;
        private static bool useReverseContext = false;
        private static bool useIndirectContext = false;
        private static int minWordOccurencesForReverseOrIndirectContext = 0;
        private static bool useQueryLemmatization = false;

        private static SearchEngine SearchEngine
        {
            get { return SemanticSimilarityUtils.searchEngine; }
            set { SemanticSimilarityUtils.searchEngine = value; }
        }

        public static VectorDiffAlgorithm VectorDiffAlgorithm
        {
            get { return SemanticSimilarityUtils.vectorDiffAlgorithm; }
            set { SemanticSimilarityUtils.vectorDiffAlgorithm = value; }
        }

        public static bool RemoveShortAndStopWords
        {
            get { return SemanticSimilarityUtils.removeShortAndStopWords; }
            set { SemanticSimilarityUtils.removeShortAndStopWords = value; }
        }

        public static int MinWordsLength
        {
            get { return SemanticSimilarityUtils.minWordsLength; }
            set { SemanticSimilarityUtils.minWordsLength = value; }
        }

        public static int ContextSize
        {
            get { return SemanticSimilarityUtils.contextSize; }
            set { SemanticSimilarityUtils.contextSize = value; }
        }

        public static bool UseLemmatization
        {
            get { return SemanticSimilarityUtils.useLemmatization; }
            set { SemanticSimilarityUtils.useLemmatization = value; }
        }

        public static bool UseTFIDF
        {
            get { return SemanticSimilarityUtils.useTFIDF; }
            set { SemanticSimilarityUtils.useTFIDF = value; }
        }

        public static bool UseReverseContext
        {
            get { return SemanticSimilarityUtils.useReverseContext; }
            set { SemanticSimilarityUtils.useReverseContext = value; }
        }

        public static bool UseIndirectContext
        {
            get { return SemanticSimilarityUtils.useIndirectContext; }
            set { SemanticSimilarityUtils.useIndirectContext = value; }
        }

        public static int MinWordOccurencesForReverseOrIndirectContext
        {
            get { return SemanticSimilarityUtils.minWordOccurencesForReverseOrIndirectContext; }
            set { SemanticSimilarityUtils.minWordOccurencesForReverseOrIndirectContext = value; }
        }

        static SemanticSimilarityUtils()
        {
            BgRuDictionary.UseHugeDictionary = SemanticSimilarityUtils.useHugeDictionary;
        }

        /// <summary>
        /// Calculates the distance between pair of words in the same language.
        /// </summary>
        public static double SemSim(
            string firstWord, string secondWord, string langCode)
        {
            WordsAndCounts firstWordContext;
            if (useReverseContext)
            {
                firstWordContext = SemanticSimilarityUtils.
                    RetrieveContextWordsWithReverseContext(firstWord, langCode);
            }
            else if (useQueryLemmatization)
            {
                firstWordContext = SemanticSimilarityUtils.
                    RetrieveContextWordsForAllWordForms(firstWord, langCode);
            }
            else if (useIndirectContext)
            {
                firstWordContext = SemanticSimilarityUtils.
                    RetrieveContextWordsWithIndirectContext(firstWord, langCode);
            }
            else
            {
                firstWordContext = SemanticSimilarityUtils.
                    RetrieveContextWords(firstWord, langCode);
            }

            WordsAndCounts secondWordContext;
            if (useReverseContext)
            {
                secondWordContext = SemanticSimilarityUtils.
                    RetrieveContextWordsWithReverseContext(secondWord, langCode);
            }
            else if (useQueryLemmatization)
            {
                secondWordContext = SemanticSimilarityUtils.
                    RetrieveContextWordsForAllWordForms(secondWord, langCode);
            }
            else if (useIndirectContext)
            {
                secondWordContext = SemanticSimilarityUtils.
                    RetrieveContextWordsWithIndirectContext(secondWord, langCode);
            }
            else
            {
                secondWordContext = SemanticSimilarityUtils.
                    RetrieveContextWords(secondWord, langCode);
            }

            if (useTFIDF)
            {
                ApplyTFIDFWeighting(firstWordContext, langCode);
                ApplyTFIDFWeighting(secondWordContext, langCode);
            }

            if (vectorDiffAlgorithm == VectorDiffAlgorithm.COSINE)
            {
                double distance = VectorUtils.CalcCosinusBetweenWordsCounts(
                    firstWordContext, secondWordContext);
                return distance;
            }
            else
            {
                throw new Exception("Algorithm not supported!");
            }
        }

        private static void ApplyTFIDFWeighting(WordsAndCounts wordContext, string langCode)
        {
            WordAndCount[] wordsAndCounts = wordContext.AsSortedArray;

            // Calculate the sum of all words occurences in the context
            double occurencesSum = 0;
            for (int i = 0; i < wordsAndCounts.Length; i++)
            {
                double occurences = wordsAndCounts[i].Count;
                occurencesSum = occurencesSum + occurences;
            }

            for (int i = 0; i < wordsAndCounts.Length; i++)
            {
                string word = wordsAndCounts[i].Word;

                // Calculate term frequency (TF)
                double termCount = wordContext[word];
                double termFrequency = termCount / occurencesSum;

                // Calculate inverse document frequency (IDF)
                double wordOccurencesOnTheWeb = GetWordOccurencesOnTheWeb(word, langCode);
                double inverseDocumentFrequency =
                    Math.Log(TOTAL_NUMBER_OF_WORDS_ON_THE_WEB / (1 + wordOccurencesOnTheWeb), 2);

                // frequency[word] = TFIDF[word] = TF[word] * IDF[word]
                double tfidf = termFrequency * inverseDocumentFrequency;
                wordContext[word] = tfidf;
            }
        }

        /// <summary>
        /// Calculates the distance (similarity) between given Bulgarian and Russian words.
        /// </summary>
        public static double CrossSim(string bgWord, string ruWord)
        {
            double[] bgVector = CalculateBgDictionaryContextVector(bgWord);
            double[] ruVector = CalculateRuDictionaryContextVector(ruWord);

            double distance;
            if (vectorDiffAlgorithm == VectorDiffAlgorithm.COSINE)
            {
                distance = VectorUtils.CalcCosinusBetweenVectors(bgVector, ruVector);
            }
            else if (vectorDiffAlgorithm == VectorDiffAlgorithm.DICE_COEFFICIENT)
            {
                distance = VectorUtils.CalcDiceCoeffBetweenVectors(bgVector, ruVector);
            }
            else
            {
                throw new Exception("Invalid vector diff algorithm!");
            }
            return distance;
        }

        public static WordsAndCounts RetrieveContextWords(string word, string langCode)
        {
            if (ContextWordsCache.IsInCache(word, langCode))
            {
                WordsAndCounts cachedContextWords =
                    ContextWordsCache.LoadFromCache(word, langCode);
                return cachedContextWords;
            }

            word = word.ToLowerInvariant();
            WordsAndCounts contextWords = new WordsAndCounts();
            List<string> titleSentences, textSentences;
            RetrieveWebContextSentences(word, langCode, out titleSentences, out textSentences);
            Dictionary<string, bool> allWordFormsDict = GetAllWordFormsDictionary(word, langCode);
            List<string> phrasesToKeep = new List<string>();
            phrasesToKeep.AddRange(allWordFormsDict.Keys);
            foreach (string titleSentence in titleSentences)
            {
                ExtractWordsAround(titleSentence, allWordFormsDict, langCode, contextWords, phrasesToKeep);
            }
            foreach (string textSentence in textSentences)
            {
                ExtractWordsAround(textSentence, allWordFormsDict, langCode, contextWords, phrasesToKeep);
            }

            if (useLemmatization)
            {
                WordsAndCounts contextWordsBasicForms =
                    LemmaDictionaryUtils.GetBasicForms(contextWords, langCode);
                ContextWordsCache.AddToCache(word, langCode, contextWordsBasicForms);
                return contextWordsBasicForms;
            }
            else
            {
                ContextWordsCache.AddToCache(word, langCode, contextWords);
                return contextWords;
            }
        }

        public static WordsAndCounts RetrieveContextWordsWithReverseContext(
            string word, string langCode)
        {
            if (ReversedContextCache.IsInCache(word, langCode))
            {
                WordsAndCounts cachedContextWordsAndCounts =
                    ReversedContextCache.LoadFromCache(word, langCode);
                return cachedContextWordsAndCounts;
            }

            WordsAndCounts contextWordsAndCounts =
                RetrieveContextWords(word, langCode);

            string[] contextWords = new string[contextWordsAndCounts.Words.Count];
            contextWordsAndCounts.Words.CopyTo(contextWords, 0);

            // Perform reverse context lookup and recalculate the occurences
            foreach (string contextWord in contextWords)
            {
                double forwardCount = contextWordsAndCounts[contextWord];

                if (forwardCount < SemanticSimilarityUtils.minWordOccurencesForReverseOrIndirectContext)
                {
                    contextWordsAndCounts.RemoveWord(contextWord);
                }
                else
                {
                    WordsAndCounts reverseContextWordsAndCounts =
                        RetrieveContextWords(contextWord, langCode);
                    double reverseCount = reverseContextWordsAndCounts[word];
                    double newOccurencesCount = Math.Min(forwardCount, reverseCount);
                    contextWordsAndCounts[contextWord] = newOccurencesCount;
                }
            }

            ReversedContextCache.AddToCache(word, langCode, contextWordsAndCounts);

            return contextWordsAndCounts;
        }

        public static WordsAndCounts RetrieveContextWordsWithIndirectContext(
            string word, string langCode)
        {
            if (IndirectContextCache.IsInCache(word, langCode))
            {
                WordsAndCounts cachedContextWordsAndCounts =
                    IndirectContextCache.LoadFromCache(word, langCode);
                return cachedContextWordsAndCounts;
            }

            WordsAndCounts contextWordsAndCounts = RetrieveContextWords(word, langCode);
            string[] contextWords = new string[contextWordsAndCounts.Words.Count];
            contextWordsAndCounts.Words.CopyTo(contextWords, 0);
            double[] contextWordsCounts = new double[contextWordsAndCounts.Counts.Count];
            contextWordsAndCounts.Counts.CopyTo(contextWordsCounts, 0);

            // Perform indirect context lookup and recalculate the occurences
            for (int i = 0; i < contextWords.Length; i++)
            {
                string contextWord = contextWords[i];
                double occurences = contextWordsCounts[i];
                if (occurences >= SemanticSimilarityUtils.minWordOccurencesForReverseOrIndirectContext)
                {
                    WordsAndCounts indirectContextWordsAndCounts =
                        RetrieveContextWords(contextWord, langCode);
                    contextWordsAndCounts.AddAll(indirectContextWordsAndCounts);
                }
            }

            IndirectContextCache.AddToCache(word, langCode, contextWordsAndCounts);

            return contextWordsAndCounts;
        }

        private static WordsAndCounts RetrieveContextWordsForAllWordForms(string word, string langCode)
        {
            if (AllWordFormsContextCache.IsInCache(word, langCode))
            {
                WordsAndCounts cachedContextWordsAndCounts =
                    AllWordFormsContextCache.LoadFromCache(word, langCode);
                return cachedContextWordsAndCounts;
            }

            // Retieve and merge the contexts of all word forms of the given word
            ICollection<string> allWordForms = LemmaDictionaryUtils.GetAllWordForms(word, langCode);
            WordsAndCounts contextWordsAndCounts = new WordsAndCounts();
            foreach (string wordForm in allWordForms)
            {
                WordsAndCounts wordFormContextWordsAndCounts =
                    RetrieveContextWords(wordForm, langCode);
                contextWordsAndCounts.AddAll(wordFormContextWordsAndCounts);
            }

            // Normalize the counts
            List<string> allContextWords = new List<string>();
            allContextWords.AddRange(contextWordsAndCounts.Words);
            foreach (string contextWord in allContextWords)
            {
                double count = contextWordsAndCounts[contextWord];
                contextWordsAndCounts[contextWord] = count / allWordForms.Count;
            }

            AllWordFormsContextCache.AddToCache(word, langCode, contextWordsAndCounts);

            return contextWordsAndCounts;
        }


        public static double GetWordOccurencesOnTheWeb(string word, string langCode)
        {
            if (searchEngine == SearchEngine.GOOGLE)
            {
                long wordOccurencesOnTheWeb = GoogleOccurencesUtils.GetOccurences(word, langCode);
                return wordOccurencesOnTheWeb;
            }
            else
            {
                throw new Exception(
                    "Can not find occurences through search engine " + searchEngine);
            }
        }

        private static void RetrieveWebContextSentences(string word, string langCode,
            out List<string> titleSentences, out List<string> textSentences)
        {
            if (searchEngine == SearchEngine.GOOGLE)
            {
                GoogleContextUtils.RetrieveGoogleContextSentences(
                    word, langCode, out titleSentences, out textSentences);
            }
            else
            {
                MSNContextUtils.RetrieveMSNContextSentences(
                    word, langCode, out titleSentences, out textSentences);
            }
        }

        private static Dictionary<string, bool> GetAllWordFormsDictionary(
            string word, string langCode)
        {
            Dictionary<string, bool> allWordFormsDict = new Dictionary<string, bool>();
            if (useLemmatization)
            {
                var allWordFormsCollection =
                    LemmaDictionaryUtils.GetAllWordForms(word, langCode);
                foreach (string wordForm in allWordFormsCollection)
                {
                    allWordFormsDict.Add(wordForm, true);
                }
            }
            else
            {
                allWordFormsDict.Add(word, true);
            }
            return allWordFormsDict;
        }

        private static void ExtractWordsAround(
            string text, Dictionary<string, bool> allWordForms, string langCode,
            WordsAndCounts wordsAndCounts, List<string> phrasesToKeep)
        {
            string textLowerCase = text.ToLowerInvariant();
            string textHtmlDecoded = HttpUtility.HtmlDecode(textLowerCase);
            string textWithoutPhrases = EncodePhrases(textHtmlDecoded, phrasesToKeep);
            string[] allWords;
            if ((langCode == LANG_CODE_BG) || (langCode == LANG_CODE_RU))
            {
                string nonCyrillicChars = "[^Р-пр-џ" + BULLET_OPERATOR + "]+";
                allWords = Regex.Split(textWithoutPhrases, nonCyrillicChars);
            }
            else
            {
                string nonLatinChars = "[^A-Za-z" + BULLET_OPERATOR + "]+";
                allWords = Regex.Split(textWithoutPhrases, nonLatinChars);
            }
            DecodePhrases(allWords);

            List<string> validWords;
            if (removeShortAndStopWords)
            {
                // TODO: this does not work correctly for short words like "эю"
                validWords = FilterWords(allWords, langCode, allWordForms);
            }
            else
            {
                validWords = new List<string>(allWords);
            }

            bool[] wordsToInclude = new bool[validWords.Count];
            for (int i = 0; i < validWords.Count; i++)
            {
                string currentWord = validWords[i];
                if (allWordForms.ContainsKey(currentWord))
                {
                    int start = Math.Max(0, i - contextSize);
                    int end = Math.Min(i + contextSize, validWords.Count - 1);
                    for (int contextIndex = start; contextIndex <= end; contextIndex++)
                    {
                        wordsToInclude[contextIndex] = true;
                    }
                }
            }
            for (int i = 0; i < validWords.Count; i++)
            {
                if (wordsToInclude[i])
                {
                    string wordToInclude = validWords[i];
                    wordsAndCounts.Add(wordToInclude, 1);
                }
            }
        }

        private static string EncodePhrases(string text, List<string> phrasesToKeep)
        {
            string resultText = text;
            foreach (string phrase in phrasesToKeep)
            {
                if (phrase.Contains(" "))
                {
                    // We have a phrase to encode into a single word
                    string encodedPhrase = phrase.Replace(" ", BULLET_OPERATOR);
                    resultText = resultText.Replace(phrase, encodedPhrase);
                }
            }
            return resultText;
        }

        private static void DecodePhrases(string[] words)
        {
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                if (word.Contains(BULLET_OPERATOR))
                {
                    // We have an encoded phrase
                    string decodedPhrase = word.Replace(BULLET_OPERATOR, " ");
                    words[i] = decodedPhrase;
                }
            }
        }

        private static List<string> FilterWords(string[] words, string langCode,
            Dictionary<string, bool> wordsToKeep)
        {
            List<string> validWords = new List<string>();
            foreach (string word in words)
            {
                bool validWord =
                    (word.Length >= minWordsLength) &&
                    (!StopWords.IsStopWord(word, langCode));
                bool toKeep = wordsToKeep.ContainsKey(word);
                if (validWord || toKeep)
                {
                    validWords.Add(word);
                }
            }
            return validWords;
        }

        private static double[] CalculateBgDictionaryContextVector(string bgWord)
        {
            // First check the vectors cache
            if (VectorsCache.IsInCache(bgWord, LANG_CODE_BG))
            {
                double[] bgVectorFromCache = VectorsCache.LoadFromCache(bgWord, LANG_CODE_BG);
                return bgVectorFromCache;
            }

            // Retrieve the word's local context
            WordsAndCounts bgWordContext;
            if (useIndirectContext)
            {
                bgWordContext = SemanticSimilarityUtils.
                    RetrieveContextWordsWithIndirectContext(bgWord, LANG_CODE_BG);
            }
            else if (useQueryLemmatization)
            {
                bgWordContext = SemanticSimilarityUtils.
                    RetrieveContextWordsForAllWordForms(bgWord, LANG_CODE_BG);
            }
            else
            {
                bgWordContext = SemanticSimilarityUtils.
                    RetrieveContextWords(bgWord, LANG_CODE_BG);
            }

            if (useTFIDF)
            {
                ApplyTFIDFWeighting(bgWordContext, LANG_CODE_BG);
            }

            // Analyse the word's local context and match the dictionary words in it
            string[] dictionaryBgWords = BgRuDictionary.DictionaryBgWords;
            double[] bgVector = new double[dictionaryBgWords.Length];
            for (int i = 0; i < dictionaryBgWords.Length; i++)
            {
                string bgDictWord = dictionaryBgWords[i];
                double bgDictWordCount = bgWordContext[bgDictWord];
                bgVector[i] = bgDictWordCount;
            }

            if (useReverseContext)
            {
                // Reverse match the context vector with the dictionary word's contexts
                for (int i = 0; i < dictionaryBgWords.Length; i++)
                {
                    double bgWordForwardCount = bgVector[i];
                    if (bgWordForwardCount >= SemanticSimilarityUtils.minWordOccurencesForReverseOrIndirectContext)
                    {
                        string bgDictWord = dictionaryBgWords[i];
                        WordsAndCounts bgDictWordReverseContext =
                            SemanticSimilarityUtils.RetrieveContextWords(bgDictWord, LANG_CODE_BG);
                        double bgWordReverseCount = GetWordCountInContext(bgWord, LANG_CODE_BG, bgDictWordReverseContext);
                        bgVector[i] = Math.Min(bgWordForwardCount, bgWordReverseCount);
                    }
                    else
                    {
                        bgVector[i] = 0;
                    }
                }
            }

            // Add the calculated context vector to the cache
            VectorsCache.AddToCache(bgWord, LANG_CODE_BG, bgVector);

            return bgVector;
        }

        private static double[] CalculateRuDictionaryContextVector(string ruWord)
        {
            // First check the vectors cache
            if (VectorsCache.IsInCache(ruWord, LANG_CODE_RU))
            {
                double[] ruVectorFromCache = VectorsCache.LoadFromCache(ruWord, LANG_CODE_RU);
                return ruVectorFromCache;
            }

            // Retrieve the word's local context
            WordsAndCounts ruWordContext;
            if (useIndirectContext)
            {
                ruWordContext = SemanticSimilarityUtils.
                    RetrieveContextWordsWithIndirectContext(ruWord, LANG_CODE_RU);
            }
            else if (useQueryLemmatization)
            {
                ruWordContext = SemanticSimilarityUtils.
                    RetrieveContextWordsForAllWordForms(ruWord, LANG_CODE_RU);
            }
            else
            {
                ruWordContext = SemanticSimilarityUtils.
                    RetrieveContextWords(ruWord, LANG_CODE_RU);
            }

            if (useTFIDF)
            {
                ApplyTFIDFWeighting(ruWordContext, LANG_CODE_RU);
            }

            // Analyse the word's local context and match the dictionary words in it
            string[] dictionaryBgWords = BgRuDictionary.DictionaryBgWords;
            double[] ruVector = new double[dictionaryBgWords.Length];
            for (int i = 0; i < dictionaryBgWords.Length; i++)
            {
                string bgDictWord = dictionaryBgWords[i];
                List<string> ruDictWords = BgRuDictionary.GetTranslations(bgDictWord);
                foreach (string ruDictWord in ruDictWords)
                {
                    double ruDictWordCount = ruWordContext[ruDictWord];
                    ruVector[i] += ruDictWordCount;
                }
            }

            if (useReverseContext)
            {
                // Reverse match the context vector with the dictionary word's contexts
                for (int i = 0; i < dictionaryBgWords.Length; i++)
                {
                    double ruWordForwardCount = ruVector[i];
                    if (ruWordForwardCount >= SemanticSimilarityUtils.minWordOccurencesForReverseOrIndirectContext)
                    {
                        string bgDictWord = dictionaryBgWords[i];
                        List<string> ruDictWords = BgRuDictionary.GetTranslations(bgDictWord);
                        double ruWordReverseTotalCount = 0;
                        foreach (string ruDictWord in ruDictWords)
                        {
                            WordsAndCounts ruDictWordReverseContext =
                                SemanticSimilarityUtils.RetrieveContextWords(ruDictWord, LANG_CODE_RU);
                            double ruWordReverseCount = GetWordCountInContext(ruWord, LANG_CODE_RU, ruDictWordReverseContext);
                            ruWordReverseTotalCount += ruWordReverseCount;
                        }
                        ruVector[i] = Math.Min(ruWordForwardCount, ruWordReverseTotalCount);
                    }
                    else
                    {
                        ruVector[i] = 0;
                    }
                }
            }

            // Add the calculated context vector to the cache
            VectorsCache.AddToCache(ruWord, LANG_CODE_RU, ruVector);

            return ruVector;
        }

        private static double GetWordCountInContext(string word, String langCode, WordsAndCounts context)
        {
            if (useLemmatization)
            {
                double wordCount = 0;
                var allWordFormsCollection =
                    LemmaDictionaryUtils.GetAllWordForms(word, langCode);
                foreach (string wordForm in allWordFormsCollection)
                {
                    wordCount += context[wordForm];
                }
                return wordCount;
            }
            else
            {
                double wordCount = context[word];
                return wordCount;
            }
        }
    }

    enum SearchEngine
    {
        GOOGLE,
        MSN
    }
}
