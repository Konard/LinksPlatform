using System;
using System.Collections.Generic;

namespace Platform.Data.Triplets
{
    // TODO: Split logic of Latin and Cyrillic alphabets into different files if possible
    public static class CharacterHelpers
    {
        public enum CharacterMapping : long
        {
            LatinAlphabet = 100,
            CyrillicAlphabet
        }

        private const char FirstLowerСaseLatinLetter = 'a';
        private const char LastLowerСaseLatinLetter = 'z';
        private const char FirstUpperСaseLatinLetter = 'A';
        private const char LastUpperСaseLatinLetter = 'Z';
        private const char FirstLowerCaseCyrillicLetter = 'а';
        private const char LastLowerCaseCyrillicLetter = 'я';
        private const char FirstUpperCaseCyrillicLetter = 'А';
        private const char LastUpperCaseCyrillicLetter = 'Я';
        private const char YoLowerCaseCyrillicLetter = 'ё';
        private const char YoUpperCaseCyrillicLetter = 'Ё';

        private static Link[] _charactersToLinks;
        private static Dictionary<Link, char> _linksToCharacters;

        static CharacterHelpers() => Create();

        private static void Create()
        {
            _charactersToLinks = new Link[char.MaxValue];
            _linksToCharacters = new Dictionary<Link, char>();
            // Create or restore characters
            CreateLatinAlphabet();
            CreateCyrillicAlphabet();
            RegisterExistingCharacters();
        }

        private static void RegisterExistingCharacters() => Net.Character.WalkThroughReferersAsSource(referer => RegisterExistingCharacter(referer));

        private static void RegisterExistingCharacter(Link character)
        {
            if (character.Source == Net.Character && character.Linker == Net.ThatHas)
            {
                var code = character.Target;
                if (code.Source == Net.Code && code.Linker == Net.ThatIsRepresentedBy)
                {
                    var charCode = (char)LinkConverter.ToNumber(code.Target);
                    _charactersToLinks[charCode] = character;
                    _linksToCharacters[character] = charCode;
                }
            }
        }

        public static void Recreate() => Create();

        private static void CreateLatinAlphabet()
        {
            var lettersCharacters = new[]
            {
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j',
                'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't',
                'u', 'v', 'w', 'x', 'y', 'z'
            };
            CreateAlphabet(lettersCharacters, "latin alphabet", CharacterMapping.LatinAlphabet);
        }

        private static void CreateCyrillicAlphabet()
        {
            var lettersCharacters = new[]
            {
                'а', 'б', 'в', 'г', 'д', 'е', 'ё', 'ж', 'з', 'и',
                'й', 'к', 'л', 'м', 'н', 'о', 'п', 'р', 'с', 'т',
                'у', 'ф', 'х', 'ц', 'ч', 'ш', 'щ', 'ъ', 'ы', 'ь',
                'э', 'ю', 'я'
            };
            CreateAlphabet(lettersCharacters, "cyrillic alphabet", CharacterMapping.CyrillicAlphabet);
        }

        private static void CreateAlphabet(char[] lettersCharacters, string alphabetName, CharacterMapping mapping)
        {
            if (Link.TryGetMapped(mapping, out Link alphabet))
            {
                var letters = alphabet.Target;
                letters.WalkThroughSequence(letter =>
                {
                    var lowerCaseLetter = Link.Search(Net.LowerCase, Net.Of, letter);
                    var upperCaseLetter = Link.Search(Net.UpperCase, Net.Of, letter);
                    if (lowerCaseLetter != null && upperCaseLetter != null)
                    {
                        RegisterExistingLetter(lowerCaseLetter);
                        RegisterExistingLetter(upperCaseLetter);
                    }
                    else
                    {
                        RegisterExistingLetter(letter);
                    }
                });
            }
            else
            {
                alphabet = Net.CreateMappedThing(mapping);
                var letterOfAlphabet = Link.Create(Net.Letter, Net.Of, alphabet);
                var lettersLinks = new Link[lettersCharacters.Length];
                GenerateAlphabetBasis(ref alphabet, ref letterOfAlphabet, lettersLinks);
                for (var i = 0; i < lettersCharacters.Length; i++)
                {
                    var lowerCaseCharacter = lettersCharacters[i];
                    SetLetterCodes(lettersLinks[i], lowerCaseCharacter, out Link lowerCaseLink, out Link upperCaseLink);
                    _charactersToLinks[lowerCaseCharacter] = lowerCaseLink;
                    _linksToCharacters[lowerCaseLink] = lowerCaseCharacter;
                    if (upperCaseLink != null)
                    {
                        var upperCaseCharacter = char.ToUpper(lowerCaseCharacter);
                        _charactersToLinks[upperCaseCharacter] = upperCaseLink;
                        _linksToCharacters[upperCaseLink] = upperCaseCharacter;
                    }
                }
                alphabet.SetName(alphabetName);
                for (var i = 0; i < lettersCharacters.Length; i++)
                {
                    var lowerCaseCharacter = lettersCharacters[i];
                    var upperCaseCharacter = char.ToUpper(lowerCaseCharacter);
                    if (lowerCaseCharacter != upperCaseCharacter)
                    {
                        lettersLinks[i].SetName("{" + upperCaseCharacter + " " + lowerCaseCharacter + "}");
                    }
                    else
                    {
                        lettersLinks[i].SetName("{" + lowerCaseCharacter + "}");
                    }
                }
            }
        }

        private static void RegisterExistingLetter(Link letter)
        {
            letter.WalkThroughReferersAsSource(referer =>
                {
                    if (referer.Linker == Net.Has)
                    {
                        var target = referer.Target;
                        if (target.Source == Net.Code && target.Linker == Net.ThatIsRepresentedBy)
                        {
                            var charCode = (char)LinkConverter.ToNumber(target.Target);
                            _charactersToLinks[charCode] = letter;
                            _linksToCharacters[letter] = charCode;
                        }
                    }
                });
        }

        private static void GenerateAlphabetBasis(ref Link alphabet, ref Link letterOfAlphabet, Link[] letters)
        {
            // Принцип, на примере латинского алфавита.
            //latin alphabet: alphabet that consists of a and b and c and ... and z.
            //a: letter of latin alphabet that is before b.
            //b: letter of latin alphabet that is between (a and c).
            //c: letter of latin alphabet that is between (b and e).
            //...
            //y: letter of latin alphabet that is between (x and z).
            //z: letter of latin alphabet that is after y.
            const int firstLetterIndex = 0;
            for (var i = firstLetterIndex; i < letters.Length; i++)
            {
                letters[i] = Net.CreateThing();
            }
            var lastLetterIndex = letters.Length - 1;
            Link.Update(ref letters[firstLetterIndex], letterOfAlphabet, Net.ThatIsBefore, letters[firstLetterIndex + 1]);
            Link.Update(ref letters[lastLetterIndex], letterOfAlphabet, Net.ThatIsAfter, letters[lastLetterIndex - 1]);
            const int secondLetterIndex = firstLetterIndex + 1;
            for (var i = secondLetterIndex; i < lastLetterIndex; i++)
            {
                Link.Update(ref letters[i], letterOfAlphabet, Net.ThatIsBetween, letters[i - 1] & letters[i + 1]);
            }
            Link.Update(ref alphabet, Net.Alphabet, Net.ThatConsistsOf, LinkConverter.FromList(letters));
        }

        private static void SetLetterCodes(Link letter, char lowerCaseCharacter, out Link lowerCase, out Link upperCase)
        {
            var upperCaseCharacter = char.ToUpper(lowerCaseCharacter);
            if (upperCaseCharacter != lowerCaseCharacter)
            {
                lowerCase = Link.Create(Net.LowerCase, Net.Of, letter);
                var lowerCaseCharacterCode = Link.Create(Net.Code, Net.ThatIsRepresentedBy, LinkConverter.FromNumber(lowerCaseCharacter));
                Link.Create(lowerCase, Net.Has, lowerCaseCharacterCode);
                upperCase = Link.Create(Net.UpperCase, Net.Of, letter);
                var upperCaseCharacterCode = Link.Create(Net.Code, Net.ThatIsRepresentedBy, LinkConverter.FromNumber(upperCaseCharacter));
                Link.Create(upperCase, Net.Has, upperCaseCharacterCode);
            }
            else
            {
                lowerCase = letter;
                upperCase = null;
                Link.Create(letter, Net.Has, Link.Create(Net.Code, Net.ThatIsRepresentedBy, LinkConverter.FromNumber(lowerCaseCharacter)));
            }
        }

        private static Link CreateSimpleCharacterLink(char character) => Link.Create(Net.Character, Net.ThatHas, Link.Create(Net.Code, Net.ThatIsRepresentedBy, LinkConverter.FromNumber(character)));

        private static bool IsLetterOfLatinAlphabet(char character)
            => character >= FirstLowerСaseLatinLetter && character <= LastLowerСaseLatinLetter
            || character >= FirstUpperСaseLatinLetter && character <= LastUpperСaseLatinLetter;

        private static bool IsLetterOfCyrillicAlphabet(char character)
            => character >= FirstLowerCaseCyrillicLetter && character <= LastLowerCaseCyrillicLetter
            || character >= FirstUpperCaseCyrillicLetter && character <= LastUpperCaseCyrillicLetter
            || character == YoLowerCaseCyrillicLetter || character == YoUpperCaseCyrillicLetter;

        public static Link FromChar(char character)
        {
            if (_charactersToLinks[character] == null)
            {
                if (IsLetterOfLatinAlphabet(character))
                {
                    CreateLatinAlphabet();
                    return _charactersToLinks[character];
                }
                else if (IsLetterOfCyrillicAlphabet(character))
                {
                    CreateCyrillicAlphabet();
                    return _charactersToLinks[character];
                }
                else
                {
                    var simpleCharacter = CreateSimpleCharacterLink(character);
                    _charactersToLinks[character] = simpleCharacter;
                    _linksToCharacters[simpleCharacter] = character;
                    return simpleCharacter;
                }
            }
            else
            {
                return _charactersToLinks[character];
            }
        }

        public static char ToChar(Link link)
        {
            if (!_linksToCharacters.TryGetValue(link, out char @char))
            {
                throw new ArgumentOutOfRangeException(nameof(link), "Указанная связь не являяется символом.");
            }
            return @char;
        }

        public static bool IsChar(Link link) => link != null && _linksToCharacters.ContainsKey(link);
    }
}
