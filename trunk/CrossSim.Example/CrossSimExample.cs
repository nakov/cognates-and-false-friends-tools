using System;
using TECFF.Toolkit;

namespace CrossSim.Example
{
    class CrossSimExample
    {
        static void Main()
        {
            string bgWord = "бира";
            string ruWord = "пиво";
            double similarity = SemanticSimilarityUtils.CrossSim(bgWord, ruWord);
            Console.WriteLine("CrossSim({0}, {1}) = {2}", bgWord, ruWord, similarity);

            bgWord = "бира";
            ruWord = "разливать";
            similarity = SemanticSimilarityUtils.CrossSim(bgWord, ruWord);
            Console.WriteLine("CrossSim({0}, {1}) = {2}", bgWord, ruWord, similarity);

            bgWord = "бира";
            ruWord = "вода";
            similarity = SemanticSimilarityUtils.CrossSim(bgWord, ruWord);
            Console.WriteLine("CrossSim({0}, {1}) = {2}", bgWord, ruWord, similarity);

            bgWord = "бира";
            ruWord = "Москва";
            similarity = SemanticSimilarityUtils.CrossSim(bgWord, ruWord);
            Console.WriteLine("CrossSim({0}, {1}) = {2}", bgWord, ruWord, similarity);

            bgWord = "бира";
            ruWord = "молоко";
            similarity = SemanticSimilarityUtils.CrossSim(bgWord, ruWord);
            Console.WriteLine("CrossSim({0}, {1}) = {2}", bgWord, ruWord, similarity);
        }
    }
}
