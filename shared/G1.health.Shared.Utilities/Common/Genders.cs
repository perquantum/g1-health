namespace G1.health.Shared.Utilities.Common;
public class Genders
{
    public static readonly Dictionary<string, int> GendersList = new Dictionary<string, int>()
                    {
                        { "Male", 0 },
                        { "Female", 1 },
                        { "Other", 2 },
                        { "Not Mentioned", 3 }
                    };

    public const string NotMentioned = "Not Mentioned";
}