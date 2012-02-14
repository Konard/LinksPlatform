using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

namespace NetLibrary
{
	public enum NetMapping
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
		static public Link Link;
		static public Link Thing;
		static public Link IsA;
		static public Link IsNotA;

		static public Link Of;
		static public Link And;
		static public Link ThatConsistsOf;
		static public Link Has;
		static public Link Contains;
		static public Link ContainedBy;

		static public Link One;
		static public Link Zero;

		static public Link Sum;
		static public Link Character;
		static public Link String;
		static public Link Name;

		static public Link Set;
		static public Link Group;

		static public Link ParsedFrom;
		static public Link ThatIs;
		static public Link ThatIsBefore;
		static public Link ThatIsBetween;
		static public Link ThatIsAfter;
		static public Link ThatIsRepresentedBy;
		static public Link ThatHas;

		static public Link Text;
		static public Link Path;
		static public Link Content;
		static public Link EmptyContent;
		static public Link Empty;
		static public Link Alphabet;
		static public Link Letter;
		static public Link Case;
		static public Link Upper;
		static public Link UpperCase;
		static public Link Lower;
		static public Link LowerCase;
		static public Link Code;

		static Net()
		{
			Create();
		}

		static public Link CreateThing()
		{
			return Link.Create(Link.Itself, Net.IsA, Net.Thing);
		}

		static public Link CreateMappedThing(object mapping)
		{
			return Link.CreateMapped(Link.Itself, Net.IsA, Net.Thing, mapping);
		}

		static public Link CreateLink()
		{
			return Link.Create(Link.Itself, Net.IsA, Net.Link);
		}

		static public Link CreateMappedLink(object mapping)
		{
			return Link.CreateMapped(Link.Itself, Net.IsA, Net.Link, mapping);
		}

		static public Link CreateSet()
		{
			return Link.Create(Link.Itself, Net.IsA, Net.Set);
		}

		static private void Create()
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

		static public void Recreate()
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
