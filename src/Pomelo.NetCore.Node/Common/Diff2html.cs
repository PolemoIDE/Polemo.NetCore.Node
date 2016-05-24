using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pomelo.NetCore.Node.Common
{
    public class FileDiff
    {
        public string OriginalFilename { get; set; }
        public string Filename { get; set; }
        public List<LineDiff> LineDiffs { get; set; } = new List<LineDiff>();
        public enum FileDiffType
        {
            Addition,
            Deletion,
            Modification
        }
        public FileDiffType Type { get; set; }

        public class LineDiff
        {
            public int OriginalLineNumber { get; set; } = -1;
            public int NewLineNumber { get; set; } = -1;
            public enum LineDiffType
            {
                Addition,
                Deletion,
                Info,
                NA
            }
            public LineDiffType Type { get; set; }

            public string Line { get; set; }
        }
    }

    public class Diff2html
    {
        static public IList<FileDiff> GetDiff(string rawDiff)
        {
            var diffLines = rawDiff.Split('\n');

            var fileDiffs = new List<FileDiff>();
            var diffsGroupByFile = new List<List<string>>();

            foreach (var diffLine in diffLines)
            {
                if (diffLine.StartsWith("diff --git"))
                {
                    var thisFileDiff = new List<string>();
                    thisFileDiff.Add(diffLine);
                    diffsGroupByFile.Add(thisFileDiff);
                }
                else
                {
                    try
                    {
                        diffsGroupByFile.Last().Add(diffLine);
                    }
                    catch { }
                }

            }

            foreach (var diffsGroup in diffsGroupByFile)
            {
                var fileDiff = new FileDiff();

                // diff --git a/AkariExecEngine.h b/AkariExecEngine.h
                foreach (Match match in Regex.Matches(diffsGroup[0], @"diff --git a/(?<oriname>.+) b/(?<newname>.+)", RegexOptions.None))
                {
                    fileDiff.OriginalFilename = match.Groups["oriname"].Value;
                    fileDiff.Filename = match.Groups["newname"].Value;
                }

                // Find first line starts with @@
                int ln = 0;
                for (int i = 1; i < diffsGroup.Count; ++i)
                {
                    if (diffsGroup[i].StartsWith("@@"))
                    {
                        ln = i;
                        break;
                    }
                }

                if (diffsGroup[ln - 2] == "--- /dev/null")
                    fileDiff.Type = FileDiff.FileDiffType.Addition;
                else if (diffsGroup[ln - 1] == "+++ /dev/null")
                    fileDiff.Type = FileDiff.FileDiffType.Deletion;
                else
                    fileDiff.Type = FileDiff.FileDiffType.Modification;

                int oriStart = 0, newStart = 0;
                for (; ln < diffsGroup.Count; ++ln)
                {
                    var line = diffsGroup[ln];

                    if (line.Length == 0)
                        continue;

                    // @@ -26,11 +26,25 @@
                    if (line.StartsWith("@@"))
                    {
                        var segs = line.Split(' ');
                        var oris = segs[1].Split(',');
                        oriStart = -int.Parse(oris[0]);
                        var news = segs[2].Split(',');
                        newStart = int.Parse(news[0]);

                        fileDiff.LineDiffs.Add(new FileDiff.LineDiff
                        {
                            Line = line,
                            OriginalLineNumber = 0,
                            NewLineNumber = 0,
                            Type = FileDiff.LineDiff.LineDiffType.NA
                        });

                        continue;
                    }

                    if (line.StartsWith("+"))
                    {
                        fileDiff.LineDiffs.Add(new FileDiff.LineDiff
                        {
                            Line = line.Remove(0, 1),
                            NewLineNumber = newStart++,
                            OriginalLineNumber = 0,
                            Type = FileDiff.LineDiff.LineDiffType.Addition
                        });

                        continue;
                    }

                    if (line.StartsWith("-"))
                    {
                        fileDiff.LineDiffs.Add(new FileDiff.LineDiff
                        {
                            Line = line.Remove(0, 1),
                            NewLineNumber = 0,
                            OriginalLineNumber = oriStart++,
                            Type = FileDiff.LineDiff.LineDiffType.Deletion
                        });

                        continue;
                    }

                    if (line.StartsWith("\\"))
                    {
                        fileDiff.LineDiffs.Add(new FileDiff.LineDiff
                        {
                            Line = @"<svg aria-hidden=""true"" class=""octicon octicon-no-newline"" height=""16"" version=""1.1"" viewBox=""0 0 16 16"" width=""16""><path d=""M16 5v3c0 0.55-0.45 1-1 1H12v2L9 8l3-3v2h2V5h2zM8 8c0 2.2-1.8 4-4 4S0 10.2 0 8s1.8-4 4-4 4 1.8 4 4zM1.5 9.66l4.16-4.16c-0.48-0.31-1.05-0.5-1.66-0.5-1.66 0-3 1.34-3 3 0 0.61 0.19 1.17 0.5 1.66z m5.5-1.66c0-0.61-0.19-1.17-0.5-1.66L2.34 10.5c0.48 0.31 1.05 0.5 1.66 0.5 1.66 0 3-1.34 3-3z""></path></svg>",
                            NewLineNumber = 0,
                            OriginalLineNumber = 0,
                            Type = FileDiff.LineDiff.LineDiffType.Info
                        });

                        continue;
                    }

                    //warning: LF will be replaced by CRLF in
                    // The file will have its original line endings in your working directory.
                    if (line.StartsWith("warning:") || line.StartsWith("The file"))
                        continue;

                    fileDiff.LineDiffs.Add(new FileDiff.LineDiff
                    {
                        Line = line.Remove(0, 1),
                        NewLineNumber = newStart++,
                        OriginalLineNumber = oriStart++,
                        Type = FileDiff.LineDiff.LineDiffType.NA
                    });
                }

                fileDiffs.Add(fileDiff);
            }

            return fileDiffs;
        }

        // OldName, NewName, Diff, Type
        static public List<Tuple<string, string, string, string>> DiffToHTML(IList<FileDiff> diff)
        {
            var ret = new List<Tuple<string, string, string, string>>();
            foreach (var file in diff)
            {
                StringBuilder builder = new StringBuilder();
                foreach (var line in file.LineDiffs)
                {
                    char symbol;
                    string trclass = string.Empty;
                    switch (line.Type)
                    {
                        case FileDiff.LineDiff.LineDiffType.Addition:
                            symbol = '+';
                            trclass = "class=\"addition\"";
                            break;
                        case FileDiff.LineDiff.LineDiffType.Deletion:
                            symbol = '-';
                            trclass = "class=\"deletion\"";
                            break;
                        case FileDiff.LineDiff.LineDiffType.Info:
                        case FileDiff.LineDiff.LineDiffType.NA:
                        default:
                            symbol = ' ';
                            break;
                    }

                    builder.Append("<tr ").Append(trclass).AppendLine(">");

                    builder.Append("  <td>").
                            Append(line.OriginalLineNumber == 0 ? "" : line.OriginalLineNumber.ToString()).
                            AppendLine("</td>").AppendLine();

                    builder.Append("  <td>").
                            Append(line.NewLineNumber == 0 ? "" : line.NewLineNumber.ToString()).
                            AppendLine("</td>").AppendLine();

                    builder.Append("  <td>").Append(symbol).AppendLine("</td>");

                    builder.AppendLine("  <td>").
                            Append(line.Line.Replace(" ", "&nbsp;")).Append("</td>").AppendLine();

                    builder.AppendLine("</tr>");
                }

                ret.Add(new Tuple<string, string, string, string>(file.OriginalFilename, file.Filename, builder.ToString(), file.Type.ToString()));
            }

            return ret;
        }
    }

}

