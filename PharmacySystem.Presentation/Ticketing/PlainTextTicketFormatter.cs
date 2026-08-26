using System;
using System.Text;

namespace PharmacySystem.Presentation
{
    // Extracted from PrintSale.cs's private AddCharacter/AddCenteredText/AddTwoColumns helpers -
    // the only genuinely pure, testable logic on that screen. Printer/hardware integration
    // (thermal-printer detection, spooler status, PrintDocument/PrintDialog, the WebBrowser HTML
    // ticket) stays in the Form; there is no meaningful seam to extract there.
    public class PlainTextTicketFormatter
    {
        private readonly int _width;
        private readonly StringBuilder _text = new StringBuilder();

        public PlainTextTicketFormatter(int width = 50)
        {
            _width = width;
        }

        public void AppendLine(string text) => _text.AppendLine(text);

        public void AddCharacter(string c)
        {
            string text = "";
            for (int i = 0; i < _width; i++)
            {
                text += c;
            }
            _text.AppendLine(text);
        }

        public void AddCenteredText(string text)
        {
            if (text.Length > _width)
            {
                _text.AppendLine(text.Substring(0, _width));
            }
            else
            {
                decimal spacesToAdd = Math.Truncate(Convert.ToDecimal((_width - text.Length) / 2));
                string spaces = "";
                for (int i = 0; i < spacesToAdd; i++)
                {
                    spaces += " ";
                }
                _text.AppendLine(spaces + text);
            }
        }

        public void AddTwoColumns(string leftText, string rightText)
        {
            int totalTextLength = leftText.Length + rightText.Length;
            if (totalTextLength > _width)
            {
                int availableSpace = _width - rightText.Length - 1; // -1 for at least one space
                if (availableSpace > 0)
                {
                    leftText = leftText.Substring(0, Math.Min(leftText.Length, availableSpace));
                    _text.AppendLine(leftText + " " + rightText);
                }
                else
                {
                    _text.AppendLine(leftText.Length > _width ? leftText.Substring(0, _width) : leftText);
                    _text.AppendLine(rightText.Length > _width ? rightText.Substring(0, _width) : rightText);
                }
            }
            else
            {
                int spacesCount = _width - totalTextLength;
                string spaces = "";
                for (int i = 0; i < spacesCount; i++)
                {
                    spaces += " ";
                }
                _text.AppendLine(leftText + spaces + rightText);
            }
        }

        public override string ToString() => _text.ToString();
    }
}
