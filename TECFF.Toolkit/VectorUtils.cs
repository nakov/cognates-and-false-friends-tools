using System;
using System.Collections.Generic;

namespace TECFF.Toolkit
{
    /// <summary>
    /// Provides utility functions for calculating vector distance measures
    /// such as cosine and Dice coefficient.
    /// </summary>
    public class VectorUtils
    {
        public static double[] CalcRanksOfVector(double[] vector)
        {
            double[] ranks = new double[vector.Length];
            for (int index = 0; index < vector.Length; index++)
            {
                long smallerValuesCount = 0;
                foreach (double value in vector)
                {
                    if (value < vector[index])
                    {
                        smallerValuesCount++;
                    }
                }
                ranks[index] = 1 + smallerValuesCount;
            }
            return ranks;
        }

        public static double CalcDistanceBetweenRankedVectors(
            double[] firstVector, double[] secondVector)
        {
            if (firstVector.Length != secondVector.Length)
            {
                throw new ArgumentException("Vectors should have the same size.");
            }

            double sum = 0;
            for (int i = 0; i < firstVector.Length; i++)
            {
                double a = firstVector[i];
                double b = secondVector[i];
                sum = sum + (a - b) * (a - b);
            }
            double n = firstVector.Length;
            double distance = 1 - 6 * sum / (4 * n * (n * n - 1));
            return distance;
        }

        public static double CalcCosinusBetweenVectors(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("Vectors should be the same size!");
            }

            double sumXiYi = 0;
            double sumXi2 = 0;
            double sumYi2 = 0;
            for (int i = 0; i < x.Length; i++)
            {
                sumXiYi += x[i] * y[i];
                sumXi2 += x[i] * x[i];
                sumYi2 += y[i] * y[i];
            }

            double length = Math.Sqrt(sumXi2 * sumYi2);
            if (length == 0)
            {
                return 0;
            }
            else
            {
                double cosinus = sumXiYi / length;
                return cosinus;
            }
        }

        public static double SumVector(double[] vector)
        {
            double sum = 0;
            foreach (double element in vector)
            {
                sum += element;
            }
            return sum;
        }

        public static double CalcDiceCoeffBetweenVectors(double[] a, double[] b)
        {
            if (a.Length != a.Length)
            {
                throw new ArgumentException("Vectors should be the same size!");
            }

            double sumMin = 0;
            double sumA = 0;
            double sumB = 0;
            for (int i = 0; i < a.Length; i++)
            {
                sumMin += Math.Min(a[i], b[i]);
                sumA += a[i];
                sumB += b[i];
            }

            double distance = 2 * sumMin / (sumA + sumB);
            return distance;
        }

        public static double CalcCosinusBetweenWordsCounts(
            WordsAndCounts wordsCounts1, WordsAndCounts wordsCounts2)
        {
            // Create a list (union) of all words from the two sets of words
            Dictionary<string, bool> allWords = new Dictionary<string, bool>();
            foreach (string word in wordsCounts1.Words)
            {
                allWords.Add(word, true);
            }
            foreach (string word in wordsCounts2.Words)
            {
                if (!allWords.ContainsKey(word))
                {
                    allWords.Add(word, true);
                }
            }

            // Create the first occurences vector
            double[] vector1 = new double[allWords.Count];
            int index1 = 0;
            foreach (string word in allWords.Keys)
            {
                vector1[index1] = wordsCounts1[word];
                index1++;
            }

            // Create the second occurences vector
            double[] vector2 = new double[allWords.Count];
            int index2 = 0;
            foreach (string word in allWords.Keys)
            {
                vector2[index2] = wordsCounts2[word];
                index2++;
            }

            double distance = VectorUtils.CalcCosinusBetweenVectors(vector1, vector2);
            return distance;
        }
    }

    public enum VectorDiffAlgorithm
    {
        COSINE,
        DICE_COEFFICIENT
    }
}