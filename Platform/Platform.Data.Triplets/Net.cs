using Platform.Threading;

namespace Platform.Data.Triplets
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

    public class Net
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

        static Net() => Create();

        public static Link CreateThing() => Link.Create(Link.Itself, IsA, Thing);

        public static Link CreateMappedThing(object mapping) => Link.CreateMapped(Link.Itself, IsA, Thing, mapping);

        public static Link CreateLink() => Link.Create(Link.Itself, IsA, Link);

        public static Link CreateMappedLink(object mapping) => Link.CreateMapped(Link.Itself, IsA, Link, mapping);

        public static Link CreateSet() => Link.Create(Link.Itself, IsA, Set);

        private static void Create()
        {
            #region Core

            if (!Link.TryGetMapped(NetMapping.IsA, out IsA)
             || !Link.TryGetMapped(NetMapping.IsNotA, out IsNotA)
             || !Link.TryGetMapped(NetMapping.Link, out Link)
             || !Link.TryGetMapped(NetMapping.Thing, out Thing))
            {
                // Наивная инициализация (Не является корректным объяснением).
                IsA = Link.CreateMapped(Link.Itself, Link.Itself, Link.Itself, NetMapping.IsA); // Стоит переделать в "[x] is a member|instance|element of the class [y]"
                IsNotA = Link.CreateMapped(Link.Itself, Link.Itself, IsA, NetMapping.IsNotA);
                Link = Link.CreateMapped(Link.Itself, IsA, Link.Itself, NetMapping.Link);
                Thing = Link.CreateMapped(Link.Itself, IsNotA, Link, NetMapping.Thing);

                Link.Update(ref IsA, IsA, IsA, Link); // Исключение, позволяющие завершить систему
            }

            #endregion

            Of = CreateMappedLink(NetMapping.Of);
            And = CreateMappedLink(NetMapping.And);
            ThatConsistsOf = CreateMappedLink(NetMapping.ThatConsistsOf);
            Has = CreateMappedLink(NetMapping.Has);
            Contains = CreateMappedLink(NetMapping.Contains);
            ContainedBy = CreateMappedLink(NetMapping.ContainedBy);

            One = CreateMappedThing(NetMapping.One);
            Zero = CreateMappedThing(NetMapping.Zero);

            Sum = CreateMappedThing(NetMapping.Sum);
            Character = CreateMappedThing(NetMapping.Character);
            String = CreateMappedThing(NetMapping.String);
            Name = Link.CreateMapped(Link.Itself, IsA, String, NetMapping.Name);

            Set = CreateMappedThing(NetMapping.Set);
            Group = CreateMappedThing(NetMapping.Group);

            ParsedFrom = CreateMappedLink(NetMapping.ParsedFrom);
            ThatIs = CreateMappedLink(NetMapping.ThatIs);
            ThatIsBefore = CreateMappedLink(NetMapping.ThatIsBefore);
            ThatIsAfter = CreateMappedLink(NetMapping.ThatIsAfter);
            ThatIsBetween = CreateMappedLink(NetMapping.ThatIsBetween);
            ThatIsRepresentedBy = CreateMappedLink(NetMapping.ThatIsRepresentedBy);
            ThatHas = CreateMappedLink(NetMapping.ThatHas);

            Text = CreateMappedThing(NetMapping.Text);
            Path = CreateMappedThing(NetMapping.Path);
            Content = CreateMappedThing(NetMapping.Content);
            Empty = CreateMappedThing(NetMapping.Empty);
            EmptyContent = Link.CreateMapped(Content, ThatIs, Empty, NetMapping.EmptyContent);
            Alphabet = CreateMappedThing(NetMapping.Alphabet);
            Letter = Link.CreateMapped(Link.Itself, IsA, Character, NetMapping.Letter);
            Case = CreateMappedThing(NetMapping.Case);
            Upper = CreateMappedThing(NetMapping.Upper);
            UpperCase = Link.CreateMapped(Case, ThatIs, Upper, NetMapping.UpperCase);
            Lower = CreateMappedThing(NetMapping.Lower);
            LowerCase = Link.CreateMapped(Case, ThatIs, Lower, NetMapping.LowerCase);
            Code = CreateMappedThing(NetMapping.Code);

            SetNames();
        }

        public static void Recreate()
        {
            ThreadHelpers.SyncInvokeWithExtendedStack(() => Link.Delete(ref IsA));
            CharacterHelpers.Recreate();
            Create();
        }

        private static void SetNames()
        {
            Thing.SetName("thing");
            Link.SetName("link");
            IsA.SetName("is a");
            IsNotA.SetName("is not a");

            Of.SetName("of");
            And.SetName("and");
            ThatConsistsOf.SetName("that consists of");
            Has.SetName("has");
            Contains.SetName("contains");
            ContainedBy.SetName("contained by");

            One.SetName("one");
            Zero.SetName("zero");

            Character.SetName("character");
            Sum.SetName("sum");
            String.SetName("string");
            Name.SetName("name");

            Set.SetName("set");
            Group.SetName("group");

            ParsedFrom.SetName("parsed from");
            ThatIs.SetName("that is");
            ThatIsBefore.SetName("that is before");
            ThatIsAfter.SetName("that is after");
            ThatIsBetween.SetName("that is between");
            ThatIsRepresentedBy.SetName("that is represented by");
            ThatHas.SetName("that has");

            Text.SetName("text");
            Path.SetName("path");
            Content.SetName("content");
            Empty.SetName("empty");
            EmptyContent.SetName("empty content");
            Alphabet.SetName("alphabet");
            Letter.SetName("letter");
            Case.SetName("case");
            Upper.SetName("upper");
            Lower.SetName("lower");
            Code.SetName("code");
        }
    }
}
