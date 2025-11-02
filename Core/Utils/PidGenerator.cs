namespace RentMaster.Core.Utils;

public class PidGenerator
{
    private static readonly Random _random = new Random();

    public static string GeneratePid(int length = 10, string prefix = "AP")
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var pidBody = new string(Enumerable.Repeat(chars, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());

        return $"{prefix}{pidBody}";
    }
}