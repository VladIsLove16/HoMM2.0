using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Adventure.Domain.Dialog
{
    public sealed class DialogueTextResolver : IDialogueTextResolver
    {
        private static readonly Regex TokenRegex = new(@"\{(?<key>[A-Za-z0-9_]+)\}|\<(?<key>[A-Za-z0-9_]+)\>",
            RegexOptions.Compiled);

        private readonly IReadOnlyDictionary<string, string> _tokens;

        public DialogueTextResolver(IReadOnlyDictionary<string, string> tokens)
        {
            _tokens = tokens ?? new Dictionary<string, string>();
        }

        public string Resolve(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text ?? string.Empty;

            return TokenRegex.Replace(text, match =>
            {
                var key = match.Groups["key"].Value;
                if (string.IsNullOrEmpty(key))
                    return match.Value;
                return _tokens.TryGetValue(key, out var value) ? value ?? string.Empty : match.Value;
            });
        }
    }
}
