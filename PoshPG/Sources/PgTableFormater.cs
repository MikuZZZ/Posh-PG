using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Facebook.Yoga;
using Npgsql;

namespace PoshPG
{
    public abstract class PgTableFormater
    {
        internal abstract Task<string> Format(NpgsqlDataReader reader);
    }

    public class PgTableDefaultFormater : PgTableFormater
    {
        internal override async Task<string> Format(NpgsqlDataReader reader)
        {
            var result = new List<Dictionary<string, object>>();

            var colNames = new string[reader.FieldCount];
            var colTypes = new string[reader.FieldCount];
            var flexChild = new YogaNode[reader.FieldCount];
            var data = new List<string[]>();

            var consoleWidth = 200;

            try
            {
                consoleWidth = Console.WindowWidth;
            }
            catch
            {
            }

            // Console.WriteLine($"Console Width: {consoleWidth}");

            var flexRoot = new YogaNode
            {
                Width = consoleWidth,
                Height = 1,
                Wrap = YogaWrap.NoWrap,
                FlexDirection = YogaFlexDirection.Row,
                JustifyContent = YogaJustify.FlexStart
            };

            for (var i = 0; i < reader.FieldCount; i++)
            {
                colNames[i] = reader.GetName(i);
                colTypes[i] = reader.GetDataTypeName(i);
                flexChild[i] = new YogaNode
                {
                    MarginLeft = 0,
                    MarginRight = i == reader.FieldCount - 1 ? 0 : 1,
                    FlexBasis = colNames[i].Length,
                    FlexShrink = 0,
                    FlexGrow = colTypes[i] == "boolean" ? 0 : 1,
                    Height = 1
                };
                flexRoot.Insert(i, flexChild[i]);
            }

            data.Add(colNames);
            data.Add(colNames.Select(col => new string(Enumerable.Range(0, col.Length).SelectMany(x => "-").ToArray()))
                .ToArray());

            flexRoot.CalculateLayout();

            while (await reader.ReadAsync())
            {
                if (data.Count >= 500)
                {
                    await reader.CloseAsync();
                    break;
                }

                var row = new string[reader.FieldCount];
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    row[i] = reader.GetValue(i).ToString();
                    flexChild[i].Width = row[i].Length;
                    if (row[i].Length > flexChild[i].LayoutWidth)
                        flexChild[i].FlexGrow = row[i].Length / flexChild[i].LayoutWidth * 2;
                }

                data.Add(row);
            }

            flexRoot.CalculateLayout();

            var tableStr = "";
            foreach (var row in data)
            {
                var colStr = new string[row.Length];
                for (var i = 0; i < row.Length; i++)
                {
                    var layoutWidth = (int) Math.Round(flexChild[i].LayoutWidth);
                    colStr[i] = row[i]
                        .Substring(0, Math.Min(layoutWidth, row[i].Length))
                        .PadRight(layoutWidth, ' ');
                }

                var rowStr = string.Join(' ', colStr);
                if (rowStr.Length > consoleWidth) rowStr = rowStr.Substring(0, consoleWidth - 2) + "..";

                tableStr += rowStr + '\n';
            }

            await reader.CloseAsync();
            return tableStr;
        }
    }
}