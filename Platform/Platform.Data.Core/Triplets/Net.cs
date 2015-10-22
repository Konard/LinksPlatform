using Platform.Helpers;

namespace Platform.Data.Core.Triplets
{
    public enum NetMapping : long
    {
        Link,
        Thing,
        IsA,
        IsNotA,

        Of,
        And,
        ThatConsistsOf,
        Has,
        Contains,
        ContainedBy,

        One,
        Zero,

        Sum,
        Character,
        String,
        Name,

        Set,
        Group,

        ParsedFrom,
        ThatIs,
        ThatIsBefore,
        ThatIsBetween,
        ThatIsAfter,
        ThatIsRepresentedBy,
        ThatHas,

        Text,
        Path,
        Content,
        EmptyContent,
        Empty,
        Alphabet,
        Letter,
        Case,
        Upper,
        UpperCase,
        Lower,
        LowerCase,
        Code
    }

    public partial class Net
    {
        public static Link Link;
        public static Link Thing;
        public static Link IsA;
        public static Link IsNotA;

        public static Link Of;
        public static Link And;
        public static Link ThatConsistsOf;
        public static Link Has;
        public static Link Contains;
        public static Link ContainedBy;

        public static Link One;
        public static Link Zero;

        public static Link Sum;
        public static Link Character;
        public static Link String;
        public static Link Name;

        public static Link Set;
        public static Link Group;

        public static Link ParsedFrom;
        public static Link ThatIs;
        public static Link ThatIsBefore;
        public static Link ThatIsBetween;
        public static Link ThatIsAfter;
        public static Link ThatIsRepresentedBy;
        public static Link ThatHas;

        public static Link Text;
        public static Link Path;
        public static Link Content;
        public static Link EmptyContent;
        public static Link Empty;
        public static Link Alphabet;
        public static Link Letter;
        public static Link Case;
        public static Link Upper;
        public static Link UpperCase;
        public static Link Lower;
        public static Link LowerCase;
        public static Link Code;

        static Net()
        {
            Create();
        }

        public static Link CreateThing()
        {
            return Link.Create(Link.Itself, Net.IsA, Net.Thing);
        }

        public static Link CreateMappedThing(object mapping)
        {
            return Link.CreateMapped(Link.Itself, Net.IsA, Net.Thing, mapping);
        }

        public static Link CreateLink()
        {
            return Link.Create(Link.Itself, Net.IsA, Net.Link);
        }

        public static Link CreateMappedLink(object mapping)
        {
            return Link.CreateMapped(Link.Itself, Net.IsA, Net.Link, mapping);
        }

        public static Link CreateSet()
        {
            return Link.Create(Link.Itself, Net.IsA, Net.Set);
        }

        private static void Create()
        {
            #region Core

            if (!Link.TryGetMapped(NetMapping.IsA, out Net.IsA)
             || !Link.TryGetMapped(NetMapping.IsNotA, out Net.IsNotA)
             || !Link.TryGetMapped(NetMapping.Link, out Net.Link)
             || !Link.TryGetMapped(NetMapping.Thing, out Net.Thing))
            {
                // Наивная инициализация (Не является корректным объяснением).
                Net.IsA = Link.CreateMapped(Link.Itself, Link.Itself, Link.Itself, NetMapping.IsA); // Стоит переделать в "[x] is a member|instance|element of the class [y]"
                Net.IsNotA = Link.CreateMapped(Link.Itself, Link.Itself, Net.IsA, NetMapping.IsNotA);
                Net.Link = Link.CreateMapped(Link.Itself, Net.IsA, Link.Itself, NetMapping.Link);
                Net.Thing = Link.CreateMapped(Link.Itself, Net.IsNotA, Net.Link, NetMapping.Thing);

                Link.Update(ref Net.IsA, Net.IsA, Net.IsA, Net.Link); // Исключение, позволяющие завершить систему
            }

            #endregion

            Net.Of = CreateMappedLink(NetMapping.Of);
            Net.And = CreateMappedLink(NetMapping.And);
            Net.ThatConsistsOf = CreateMappedLink(NetMapping.ThatConsistsOf);
            Net.Has = CreateMappedLink(NetMapping.Has);
            Net.Contains = CreateMappedLink(NetMapping.Contains);
            Net.ContainedBy = CreateMappedLink(NetMapping.ContainedBy);

            Net.One = CreateMappedThing(NetMapping.One);
            Net.Zero = CreateMappedThing(NetMapping.Zero);

            Net.Sum = CreateMappedThing(NetMapping.Sum);
            Net.Character = CreateMappedThing(NetMapping.Character);
            Net.String = CreateMappedThing(NetMapping.String);
            Net.Name = Link.CreateMapped(Link.Itself, Net.IsA, Net.String, NetMapping.Name);

            Net.Set = CreateMappedThing(NetMapping.Set);
            Net.Group = CreateMappedThing(NetMapping.Group);

            Net.ParsedFrom = CreateMappedLink(NetMapping.ParsedFrom);
            Net.ThatIs = CreateMappedLink(NetMapping.ThatIs);
            Net.ThatIsBefore = CreateMappedLink(NetMapping.ThatIsBefore);
            Net.ThatIsAfter = CreateMappedLink(NetMapping.ThatIsAfter);
            Net.ThatIsBetween = CreateMappedLink(NetMapping.ThatIsBetween);
            Net.ThatIsRepresentedBy = CreateMappedLink(NetMapping.ThatIsRepresentedBy);
            Net.ThatHas = CreateMappedLink(NetMapping.ThatHas);

            Net.Text = CreateMappedThing(NetMapping.Text);
            Net.Path = CreateMappedThing(NetMapping.Path);
            Net.Content = CreateMappedThing(NetMapping.Content);
            Net.Empty = CreateMappedThing(NetMapping.Empty);
            Net.EmptyContent = Link.CreateMapped(Net.Content, Net.ThatIs, Net.Empty, NetMapping.EmptyContent);
            Net.Alphabet = CreateMappedThing(NetMapping.Alphabet);
            Net.Letter = Link.CreateMapped(Link.Itself, Net.IsA, Net.Character, NetMapping.Letter);
            Net.Case = CreateMappedThing(NetMapping.Case);
            Net.Upper = CreateMappedThing(NetMapping.Upper);
            Net.UpperCase = Link.CreateMapped(Net.Case, Net.ThatIs, Net.Upper, NetMapping.UpperCase);
            Net.Lower = CreateMappedThing(NetMapping.Lower);
            Net.LowerCase = Link.CreateMapped(Net.Case, Net.ThatIs, Net.Lower, NetMapping.LowerCase);
            Net.Code = CreateMappedThing(NetMapping.Code);

            SetNames();
        }

        public static void Recreate()
        {
            ThreadHelpers.SyncInvokeWithExtendedStack(() => Link.Delete(ref Net.IsA));
            CharacterHelpers.Recreate();
            Create();
        }

        private static void SetNames()
        {
            Net.Thing.SetName("thing");
            Net.Link.SetName("link");
            Net.IsA.SetName("is a");
            Net.IsNotA.SetName("is not a");

            Net.Of.SetName("of");
            Net.And.SetName("and");
            Net.ThatConsistsOf.SetName("that consists of");
            Net.Has.SetName("has");
            Net.Contains.SetName("contains");
            Net.ContainedBy.SetName("contained by");

            Net.One.SetName("one");
            Net.Zero.SetName("zero");

            Net.Character.SetName("character");
            Net.Sum.SetName("sum");
            Net.String.SetName("string");
            Net.Name.SetName("name");

            Net.Set.SetName("set");
            Net.Group.SetName("group");

            Net.ParsedFrom.SetName("parsed from");
            Net.ThatIs.SetName("that is");
            Net.ThatIsBefore.SetName("that is before");
            Net.ThatIsAfter.SetName("that is after");
            Net.ThatIsBetween.SetName("that is between");
            Net.ThatIsRepresentedBy.SetName("that is represented by");
            Net.ThatHas.SetName("that has");

            Net.Text.SetName("text");
            Net.Path.SetName("path");
            Net.Content.SetName("content");
            Net.Empty.SetName("empty");
            Net.EmptyContent.SetName("empty content");
            Net.Alphabet.SetName("alphabet");
            Net.Letter.SetName("letter");
            Net.Case.SetName("case");
            Net.Upper.SetName("upper");
            Net.Lower.SetName("lower");
            Net.Code.SetName("code");
        }
    }
}
