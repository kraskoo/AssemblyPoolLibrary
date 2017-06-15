namespace Library.Extensions
{
    using System.Linq;

    internal static class StringExtension
    {
        public static string[] GetSplittedPascalCaseWords(this string word)
        {
            var uppercaseIndices = word
                .Select(
                    (c, i) => new
                    {
                        @char = c,
                        index = i
                    })
                .Where(t => char.IsUpper(t.@char))
                .Select(t => t.index)
                .ToArray();
            if (uppercaseIndices[0] != 0)
            {
                uppercaseIndices = new[] { 0 }
                    .Concat(uppercaseIndices).ToArray();
            }

            if (uppercaseIndices.Length < 2)
            {
                return new[] { word };
            }

            int upperCasesCount = uppercaseIndices.Length;
            string[] words = new string[upperCasesCount];
            for (int i = 1; i <= upperCasesCount; i++)
            {
                var firstIndex = uppercaseIndices[i - 1];
                var secondIndex = i == upperCasesCount ? word.Length : uppercaseIndices[i];
                words[i - 1] = word.Substring(firstIndex, secondIndex - firstIndex);
            }

            return words;
        }
    }
}