using System;
using System.Collections.Generic;

namespace TECFF.Toolkit
{
    /// <summary>
    /// Implements the MMEDR algorithm (modified weighted min-edit-distance-ratio)
    /// for measuring orthographic similarity between Bulgarian and Russian words.
    /// It measures traditional levenshtein distance with weights for the letter
    /// substitutions and some hard-coded n-grams transformation rules.
    /// </summary>
    public class MMEDR
    {
        private const string LANG_CODE_BG = "bg";
        private const string LANG_CODE_RU = "ru";

        private const bool USE_LEMMATIZATION = false;
        private const int MIN_WORD_LENGTH_FOR_LEMMATIZATION = 1;

        private static double[] pricesInsert = new double[(int)'¸' + 1];
        private static double[] pricesDelete = new double[(int)'¸' + 1];
        private static double[,] pricesReplace = new double[(int)'¸' + 1, (int)'¸' + 1];

        private string word1;
        private string word2;
        private double[,] dist;

        const double NOT_CALCULATED = -1.00;

        static MMEDR()
        {
            // Initialize INSERT prices
            for (int i = (int)'à'; i <= (int)'¸'; i++)
            {
                MMEDR.pricesInsert[i] = 1;
            }

            // Initialize DELETE prices
            for (int i = (int)'à'; i <= (int)'¸'; i++)
            {
                MMEDR.pricesDelete[i] = 1;
            }

            // Initialize REPLACE prices
            for (int i = (int)'à'; i <= (int)'¸'; i++)
            {
                for (int j = (int)'à'; j <= (int)'¸'; j++)
                {
                    MMEDR.pricesReplace[i, j] = 1;
                }
            }

            // This is the cyrillic alphabet
            // àáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ¸

            // Now modify the weight for some couples of letters
            SetReplacePrice('à', 'å', 0.7f);
            SetReplacePrice('à', 'è', 0.8f);
            SetReplacePrice('à', 'î', 0.7f);
            SetReplacePrice('à', 'ó', 0.6f);
            SetReplacePrice('à', 'ú', 0.5f);
            SetReplacePrice('à', '¸', 0.8f);
            SetReplacePrice('à', 'þ', 0.8f);
            SetReplacePrice('à', 'ÿ', 0.5f);

            SetReplacePrice('á', 'â', 0.8f);
            SetReplacePrice('á', 'ï', 0.6f);

            SetReplacePrice('â', 'ô', 0.6f);

            SetReplacePrice('ã', 'ê', 0.6f);
            SetReplacePrice('ã', 'õ', 0.5f);

            SetReplacePrice('ä', 'ò', 0.6f);

            SetReplacePrice('å', 'è', 0.6f);
            SetReplacePrice('å', 'î', 0.7f);
            SetReplacePrice('å', 'ó', 0.8f);
            SetReplacePrice('å', 'ú', 0.5f);
            SetReplacePrice('å', '¸', 0.6f);
            SetReplacePrice('å', 'þ', 0.8f);
            SetReplacePrice('å', 'ÿ', 0.5f);

            SetReplacePrice('æ', 'ç', 0.8f);
            SetReplacePrice('æ', 'ø', 0.6f);

            SetReplacePrice('ç', 'ñ', 0.5f);

            SetReplacePrice('è', 'é', 0.6f);
            SetReplacePrice('è', 'î', 0.8f);
            SetReplacePrice('è', 'ó', 0.8f);
            SetReplacePrice('è', 'ú', 0.8f);
            SetReplacePrice('è', '¸', 0.7f);
            SetReplacePrice('è', 'þ', 0.7f);
            SetReplacePrice('è', 'ÿ', 0.7f);

            SetReplacePrice('é', '¸', 0.7f);
            SetReplacePrice('é', 'þ', 0.7f);
            SetReplacePrice('é', 'ÿ', 0.7f);

            SetReplacePrice('ê', 'ò', 0.8f);
            SetReplacePrice('ê', 'õ', 0.6f);

            SetReplacePrice('ë', 'ó', 0.6f);

            SetReplacePrice('ì', 'í', 0.7f);

            SetReplacePrice('î', 'ó', 0.6f);
            SetReplacePrice('î', 'ú', 0.8f);
            SetReplacePrice('î', '¸', 0.6f);
            SetReplacePrice('î', 'þ', 0.7f);
            SetReplacePrice('î', 'ÿ', 0.8f);

            SetReplacePrice('ï', 'ô', 0.8f);
            SetReplacePrice('ï', 'õ', 0.9f);

            SetReplacePrice('ñ', 'ö', 0.6f);
            SetReplacePrice('ñ', 'ø', 0.9f);

            SetReplacePrice('ò', 'ô', 0.8f);
            SetReplacePrice('ò', 'õ', 0.9f);
            SetReplacePrice('ò', 'ö', 0.9f);

            SetReplacePrice('ó', 'ú', 0.5f);
            SetReplacePrice('ó', '¸', 0.8f);
            SetReplacePrice('ó', 'þ', 0.6f);
            SetReplacePrice('ó', 'ÿ', 0.8f);

            SetReplacePrice('ô', 'õ', 0.8f);

            SetReplacePrice('õ', 'ø', 0.9f);

            SetReplacePrice('ö', '÷', 0.8f);

            SetReplacePrice('÷', 'ø', 0.9f);

            SetReplacePrice('ø', 'ù', 0.8f);

            SetReplacePrice('ú', '¸', 0.8f);
            SetReplacePrice('ú', 'þ', 0.8f);
            SetReplacePrice('ú', 'ÿ', 0.8f);

            SetReplacePrice('¸', 'þ', 0.6f);
            SetReplacePrice('¸', 'ÿ', 0.7f);

            SetReplacePrice('þ', 'ÿ', 0.8f);
        }

        private MMEDR(string word1, string word2)
        {
            this.word1 = word1;
            this.word2 = word2;
            dist = new double[word1.Length + 1, word2.Length + 1];
            for (int i = 0; i <= word1.Length; i++)
            {
                for (int j = 0; j <= word2.Length; j++)
                {
                    dist[i, j] = NOT_CALCULATED;
                }
            }
            dist[0, 0] = 0;
        }

        private static double GetInsertPrice(char letter)
        {
            double price = MMEDR.pricesInsert[(int)letter];
            return price;
        }

        private static double GetDeletePrice(char letter)
        {
            double price = MMEDR.pricesDelete[(int)letter];
            return price;
        }

        private static double GetReplacePrice(char c1, char c2)
        {
            double price = MMEDR.pricesReplace[(int)c1, (int)c2];
            return price;
        }

        private static void SetReplacePrice(char c1, char c2, double price)
        {
            MMEDR.pricesReplace[(int)c1, (int)c2] = price;
            MMEDR.pricesReplace[(int)c2, (int)c1] = price;
        }

        private double CalcDistance(int len1, int len2)
        {
            if (dist[len1, len2] != NOT_CALCULATED)
            {
                // Value already calculated
                return dist[len1, len2];
            }

            if (len1 == 0)
            {
                // The first word is empty, the second is not
                CalcDistance(len1, len2 - 1);
                dist[len1, len2] = GetInsertPrice(word2[len2 - 1]) + dist[len1, len2 - 1];
                return dist[len1, len2];
            }

            if (len2 == 0)
            {
                // The second word is empty, the first is not
                CalcDistance(len1 - 1, len2);
                dist[len1, len2] = GetDeletePrice(word1[len1 - 1]) + dist[len1 - 1, len2];
                return dist[len1, len2];
            }

            CalcDistance(len1 - 1, len2);
            CalcDistance(len1, len2 - 1);
            CalcDistance(len1 - 1, len2 - 1);

            if (word1[len1 - 1] == word2[len2 - 1])
            {
                // We have equal last letter
                dist[len1, len2] = dist[len1 - 1, len2 - 1];
                return dist[len1, len2];
            }

            double replacePrice =
                GetReplacePrice(word1[len1 - 1], word2[len2 - 1]) + dist[len1 - 1, len2 - 1];
            double insertPrice =
                GetInsertPrice(word2[len2 - 1]) + dist[len1, len2 - 1];
            double deletePrice =
                GetDeletePrice(word1[len1 - 1]) + dist[len1 - 1, len2];
            dist[len1, len2] = Math.Min(Math.Min(replacePrice, insertPrice), deletePrice);
            return dist[len1, len2];
        }

        private static void ReplaceAll(ref string word, string oldSubstr, string newSubstr)
        {
            word = word.Replace(oldSubstr, newSubstr);
        }

        private static void ReplaceAtEnd(ref string word, string oldEnding, string newEnding)
        {
            if (word.EndsWith(oldEnding))
            {
                word = word.Substring(0, word.Length - oldEnding.Length) + newEnding;
            }
        }

        private static string TransformRussianInflextionToBulgarian(string ruWord)
        {
            ReplaceAtEnd(ref ruWord, "ííûé", "íåí");
            ReplaceAtEnd(ref ruWord, "íûé", "åí");
            ReplaceAtEnd(ref ruWord, "ííèé", "íåí");
            ReplaceAtEnd(ref ruWord, "íèé", "åí");
            ReplaceAtEnd(ref ruWord, "èé", "è");
            ReplaceAtEnd(ref ruWord, "ûé", "è");
            ReplaceAtEnd(ref ruWord, "üñÿ", "ü");
            ReplaceAtEnd(ref ruWord, "îâàòü", "àì");
            ReplaceAtEnd(ref ruWord, "èòü", "ÿ");
            ReplaceAtEnd(ref ruWord, "ÿòü", "ÿ");
            ReplaceAtEnd(ref ruWord, "àòü", "àì");
            ReplaceAtEnd(ref ruWord, "óòü", "à");
            ReplaceAtEnd(ref ruWord, "åòü", "åÿ");

            return ruWord;
        }

        private static double CalcMMEDR(string bgWord, string ruWord)
        {
            // Transcribe the Bulgarian word
            ReplaceAll(ref bgWord, "ù", "øò");
            ReplaceAll(ref bgWord, "üî", "¸");
            ReplaceAll(ref bgWord, "éî", "¸");

            // Transcribe the Russian word
            ReplaceAll(ref ruWord, "ý", "å");
            ReplaceAll(ref ruWord, "ù", "ñ÷");
            ReplaceAll(ref ruWord, "ú", "");
            ReplaceAll(ref ruWord, "û", "è");
            ReplaceAll(ref ruWord, "ü", "");

            // Remove double consonants in the Bulgarian word
            ReplaceAll(ref bgWord, "çñ", "ñ");

            // Remove double consonants in the Russian word
            ReplaceAll(ref ruWord, "áá", "á");
            ReplaceAll(ref ruWord, "ææ", "æ");
            ReplaceAll(ref ruWord, "êê", "ê");
            ReplaceAll(ref ruWord, "ëë", "ë");
            ReplaceAll(ref ruWord, "ìì", "ì");
            ReplaceAll(ref ruWord, "ïï", "ï");
            ReplaceAll(ref ruWord, "ññ", "ñ");
            ReplaceAll(ref ruWord, "ôô", "ô");

            // Calculate weighted Levenstain distance (MMEDR)
            MMEDR distCalc = new MMEDR(bgWord, ruWord);
            double distance = distCalc.CalcDistance(bgWord.Length, ruWord.Length);
            double maxLen = Math.Max(bgWord.Length, ruWord.Length);
            double similarity = (maxLen - distance) / maxLen;
            return similarity;
        }

        public static double CalculateSimilarity(string bgWord, string ruWord)
        {
            // Prepare all forms of the Bulgarian word
            bgWord = bgWord.ToLowerInvariant();
            HashSet<string> bgWordForms = new HashSet<string>();
            bgWordForms.Add(bgWord);

            if (USE_LEMMATIZATION)
            {
                if (bgWord.Length >= MIN_WORD_LENGTH_FOR_LEMMATIZATION &&
                    ruWord.Length >= MIN_WORD_LENGTH_FOR_LEMMATIZATION)
                {
                    List<string> bgLemmas = LemmaDictionaryUtils.GetBasicForms(bgWord, LANG_CODE_BG);
                    foreach (string bgLemma in bgLemmas)
                    {
                        bgWordForms.Add(bgLemma);
                    }
                }
            }

            // Prepare all forms of the Russian word
            ruWord = ruWord.ToLowerInvariant();
            HashSet<string> ruWordForms = new HashSet<string>();
            ruWordForms.Add(ruWord);
            string ruWordNoInflexion = TransformRussianInflextionToBulgarian(ruWord);
            ruWordForms.Add(ruWordNoInflexion);

            if (USE_LEMMATIZATION)
            {
                if (bgWord.Length >= MIN_WORD_LENGTH_FOR_LEMMATIZATION &&
                    ruWord.Length >= MIN_WORD_LENGTH_FOR_LEMMATIZATION)
                {
                    List<string> ruLemmas = LemmaDictionaryUtils.GetBasicForms(ruWord, LANG_CODE_RU);
                    foreach (string ruLemma in ruLemmas)
                    {
                        ruWordForms.Add(ruLemma);
                        string ruLemmaNoInflexion = TransformRussianInflextionToBulgarian(ruLemma);
                        ruWordForms.Add(ruLemmaNoInflexion);
                    }
                }
            }

            // Calculate MMEDR for each couple of Bulgarian and Russian wordform
            double maxMMEDR = 0;
            foreach (string bgWordForm in bgWordForms)
            {
                foreach (string ruWordForm in ruWordForms)
                {
                    double mmedr = CalcMMEDR(bgWordForm, ruWordForm);
                    if (mmedr > maxMMEDR)
                    {
                        maxMMEDR = mmedr;
                    }
                }
            }

            return maxMMEDR;
        }
    }
}
