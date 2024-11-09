using System.Data;
using System.Globalization;
using CsvHelper;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ConsoleApp;

public class Program
{
    public static void Main(string[] args)
    {
        var app = new CommandApp();
        app.Configure(config =>
        {
            config.AddCommand<FormatCsvToTable>("format-csv-to-table")
                .WithDescription("pass target file to printed")
                .WithExample("format-csv-to-table", "file.csv");
        });

        app.Run(args);
    }
}

public class FormatCsvToTable : Command<FormatCsvToTable.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<FilePath>")]
        public string FilePath { get; set; }
    }


    public override int Execute(CommandContext context, Settings settings)
    {
        BuildTable(settings);
        return 0;
    }

    private string GetFilePath(string path)
    {
        return Path.Combine(Environment.CurrentDirectory, path);
    }

    private (List<string> Headers, List<string[]> Rows) GetFileContent(Settings settings)
    {
        //C:\Users\gusta\RiderProjects\EverythingComparer\ConsoleApp\bin\Debug\net8.0\files\file.csv
        var path = GetFilePath(settings.FilePath);
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord.ToList();
        var rows = new List<string[]>();
        while (csv.Read())
        {
            var row = new string[headers.Count];
            for (var i = 0; i < headers.Count; i++) 
            {
                row[i] = csv.GetField(i);
            }
            
            rows.Add(row);
        }
        
        return (headers, rows);
    }

    private void BuildTable(Settings settings)
    {
        var table = new Table();

        var (headers, rows) = GetFileContent(settings);
        
        foreach (var header in headers)
            table.AddColumn(new TableColumn(header));
        
        foreach (var row in rows)
            table.AddRow(row.Select(cell => new Markup(cell ?? string.Empty)).ToArray());
        
        AnsiConsole.Write(table);
    }
}