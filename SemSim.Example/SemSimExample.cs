using System;
using TECFF.Toolkit;

namespace SemSim.Example
{
    class SemSimExample
    {
        static void Main()
        {
            string word1 = "car";
            string word2 = "automobile";
            double similarity = SemanticSimilarityUtils.SemSim(word1, word2, SemanticSimilarityUtils.LANG_CODE_EN);
            Console.WriteLine("SemSim({0}, {1}) = {2}", word1, word2, similarity);

            word1 = "car";
            word2 = "motor";
            similarity = SemanticSimilarityUtils.SemSim(word1, word2, SemanticSimilarityUtils.LANG_CODE_EN);
            Console.WriteLine("SemSim({0}, {1}) = {2}", word1, word2, similarity);

            word1 = "car";
            word2 = "drive";
            similarity = SemanticSimilarityUtils.SemSim(word1, word2, SemanticSimilarityUtils.LANG_CODE_EN);
            Console.WriteLine("SemSim({0}, {1}) = {2}", word1, word2, similarity);

            word1 = "car";
            word2 = "water";
            similarity = SemanticSimilarityUtils.SemSim(word1, word2, SemanticSimilarityUtils.LANG_CODE_EN);
            Console.WriteLine("SemSim({0}, {1}) = {2}", word1, word2, similarity);

            word1 = "car";
            word2 = "Obama";
            similarity = SemanticSimilarityUtils.SemSim(word1, word2, SemanticSimilarityUtils.LANG_CODE_EN);
            Console.WriteLine("SemSim({0}, {1}) = {2}", word1, word2, similarity);
        }
    }
}
