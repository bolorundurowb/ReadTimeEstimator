using System;
using System.Text.RegularExpressions;

namespace ReadTimeEstimator
{
    public class Utilities
    {
        public string StripWhitespace(string input)
        {
            return input.Trim();
        }

        public int ImageCount(string[] imageTags, string input)
        {
            var combinedImageTags = string.Join("|", imageTags);
            var pattern = $"<({combinedImageTags})([\\w\\W]+?)[\\/]?>";
            var regex = new Regex(pattern, RegexOptions.Multiline);
            var matches = regex.Match(input);
            return matches.Length;
        }

        public (double, int) ImageReadTime( string input, int customImageTime = Constants.ImageReadTimeInSeconds)
        {
            var seconds = 0.0;
            var imageCount = ImageCount(new[] {"img", "Image"}, input);

            if (imageCount > 10)
            {
                seconds = ((imageCount / 2.0) * (customImageTime * 3)) + (imageCount - 10) * 3;
            }
            else
            {
                seconds = (imageCount / 2.0) * (2 * customImageTime + (1 - imageCount));
            }

            return (seconds / 60.0, imageCount);
        }

        public string StripTags(string input)
        {
            var pattern = "<\\w+(\\s+(\"[^\"]*\"|\\\'[^\\\']*\'|[^>])+)?>|<\\/\\w+>";
            var regex = new Regex(pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            return regex.Replace(input, "");
        }

        public int WordCount(string input)
        {
            var pattern = "\\w+";
            var regex = new Regex(pattern, RegexOptions.Multiline);
            var matches = regex.Match(input);
            return matches.Length;
        }

        public (int, double, string) OtherLanguagesReadTime(string input)
        {
            var pattern = "[\u3040-\u30ff\u3400-\u4dbf\u4e00-\u9fff\uf900-\ufaff\uff66-\uff9f]";
            var regex = new Regex(pattern, RegexOptions.Multiline);
            var matches = regex.Match(input);
            var count = matches.Length;
            var time = count / (double) Constants.EastAsianCharactersPerMinute;
            var formattedString = regex.Replace(input, "");
            return (count, time, formattedString);
        }

        public (int, double, double, int) WordsReadTime(string input, int wordsPerMin = Constants.WordsPerMinute)
        {
            var (characterCount, otherLanguageTime, formattedString) = OtherLanguagesReadTime(input);
            var wordCount = WordCount(formattedString);
            var wordTime = wordCount / (double) wordsPerMin;
            return (characterCount, otherLanguageTime, wordTime, wordCount);
        }

        public string HumanizeTime(double time)
        {
            if (time < 0.5)
            {
                return "less than a minute";
            }

            if (time >= 0.5 && time < 1.5)
            {
                return "1 minute";
            }

            return $"{Math.Ceiling(time)} minutes";
        }

        public string ReadTime(string input)
        {
            var (imageTime, imageCount) = ImageReadTime(input);
            var strippedString = StripTags(StripWhitespace(input));
            var (characterCount, otherLanguageTime, wordTime, wordCount) = WordsReadTime(strippedString);
            return HumanizeTime(imageTime + wordTime);
        }
    }
}
