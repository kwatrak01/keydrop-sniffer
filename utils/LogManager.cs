using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace KeyDrop_Sniffer.utils
{
    internal class LogManager
    {
        public static LogManager Instance
        {
            get
            {
                if (core == null)
                    core = new LogManager();
                return core;
            }
        }

        private static LogManager core;
        private static Dictionary<string, Paragraph> paragraphs = new Dictionary<string, Paragraph>();

        public void Success(RichTextBox box, string prefix, string message)
        {
            if (!paragraphs.ContainsKey(box.Name))
                Initialize(box);

            var logParagraph = paragraphs[box.Name];
            logParagraph.Inlines.Add(new Run("[ " + DateTime.Now.ToString("HH:mm:ss") + " ] ")
            {
                Foreground = Brushes.Gray
            });
            logParagraph.Inlines.Add(new Run("[ " + prefix + " ] ")
            {
                Foreground = Brushes.Green
            });
            logParagraph.Inlines.Add(message);
            logParagraph.Inlines.Add(Environment.NewLine);
            box.ScrollToEnd();
        }

        public void Fail(RichTextBox box, string prefix, string message)
        {
            if (!paragraphs.ContainsKey(box.Name))
                Initialize(box);

            var logParagraph = paragraphs[box.Name];
            logParagraph.Inlines.Add(new Run("[ " + DateTime.Now.ToString("HH:mm:ss") + " ] ")
            {
                Foreground = Brushes.Gray
            });
            logParagraph.Inlines.Add(new Run("[ " + prefix + " ] ")
            {
                Foreground = Brushes.Red
            });
            logParagraph.Inlines.Add(message);
            logParagraph.Inlines.Add(Environment.NewLine);
            box.ScrollToEnd();
        }

        public void Warning(RichTextBox box, string prefix, string message)
        {
            if (!paragraphs.ContainsKey(box.Name))
                Initialize(box);

            var logParagraph = paragraphs[box.Name];
            logParagraph.Inlines.Add(new Run("[ " + DateTime.Now.ToString("HH:mm:ss") + " ] ")
            {
                Foreground = Brushes.Gray
            });
            logParagraph.Inlines.Add(new Run("[ " + prefix + " ] ")
            {
                Foreground = Brushes.Orange
            });
            logParagraph.Inlines.Add(message);
            logParagraph.Inlines.Add(Environment.NewLine);
            box.ScrollToEnd();
        }

        public void Clear(string name)
        {
            if (!paragraphs.ContainsKey(name))
                return;

            paragraphs[name].Inlines.Clear();
        }

        private void Initialize(RichTextBox box)
        {
            var lp = new Paragraph();
            box.Document = new FlowDocument(lp);
            paragraphs.Add(box.Name, lp);
        }
    }
}
