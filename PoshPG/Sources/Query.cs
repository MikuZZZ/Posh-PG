using System;
using System.Management.Automation;
using TTRider.PowerShellAsync;
using Npgsql;
using System.Threading.Tasks;
using System.Collections.Generic;
using Facebook.Yoga;
using System.Linq;

namespace PoshPG
{
    [Cmdlet(VerbsLifecycle.Invoke, "PgQuery")]
    [OutputType(typeof(string))]
    public class InvokePgQuery : AsyncCmdlet
    {

        [Parameter(Mandatory = false)]
        public string SessionId = "";

        [Parameter(Mandatory = false, HelpMessage = "Session Alias")]
        public string Session = "";

        [Parameter(Mandatory = true)]
        public string Query { get; set; }

        internal async Task<string> GetQueryResult(NpgsqlConnection connection, string query)
        {
            var result = new List<Dictionary<string, object>>();

            await using var cmd = new NpgsqlCommand(query, connection);
            await using var reader = await cmd.ExecuteReaderAsync();

            var colNames = new string[reader.FieldCount];
            var colTypes = new string[reader.FieldCount];
            var flexChild = new YogaNode[reader.FieldCount];
            var data = new List<string[]>();

            var consoleWidth = 200;

            try
            {
                consoleWidth = Console.WindowWidth;
            }
            catch { }

            // Console.WriteLine($"Console Width: {consoleWidth}");

            var flexRoot = new YogaNode()
            {
                Width = consoleWidth,
                Height = 1,
                Wrap = YogaWrap.NoWrap,
                FlexDirection = YogaFlexDirection.Row,
                JustifyContent = YogaJustify.SpaceBetween,
            };

            for (var i = 0; i < reader.FieldCount; i++)
            {
                colNames[i] = reader.GetName(i);
                colTypes[i] = reader.GetDataTypeName(i);
                flexChild[i] = new YogaNode()
                {
                    MarginLeft = 0,
                    MarginRight = i == reader.FieldCount - 1 ? 0 : 1,
                    FlexBasis = colNames[i].Length,
                    FlexShrink = 0,
                    FlexGrow = colTypes[i] == "boolean" ? 0 : 1,
                    Height = 1,
                };
                flexRoot.Insert(i, flexChild[i]);
            }

            data.Add(colNames);
            data.Add(colNames.Select(col => new String(Enumerable.Range(0, col.Length).SelectMany(x => "-").ToArray())).ToArray<string>());

            flexRoot.CalculateLayout();

            while (await reader.ReadAsync())
            {
                if (data.Count >= 100)
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
                    {
                        flexChild[i].FlexGrow = row[i].Length / flexChild[i].LayoutWidth * 2;
                    }
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
                    var layoutWidth = (int)Math.Round(flexChild[i].LayoutWidth);
                    colStr[i] = row[i]
                        .Substring(0, Math.Min(layoutWidth, row[i].Length))
                        .PadRight(layoutWidth, ' ');
                }

                var rowStr = String.Join(' ', colStr);
                if (rowStr.Length > consoleWidth)
                {
                    rowStr = rowStr.Substring(0, consoleWidth - 2) + "..";
                }

                tableStr += rowStr + '\n';
            }

            return tableStr;
        }

        protected override async Task ProcessRecordAsync()
        {
            try
            {
                var currentSession = this.SessionState.PSVariable.GetValue("Global:PgSessions") as List<PgSession>;
                var conn = null as NpgsqlConnection;
                if (SessionId != "")
                {
                    conn = currentSession.Find(conn => conn.SessionId == Int32.Parse(SessionId)).Connection;
                }
                else if (Session != "")
                {
                    conn = currentSession.Find(conn => conn.Alias == Session).Connection;
                }
                else
                {
                    throw new System.Exception("Either Session or SessionId Required");
                }

                var table = await GetQueryResult(conn, Query);
                WriteObject(table);
            }
            catch (Exception e)
            {
                WriteObject(e);
            }

        }
    }
}
