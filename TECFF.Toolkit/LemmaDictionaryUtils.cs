using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TECFF.Toolkit
{
    public class LemmaDictionaryUtils
    {
        private const string LEMMA_DICT_FILE_PREFIX = @"..\..\..\Resources\Lemma-Dictionary-";
        private const string LEMMA_DICT_FILE_SUFFIX = @".csv";
        private static readonly Encoding FILE_ENCODING = Encoding.GetEncoding("windows-1251");

        private static Dictionary<string, Dictionary<string, List<string>>> word2basicFormHT =
            new Dictionary<string, Dictionary<string, List<string>>>();

        private static Dictionary<string, Dictionary<string, List<string>>> basicForm2wordFormsHT =
            new Dictionary<string, Dictionary<string, List<string>>>();

        private LemmaDictionaryUtils()
        {
        }

        private static void ReadLemmaDictionaryFile(string langCode)
        {
            string fileName = LEMMA_DICT_FILE_PREFIX + langCode + LEMMA_DICT_FILE_SUFFIX;
            Console.WriteLine("Loading lemma dictionary {0}", fileName);

            Dictionary<string, List<string>> word2basicForm =
                new Dictionary<string, List<string>>();
            word2basicFormHT[langCode] = word2basicForm;

            Dictionary<string, List<string>> basicForm2wordForms =
                new Dictionary<string, List<string>>();
            basicForm2wordFormsHT[langCode] = basicForm2wordForms;

            StreamReader reader = new StreamReader(fileName, FILE_ENCODING);
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
                    string wordForm = tokens[0].Trim();
                    string basicForm = tokens[1].Trim();
                    if (!word2basicForm.ContainsKey(wordForm))
                    {
                        List<string> basicForms = new List<string>();
                        basicForms.Add(basicForm);
                        word2basicForm.Add(wordForm, basicForms);
                    }
                    else
                    {
                        List<string> basicForms = word2basicForm[wordForm];
                        basicForms.Add(basicForm);
                    }
                    if (!basicForm2wordForms.ContainsKey(basicForm))
                    {
                        List<string> wordForms = new List<string>();
                        wordForms.Add(wordForm);
                        basicForm2wordForms.Add(basicForm, wordForms);
                    }
                    else
                    {
                        List<string> wordForms = basicForm2wordForms[basicForm];
                        wordForms.Add(wordForm);
                    }
                }
            }
        }

        public static List<string> GetBasicForms(string word, string langCode)
        {
            if (!word2basicFormHT.ContainsKey(langCode))
            {
                ReadLemmaDictionaryFile(langCode);
            }
            Dictionary<string, List<string>> word2basicForm = word2basicFormHT[langCode];

            if (word2basicForm.ContainsKey(word))
            {
                List<string> basicWords = word2basicForm[word];
                return basicWords;
            }
            else
            {
                List<string> basicWords = new List<string>();
                basicWords.Add(word);
                return basicWords;
            }
        }

        public static WordsAndCounts GetBasicForms(WordsAndCounts words, string langCode)
        {
            WordsAndCounts basicForms = new WordsAndCounts();
            WordAndCount[] wordsAndCounts = words.AsSortedArray;
            foreach (WordAndCount word in wordsAndCounts)
            {
                List<string> basicFormWords =
                    LemmaDictionaryUtils.GetBasicForms(word.Word, langCode);
                foreach (string basicWord in basicFormWords)
                {
                    //double count = (double)word.Count / basicFormWords.Count;
                    double count = (double)word.Count;
                    basicForms.Add(basicWord, count);
                }
            }
            return basicForms;
        }

        public static List<string> GetNonBasicForms(string wordInBasicForm, string langCode)
        {
            if (!basicForm2wordFormsHT.ContainsKey(langCode))
            {
                ReadLemmaDictionaryFile(langCode);
            }
            Dictionary<string, List<string>> basicForm2otherForms =
                basicForm2wordFormsHT[langCode];

            if (basicForm2otherForms.ContainsKey(wordInBasicForm))
            {
                List<string> wordForms = basicForm2otherForms[wordInBasicForm];
                return wordForms;
            }
            else
            {
                List<string> wordForms = new List<string>();
                wordForms.Add(wordInBasicForm);
                return wordForms;
            }
        }

        public static ICollection<string> GetAllWordForms(string word, string langCode)
        {
            Dictionary<string, bool> allWordForms = new Dictionary<string, bool>();
            List<string> basicForms = LemmaDictionaryUtils.GetBasicForms(word, langCode);
            foreach (string basicForm in basicForms)
            {
                List<string> wordForms =
                    LemmaDictionaryUtils.GetNonBasicForms(basicForm, langCode);
                foreach (string wordForm in wordForms)
                {
                    allWordForms[wordForm] = true;
                }
            }
            ICollection<string> allWordFormsList = allWordForms.Keys;
            return allWordFormsList;
        }

        public static ICollection<string> GetAllBasicForms(string langCode)
        {
            if (!basicForm2wordFormsHT.ContainsKey(langCode))
            {
                ReadLemmaDictionaryFile(langCode);
            }
            Dictionary<string, List<string>> basicForm2otherForms =
                basicForm2wordFormsHT[langCode];
            ICollection<string> allbasicForms = basicForm2otherForms.Keys;
            return allbasicForms;
        }

    }
}
