using System;
using System.Text;

namespace TECFF.Toolkit
{
    /// <summary>
    /// Utility class for converting cyrillic letters to their latin equivalents.
    /// </summary>
    public class Cyr2LatUtils
    {
        public static string CyrillicToLatin(string text)
        {
            text = text.ToLowerInvariant();
            StringBuilder latin = new StringBuilder();
            foreach (char ch in text)
            {
                if (ch >= 'a' && ch <= 'z')
                {
                    latin.Append(ch);
                }
                else if (ch == '�')
                {
                    latin.Append('a');
                }
                else if (ch == '�')
                {
                    latin.Append('b');
                }
                else if (ch == '�')
                {
                    latin.Append('v');
                }
                else if (ch == '�')
                {
                    latin.Append('g');
                }
                else if (ch == '�')
                {
                    latin.Append('d');
                }
                else if (ch == '�')
                {
                    latin.Append('e');
                }
                else if (ch == '�')
                {
                    latin.Append("zh");
                }
                else if (ch == '�')
                {
                    latin.Append('z');
                }
                else if (ch == '�')
                {
                    latin.Append('i');
                }
                else if (ch == '�')
                {
                    latin.Append('j');
                }
                else if (ch == '�')
                {
                    latin.Append('k');
                }
                else if (ch == '�')
                {
                    latin.Append('l');
                }
                else if (ch == '�')
                {
                    latin.Append('m');
                }
                else if (ch == '�')
                {
                    latin.Append('n');
                }
                else if (ch == '�')
                {
                    latin.Append('o');
                }
                else if (ch == '�')
                {
                    latin.Append('p');
                }
                else if (ch == '�')
                {
                    latin.Append('r');
                }
                else if (ch == '�')
                {
                    latin.Append('s');
                }
                else if (ch == '�')
                {
                    latin.Append('t');
                }
                else if (ch == '�')
                {
                    latin.Append('u');
                }
                else if (ch == '�')
                {
                    latin.Append('f');
                }
                else if (ch == '�')
                {
                    latin.Append('h');
                }
                else if (ch == '�')
                {
                    latin.Append("ch");
                }
                else if (ch == '�')
                {
                    latin.Append('c');
                }
                else if (ch == '�')
                {
                    latin.Append("sh");
                }
                else if (ch == '�')
                {
                    latin.Append("sht");
                }
                else if (ch == '�')
                {
                    latin.Append("y");
                }
                else if (ch == '�')
                {
                    latin.Append("x");
                }
                else if (ch == '�')
                {
                    latin.Append("_");
                }
                else if (ch == '�')
                {
                    latin.Append("y");
                }
                else if (ch == '�')
                {
                    latin.Append("yu");
                }
                else if (ch == '�')
                {
                    latin.Append("ya");
                }
                else if (ch == '-')
                {
                    latin.Append("-");
                }
                else if (ch == '�')
                {
                    latin.Append("yo");
                }
                else if (ch == ' ')
                {
                    latin.Append(" ");
                }
                else
                {
                    throw new Exception("Transliteration failed: Invalid char " + ch);
                }
            }
            string latinText = latin.ToString();
            return latinText;
        }
    }
}
