﻿namespace OJS.Services.Administration.Business.Similarity;

using System.Collections.Generic;
using System.IO;
using System.Text;

public class SortAndTrimLinesVisitor : IDetectSimilarityVisitor
{
    public string Visit(string text)
    {
        text = text.Trim();

        var lines = new List<string>();
        using (var stringReader = new StringReader(text))
        {
            string line;
            while ((line = stringReader.ReadLine()!) != null)
            {
                line = line.Trim();
                lines.Add(line);
            }
        }

        lines.Sort();

        var stringBuilder = new StringBuilder();
        foreach (var line in lines)
        {
            stringBuilder.AppendLine(line);
        }

        return stringBuilder.ToString();
    }
}