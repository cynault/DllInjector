using ArgParseCS;
namespace DllInject;

public class InjectArgParser
{
    private readonly ArgParse _parseResult;
    private readonly OptionSet _activeOptionSet;
    public InjectArgParser(string[] args)
    {
        _parseResult = new ArgParse {
            new OptionSet("Analysis command") {
                new Option("-pid", "--input", "Specify the input folder path", true, true),
                new Option("-dll", "--output", "Specify the output folder path", true, true),
            },
            new OptionSet("Help command") {
                new Option("-h", "--help", "Show help options", true, false),
            }
        };
        _parseResult.Parse(args);
        _activeOptionSet = _parseResult.GetActiveOptionSet();
    }


    public string GetOption(string name)
    {
        return _activeOptionSet.GetOption(name).ParamValue;
    }
    
}