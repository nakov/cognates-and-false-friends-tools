using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TECFF.Toolkit
{
    /// <summary>
    /// Provides access to the bilingual Bulgarian-to-Russian glossary used as cross-language bridge.
    /// </summary>
    public class BgRuDictionary
    {
        private static bool useHugeDictionary = false;

        private const string DICTIONARY_WORDS_FILE_NORMAL = @"..\..\..\Resources\BG-RU-Dictionary-Normal-4562.csv";
        private const string DICTIONARY_WORDS_FILE_HUGE = @"..\..\..\Resources\BG-RU-Dictionary-Huge-59582-Words.csv";

        private static readonly Encoding FILE_ENCODING = Encoding.GetEncoding("windows-1251");
        private static readonly List<string> EMPTY_LIST = new List<string>();
        private static List<WordsPair> dictionaryWords = null;
        private static Dictionary<string, List<string>> dictionaryBgRu = null;
        private static string[] dictBgWords = null;

        private BgRuDictionary()
        {
        }

        public static bool UseHugeDictionary
        {
            get { return BgRuDictionary.useHugeDictionary; }
            set { BgRuDictionary.useHugeDictionary = value; }
        }

        public static List<WordsPair> DictionaryEntries
        {
            get
            {
                if (dictionaryWords != null)
                {
                    return dictionaryWords;
                }

                dictionaryWords = new List<WordsPair>();
                if (useHugeDictionary)
                {
                    ReadDictionaryFile(DICTIONARY_WORDS_FILE_HUGE);
                }
                else
                {
                    ReadDictionaryFile(DICTIONARY_WORDS_FILE_NORMAL);
                }
                return dictionaryWords;
            }
        }

        private static void ReadDictionaryFile(string dictFileName)
        {
            StreamReader reader = new StreamReader(dictFileName, FILE_ENCODING);
            using (reader)
            {
                // Skip the header line
                reader.ReadLine();

                // Process all lines to the end of the file
                while (true)
                {
                    string line = reader.ReadLine();
                    if ((line == null) || (line == ""))
                    {
                        // End of file reached
                        break;
                    }
                    string[] tokens = line.Split(';');
                    //string bgWord = String.Intern(tokens[0].Trim());
                    //string ruWord = String.Intern(tokens[1].Trim());
                    string bgWord = tokens[0].Trim();
                    string ruWord = tokens[1].Trim();
                    WordsPair wordsPair = new WordsPair(bgWord, ruWord);
                    dictionaryWords.Add(wordsPair);
                }
            }
        }

        public static string[] DictionaryBgWords
        {
            get
            {
                if (dictBgWords == null)
                {
                    dictionaryBgRu = GetDictionaryBgRu();
                    dictBgWords = new string[dictionaryBgRu.Keys.Count];
                    int i = 0;
                    foreach (string word in dictionaryBgRu.Keys)
                    {
                        dictBgWords[i] = word;
                        i++;
                    }
                }
                return dictBgWords;
            }
        }

        public static List<string> GetTranslations(string bgWord)
        {
            dictionaryBgRu = GetDictionaryBgRu();
            if (dictionaryBgRu.ContainsKey(bgWord))
            {
                List<string> ruTranslations = dictionaryBgRu[bgWord];
                return ruTranslations;
            }
            else
            {
                return EMPTY_LIST;
            }
        }

        private static Dictionary<string, List<string>> GetDictionaryBgRu()
        {
            if (dictionaryBgRu == null)
            {
                dictionaryBgRu = new Dictionary<string, List<string>>();
                List<WordsPair> dict = BgRuDictionary.DictionaryEntries;
                foreach (WordsPair dictEntry in dict)
                {
                    string word = dictEntry.BgWord;
                    string translation = dictEntry.RuWord;
                    if (dictionaryBgRu.ContainsKey(word))
                    {
                        List<string> translations = dictionaryBgRu[word];
                        translations.Add(translation);
                    }
                    else
                    {
                        List<string> translations = new List<string>();
                        translations.Add(translation);
                        dictionaryBgRu[word] = translations;
                    }
                }
            }

            return dictionaryBgRu;
        }
    }

}
