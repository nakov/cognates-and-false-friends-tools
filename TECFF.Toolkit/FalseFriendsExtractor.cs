using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TECFF.Toolkit
{
    /// <summary>
    /// Implementation of the FFExtract algorithm for extracting false friends from given
    /// parallel corpus. It combines statistical co-occurences formulas and the CrossSim
    /// algorithm.
    /// 
    /// Remarks: lemmatization is currently not supported because the lemmatization
    /// dictionaries are not available as open source. For Bulgarian and Russian grammatical
    /// dictionaries please contact the Linguistic Modelling Department: http://lml.bas.bg/
    /// </summary>
    public class FalseFriendsExtractor
    {
        private const string LANG_CODE_BG = "bg";
        private const string LANG_CODE_RU = "ru";
        private const float MIN_WORD_SIMILARITY = 0.90f;
        private bool USE_LEMMATIZATION = false;
        //private string FORMULA = "F1";
        private string FORMULA = "F2";
        //private string FORMULA = "F6";
        private readonly Encoding FILE_ENCODING = Encoding.GetEncoding("windows-1251");

        private SortedDictionary<string, bool> allBgWords = new SortedDictionary<string, bool>();
        private SortedDictionary<string, bool> allRuWords = new SortedDictionary<string, bool>();
        private List<SentencePair> sentencePairs = new List<SentencePair>();
        private Dictionary<string, string> friendsJudge = new Dictionary<string, string>();

        private Dictionary<string, float> bgWordsCounts = new Dictionary<string, float>();
        private Dictionary<string, float> ruWordsCounts = new Dictionary<string, float>();
        private Dictionary<string, float> correspondingCounts = new Dictionary<string, float>();

        public void LoadSentencesAndExtractWords(string corpusFileName)
        {
            StreamReader reader = new StreamReader(corpusFileName, FILE_ENCODING);
            using (reader)
            {
                while (true)
                {
                    string ruSentence = reader.ReadLine();
                    if (ruSentence == null)
                    {
                        break;
                    }
                    List<string> ruSentenceWords =
                        WordExtractor.ExtractAndNormalizeWords(ruSentence);
                    foreach (string ruWord in ruSentenceWords)
                    {
                        if (!allRuWords.ContainsKey(ruWord))
                        {
                            allRuWords.Add(ruWord, true);
                        }
                    }

                    string bgSentence = reader.ReadLine();
                    if (bgSentence == null)
                    {
                        Console.Error.WriteLine("Error: Sentences not equal!");
                    }
                    List<string> bgSentenceWords =
                        WordExtractor.ExtractAndNormalizeWords(bgSentence);
                    foreach (string bgWord in bgSentenceWords)
                    {
                        if (!allBgWords.ContainsKey(bgWord))
                        {
                            allBgWords.Add(bgWord, true);
                        }
                    }

                    SentencePair sentencePair =
                        new SentencePair(bgSentenceWords, ruSentenceWords);
                    this.sentencePairs.Add(sentencePair);
                }
            }
        }

        public void LoadFriendsJudgeFile(string friendsJudgeFileName)
        {
            StreamReader reader = new StreamReader(friendsJudgeFileName, FILE_ENCODING);
            using (reader)
            {
                // Skip header line
                reader.ReadLine();

                // Load false friends / cognates judge file
                while (true)
                {
                    String line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    string[] tokens = line.Split(';');
                    string bgWord = tokens[0];
                    string ruWord = tokens[1];
                    string key = bgWord + ":" + ruWord;
                    string judge = tokens[2];
                    this.friendsJudge.Add(key, judge);
                }
            }
        }

        public void CalculateStatistics()
        {
            foreach (SentencePair sentencePair in this.sentencePairs)
            {
                List<string> bgLemmas = ExtractUniqueLemmas(sentencePair.BgWords, LANG_CODE_BG);
                foreach (string bgLemma in bgLemmas)
                {
                    CountBgLemma(bgLemma);
                }

                List<string> ruLemmas = ExtractUniqueLemmas(sentencePair.RuWords, LANG_CODE_RU);
                foreach (string ruLemma in ruLemmas)
                {
                    CountRuLemma(ruLemma);
                }

                foreach (string bgLemma in bgLemmas)
                {
                    foreach (string ruLemma in ruLemmas)
                    {
                        CountCorrespondingLemmas(bgLemma, ruLemma);
                    }
                }
            }
        }

        private List<string> ExtractUniqueLemmas(List<string> words, string langCode)
        {
            List<string> allLemmas = new List<string>();
            foreach (string word in words)
            {
                List<string> lemmas = GetLemmas(word, langCode);
                allLemmas.AddRange(lemmas);
            }
            allLemmas = RemoveDuplicates(allLemmas);
            return allLemmas;
        }

        private List<string> RemoveDuplicates(List<string> words)
        {
            SortedDictionary<string, bool> wordsUnique = new SortedDictionary<string, bool>();
            foreach (string word in words)
            {
                if (!wordsUnique.ContainsKey(word))
                {
                    wordsUnique.Add(word, true);
                }
            }
            List<string> uniqueWordsList = new List<string>();
            uniqueWordsList.AddRange(wordsUnique.Keys);
            return uniqueWordsList;
        }

        private void CountBgLemma(string bgLemma)
        {
            float oldValue = 0.0f;
            if (this.bgWordsCounts.ContainsKey(bgLemma))
            {
                oldValue = bgWordsCounts[bgLemma];
            }
            float newValue = oldValue + 1;
            bgWordsCounts[bgLemma] = newValue;
        }

        private void CountRuLemma(string ruLemma)
        {
            float oldValue = 0.0f;
            if (this.ruWordsCounts.ContainsKey(ruLemma))
            {
                oldValue = ruWordsCounts[ruLemma];
            }
            float newValue = oldValue + 1;
            ruWordsCounts[ruLemma] = newValue;
        }

        private void CountCorrespondingLemmas(string bgLemma, string ruLemma)
        {
            string key = bgLemma + ":" + ruLemma;
            float oldValue = 0.0f;
            if (this.correspondingCounts.ContainsKey(key))
            {
                oldValue = correspondingCounts[key];
            }
            float newValue = oldValue + 1;
            correspondingCounts[key] = newValue;
        }

        private List<string> GetLemmas(string word, string langCode)
        {
            List<string> lemmas;
            if (USE_LEMMATIZATION)
            {
                lemmas = LemmaDictionaryUtils.GetBasicForms(word, langCode);
            }
            else
            {
                lemmas = new List<string>();
                lemmas.Add(word);
            }
            return lemmas;
        }

        public void ExtractCognatesAndFalseFriends(string outputFileName)
        {
            StreamWriter writer = new StreamWriter(outputFileName, false, FILE_ENCODING);
            using (writer)
            {
                writer.WriteLine("BG Word;RU Word;Friends?;MMED Similarity;BG Count;RU Count;Corresponding Count;Friendness;Semantic Similarity");
                foreach (string bgWord in this.allBgWords.Keys)
                {
                    Console.WriteLine("Extracting words statistics: " + bgWord);
                    foreach (string ruWord in this.allRuWords.Keys)
                    {
                        double wordsSimilarity = MMEDR.CalculateSimilarity(bgWord, ruWord);
                        if (wordsSimilarity >= MIN_WORD_SIMILARITY)
                        {
                            string friendsJudgeAnswer = GetFriendsJudgeAnswer(bgWord, ruWord);
                            if (friendsJudgeAnswer != null)
                            {
                                float bgCount = CalculateBgCount(bgWord);
                                float ruCount = CalculateRuCount(ruWord);
                                float correspondingCount = CalculateCorrespondingCount(bgWord, ruWord);
                                float friendness = CalculateFriendness(bgCount, ruCount, correspondingCount);
                                double semanticSimilarity = SemanticSimilarityUtils.CrossSim(bgWord, ruWord);
                                writer.WriteLine("{0};{1};{2};{3};{4};{5};{6};{7};{8}",
                                    bgWord, ruWord, friendsJudgeAnswer, wordsSimilarity, bgCount, ruCount,
                                    correspondingCount, friendness, semanticSimilarity);
                                writer.Flush();
                            }
                        }
                    }
                }
            }
        }

        private string GetFriendsJudgeAnswer(string bgWord, string ruWord)
        {
            string key = bgWord + ":" + ruWord;
            if (this.friendsJudge.ContainsKey(key))
            {
                string judge = this.friendsJudge[key];
                return judge;
            }
            else
            {
                return null;
            }
        }

        private float CalculateBgCount(string bgWord)
        {
            List<string> lemmas = GetLemmas(bgWord, LANG_CODE_BG);
            float sum = 0.0f;
            foreach (string lemma in lemmas)
            {
                sum += this.bgWordsCounts[lemma];
            }
            float average = sum / lemmas.Count;
            return average;
        }

        private float CalculateRuCount(string ruWord)
        {
            List<string> lemmas = GetLemmas(ruWord, LANG_CODE_RU);
            float sum = 0.0f;
            foreach (string lemma in lemmas)
            {
                sum += this.ruWordsCounts[lemma];
            }
            float average = sum / lemmas.Count;
            return average;
        }

        private float CalculateCorrespondingCount(string bgWord, string ruWord)
        {
            List<string> bgLemmas = GetLemmas(bgWord, LANG_CODE_BG);
            List<string> ruLemmas = GetLemmas(ruWord, LANG_CODE_RU);
            float sum = 0.0f;
            foreach (string bgLemma in bgLemmas)
            {
                foreach (string ruLemma in ruLemmas)
                {
                    string key = bgLemma + ":" + ruLemma;
                    if (this.correspondingCounts.ContainsKey(key))
                    {
                        sum += this.correspondingCounts[key];
                    }
                }
            }
            float average = sum / (bgLemmas.Count * ruLemmas.Count);
            return average;
        }

        private float CalculateFriendness(float bgCount, float ruCount, float correspondingCount)
        {
            if (FORMULA == "F6")
            {
                return CalculateF6(bgCount, ruCount, correspondingCount);
            }
            else if (FORMULA == "F1")
            {
                return CalculateF1(bgCount, ruCount, correspondingCount);
            }
            else if (FORMULA == "F2")
            {
                return CalculateF2(bgCount, ruCount, correspondingCount);
            }
            else
            {
                throw new Exception("Formula " + FORMULA + " is unavailable!");
            }
        }

        private float CalculateF6(float bgCount, float ruCount, float correspondingCount)
        {
            // Preslav and Veno formula (refered as F6 in their paper)
            // (corr+1) / max( (bg+1)/(ru+1), (ru+1)/(bg+1) )
            float friendness = (correspondingCount + 1) /
                Math.Max(
                    (1.0f + bgCount) / (1.0f + ruCount),
                    (1.0f + ruCount) / (1.0f + bgCount));
            return friendness;
        }

        private float CalculateF1(float bgCount, float ruCount, float correspondingCount)
        {
            // (corr+1)*(corr+1) / ((bg+1)*(ru+1))
            float friendness =
                (correspondingCount + 1) * (correspondingCount + 1) /
                ((bgCount + 1) * (ruCount + 1));
            return friendness;
        }

        private float CalculateF2(float bgCount, float ruCount, float correspondingCount)
        {
            // (corr+1)*(corr+1) / ((bg-corr+1)*(ru-corr+1))
            float friendness =
                (correspondingCount + 1) * (correspondingCount + 1) /
                ((bgCount - correspondingCount + 1) * (ruCount - correspondingCount + 1));
            return friendness;
        }
    }
}
