using System;

namespace MMEDR.Example
{
    class MMEDRExample
    {
        static void Main()
        {
            string bgWord = "афектирахме";
            string ruWord = "аффектировались";
            double mmedr = TECFF.Toolkit.MMEDR.CalculateSimilarity(bgWord, ruWord);

            Console.WriteLine("MMEDR({0}, {1}) = {2}", bgWord, ruWord, mmedr);
        }
    }
}
