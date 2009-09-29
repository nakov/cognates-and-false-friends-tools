using System;
using TECFF.Toolkit;

namespace FFExtract.Example
{
    class FFExtractExample
    {
        const string CORPUS_FILE = @"..\..\RU-BG-Corpus-Beliaev.txt";
        const string FRIENDS_JUDGE_FILE = @"..\..\FalseFriends-Cognates-From-Beliaev-Judge.csv";
        const string OUTPUT_FILE = @"FalseFriendsFromParallelCorpus.csv";

        static void Main()
        {
            FalseFriendsExtractor extractor = new FalseFriendsExtractor();

            Console.WriteLine("Loading corpus...");
            extractor.LoadSentencesAndExtractWords(CORPUS_FILE);
            extractor.LoadFriendsJudgeFile(FRIENDS_JUDGE_FILE);
            Console.WriteLine("Done.");

            Console.WriteLine("Calculating statistics...");
            extractor.CalculateStatistics();
            Console.WriteLine("Done.");

            Console.WriteLine("Extracting cognates and false friends...");
            extractor.ExtractCognatesAndFalseFriends(OUTPUT_FILE);
            Console.WriteLine("Done.");

        }
    }
}
