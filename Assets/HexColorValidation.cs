using UnityEngine;
using TMPro;

namespace aurw
{
    public class StrictHexValidator : TMP_InputValidator
    {
        public override char Validate(ref string text, ref int pos, char ch)
        {
            if (ch == '\b') return ch;

            if ((ch >= '0' && ch <= '9') ||
                (ch >= 'A' && ch <= 'F') ||
                (ch >= 'a' && ch <= 'f'))
            {

                text = text.Insert(pos, ch.ToString());
                pos++;
                return ch;
            }

            return '\0';
        }
    }
}