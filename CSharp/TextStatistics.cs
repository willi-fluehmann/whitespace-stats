using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhitespaceStats
{
    public class TextStatistics
    {
        public long AllLines { get; internal set; }

        public long LeadingSpacesLines { get; internal set; }
        public long LeadingTabsLines { get; internal set; }
        public long LeadingMixedLines { get; internal set; }

        public long TrailingWhitespaceLines { get; internal set; }

        public long LfLines { get; internal set; }
        public long CrLfLines { get; internal set; }

        public long NonLeadingTabsLines { get; internal set; }
        public long AnySoleCrLines { get; internal set; }

        public long AllCharacters { get; internal set; }
        public long NonPrintableCharacters { get; internal set; }

        private TextStatistics()
        {
        }

        public static TextStatistics Analyze(TextReader reader)
        {
            var statistics = new TextStatistics();
            var characterEvaluator = new CharacterEvaluator(statistics);
            var lineSeparator = new LineSeparator(statistics, new LineEvaluator(statistics));

            var c = new char[1];
            while (reader.Read(c, index: 0, count: 1) > 0)
            {
                characterEvaluator.Evaluate(c[0]);
                lineSeparator.Evaluate(c[0]);
            }
            lineSeparator.FileDone();

            return statistics;
        }

        private class CharacterEvaluator
        {
            private TextStatistics _statistics;

            public CharacterEvaluator(TextStatistics statistics)
            {
                _statistics = statistics;
            }

            public void Evaluate(char c)
            {
                _statistics.AllCharacters++;

                bool printable = (c >= ' ') || (c == '\t') || (c == '\n') || (c == '\r');
                if (!printable)
                {
                    _statistics.NonPrintableCharacters++;
                }
            }
        }

        private class LineSeparator
        {
            private TextStatistics _statistics;
            private LineEvaluator _lineEvaluator;

            private char? _bufferedPrevious = null;

            public LineSeparator(TextStatistics statistics, LineEvaluator lineEvaluator)
            {
                _statistics = statistics;
                _lineEvaluator = lineEvaluator;
            }

            public void Evaluate(char c)
            {
                if (c == '\n')
                {
                    if (_bufferedPrevious == '\r')
                    {
                        _statistics.CrLfLines++;
                    }
                    else
                    {
                        if (_bufferedPrevious.HasValue)
                        {
                            _lineEvaluator.Evaluate(_bufferedPrevious.Value);
                        }
                        _statistics.LfLines++;
                    }

                    _lineEvaluator.LineDone();
                    _bufferedPrevious = null;
                }
                else
                {
                    if (_bufferedPrevious.HasValue)
                    {
                        _lineEvaluator.Evaluate(_bufferedPrevious.Value);
                    }
                    _bufferedPrevious = c;
                }
            }

            public void FileDone()
            {
                if (_bufferedPrevious.HasValue)
                {
                    _lineEvaluator.Evaluate(_bufferedPrevious.Value);
                    _bufferedPrevious = null;

                    _lineEvaluator.LineDone();
                }
            }
        }

        private class LineEvaluator
        {
            private TextStatistics _statistics;

            private bool _afterLeading = false;

            private bool _leadingSpaces = false;
            private bool _leadingTabs = false;

            private bool _trailingWhitespace = false;

            private bool _nonLeadingTabs = false;
            private bool _anySoleCr = false;

            public LineEvaluator(TextStatistics statistics)
            {
                _statistics = statistics;
            }

            public void Evaluate(char c)
            {
                bool isWhiteSpace = Char.IsWhiteSpace(c);

                // evaluate leading whitespace
                if (!_afterLeading)
                {
                    if (isWhiteSpace)
                    {
                        switch (c)
                        {
                            case ' ': _leadingSpaces = true; break;
                            case '\t': _leadingTabs = true; break;

                            default:
                                // What to do with other whitespace?
                                break;
                        }
                    }
                    else
                    {
                        _afterLeading = true;
                    }
                }
                else
                {
                    if (c == '\t')
                    {
                        _nonLeadingTabs = true;
                    }
                }

                // evaluate trailing whitespace
                if (isWhiteSpace)
                {
                    _trailingWhitespace = true;
                }
                else
                {
                    _trailingWhitespace = false;
                }

                // evaluate other characters
                if (c == '\r')
                {
                    _anySoleCr = true;
                }
            }

            public void LineDone()
            {
                if (_leadingSpaces && _leadingTabs)
                {
                    _statistics.LeadingMixedLines++;
                }
                else if (_leadingSpaces)
                {
                    _statistics.LeadingSpacesLines++;
                }
                else if (_leadingTabs)
                {
                    _statistics.LeadingTabsLines++;
                }

                if (_trailingWhitespace)
                {
                    _statistics.TrailingWhitespaceLines++;
                }

                if (_nonLeadingTabs)
                {
                    _statistics.NonLeadingTabsLines++;
                }

                if (_anySoleCr)
                {
                    _statistics.AnySoleCrLines++;
                }

                _statistics.AllLines++;

                _afterLeading = false;
                _leadingSpaces = false;
                _leadingTabs = false;
                _trailingWhitespace = false;
                _nonLeadingTabs = false;
                _anySoleCr = false;
            }
        }
    }
}
