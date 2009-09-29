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
                else if (ch == 'à')
                {
                    latin.Append('a');
                }
                else if (ch == 'á')
                {
                    latin.Append('b');
                }
                else if (ch == 'â')
                {
                    latin.Append('v');
                }
                else if (ch == 'ã')
                {
                    latin.Append('g');
                }
                else if (ch == 'ä')
                {
                    latin.Append('d');
                }
                else if (ch == 'å')
                {
                    latin.Append('e');
                }
                else if (ch == 'æ')
                {
                    latin.Append("zh");
                }
                else if (ch == 'ç')
                {
                    latin.Append('z');
                }
                else if (ch == 'è')
                {
                    latin.Append('i');
                }
                else if (ch == 'é')
                {
                    latin.Append('j');
                }
                else if (ch == 'ê')
                {
                    latin.Append('k');
                }
                else if (ch == 'ë')
                {
                    latin.Append('l');
                }
                else if (ch == 'ì')
                {
                    latin.Append('m');
                }
                else if (ch == 'í')
                {
                    latin.Append('n');
                }
                else if (ch == 'î')
                {
                    latin.Append('o');
                }
                else if (ch == 'ï')
                {
                    latin.Append('p');
                }
                else if (ch == 'ð')
                {
                    latin.Append('r');
                }
                else if (ch == 'ñ')
                {
                    latin.Append('s');
                }
                else if (ch == 'ò')
                {
                    latin.Append('t');
                }
                else if (ch == 'ó')
                {
                    latin.Append('u');
                }
                else if (ch == 'ô')
                {
                    latin.Append('f');
                }
                else if (ch == 'õ')
                {
                    latin.Append('h');
                }
                else if (ch == '÷')
                {
                    latin.Append("ch");
                }
                else if (ch == 'ö')
                {
                    latin.Append('c');
                }
                else if (ch == 'ø')
                {
                    latin.Append("sh");
                }
                else if (ch == 'ù')
                {
                    latin.Append("sht");
                }
                else if (ch == 'ú')
                {
                    latin.Append("y");
                }
                else if (ch == 'ü')
                {
                    latin.Append("x");
                }
                else if (ch == 'ý')
                {
                    latin.Append("_");
                }
                else if (ch == 'û')
                {
                    latin.Append("y");
                }
                else if (ch == 'þ')
                {
                    latin.Append("yu");
                }
                else if (ch == 'ÿ')
                {
                    latin.Append("ya");
                }
                else if (ch == '-')
                {
                    latin.Append("-");
                }
                else if (ch == '¸')
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
