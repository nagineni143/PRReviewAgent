using System.Text.RegularExpressions;

public class DiffService
{
    public List<CodeChange> ExtractChanges(List<PullRequestFile> files)
    {
        var changes = new List<CodeChange>();

        foreach (var file in files)
        {
            if (string.IsNullOrEmpty(file.Patch))
                continue;

            var lines = file.Patch.Split('\n');

            int currentLine = 0;

            foreach (var line in lines)
            {
                if (line.StartsWith("@@"))
                {
                    var match = Regex.Match(line, @"\+(\d+)");
                    if (match.Success)
                        currentLine = int.Parse(match.Groups[1].Value);

                    continue;
                }

                if (line.StartsWith("+") && !line.StartsWith("+++"))
                {
                    var change = new CodeChange
                    {
                        File = file.Filename,
                        LineNumber = currentLine,
                        Code = line.Substring(1).Trim()
                    };

                    if (change.Code.Contains(".Result"))
                        change.RuleHints.Add("ASYNC002");

                    if (change.Code.Contains(".Wait("))
                        change.RuleHints.Add("ASYNC002");

                    if (change.Code.Contains("async void"))
                        change.RuleHints.Add("ASYNC001");

                    changes.Add(change);


                    currentLine++;
                }
                else if (!line.StartsWith("-"))
                {
                    currentLine++;
                }
            }
        }

        return changes;
    }
}