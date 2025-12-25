using Microsoft.Extensions.DependencyInjection;
using OlieBufr.Lib;
using OlieBufr.Lib.Models.Messages;
using OlieBufr.Lib.Services;

namespace OlieBufr.Cli;

internal static class Program
{
    private static ServiceProvider _serviceProvider = new ServiceCollection().BuildServiceProvider();
    private static Dictionary<string, Element> _elements = [];
    private static Dictionary<string, Sequence> _sequences = [];

    private static int Main(string[] args)
    {
        Console.WriteLine($"OlieBufr.Cli {DateTime.UtcNow:s}");

        CreateService();
        Initialize();

        var filename = GetFilename(args);

        Console.WriteLine($"Verifying {filename}");

        using var br = new BinaryReader(File.OpenRead(filename));
        var bufrFile = BufrFile.Decode(_elements, _sequences, br);

        Console.WriteLine($"Success! The file contained {bufrFile.BufrMessages.Count} messages.");

        return 0;
    }

    private static string GetFilename(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine();
            Console.WriteLine("Usage: OlieBufr.Cli bufr_filename");

            Environment.Exit(1);
        }

        return args[0];
    }

    private static T CreateService<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    private static void CreateService()
    {
        var services = new ServiceCollection();

        #region Common Dependencies

        services.AddScoped<IOlieService, OlieService>();

        #endregion

        _serviceProvider = services.BuildServiceProvider();
    }

    private static void Initialize()
    {
        var olieService = CreateService<IOlieService>();

        _elements = new Elements(olieService).Reference;
        _sequences = new Sequences(olieService).Reference;
    }
}
