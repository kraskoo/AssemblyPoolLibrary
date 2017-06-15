namespace Library
{
    using System;
    using System.Collections.Generic;
    using Extensions;

    internal class StringWorker
    {
        public string GetTheMostCommonWordsInString(string @string, params string[] stringCollection)
        {
            var wordsOfTypename = @string.GetSplittedPascalCaseWords();
            var searchedKey = string.Empty;
            var meetCount = 0;
            foreach (var stringPair in stringCollection)
            {
                int innerCounter = 0;
                var wordsOfCurrentString = new SortedSet<string>(
                    stringPair.GetSplittedPascalCaseWords());
                foreach (var typenameWord in wordsOfTypename)
                {
                    var foundWord = string.Empty;
                    foreach (var fieldnameWord in wordsOfCurrentString)
                    {
                        if (string.Compare(typenameWord, fieldnameWord, StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            foundWord = fieldnameWord;
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(foundWord))
                    {
                        wordsOfCurrentString.Remove(foundWord);
                        innerCounter++;
                    }
                }

                if (innerCounter > meetCount)
                {
                    meetCount = innerCounter;
                    searchedKey = stringPair;
                }
            }

            return searchedKey;
        }
    }
}