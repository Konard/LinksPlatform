using System;
using System.Collections.Generic;
using Platform.Disposables;
using Platform.Collections;
using Platform.Collections.Segments;
using Platform.Collections.Segments.Walkers;
using Platform.Collections.Trees;
using Platform.Memory;
using Platform.Data.Doublets;
using Platform.Data.Doublets.ResizableDirectMemory;
using TLink = System.UInt32;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Numbers.Unary;
using Platform.Data.Doublets.PropertyOperators;

namespace Platform.Sandbox
{
    internal class AllRepeatingSubstringsInString
    {
        private static readonly string _exampleText =
    @"([english version](https://github.com/Konard/LinksPlatform/wiki/About-the-beginning))

Обозначение пустоты, какое оно? Темнота ли это? Там где отсутствие света, отсутствие фотонов (носителей света)? Или это то, что полностью отражает свет? Пустой белый лист бумаги? Там где есть место для нового начала? Разве пустота это не характеристика пространства? Пространство это то, что можно чем-то наполнить?

[![чёрное пространство, белое пространство](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/1.png ""чёрное пространство, белое пространство"")](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/1.png)

Что может быть минимальным рисунком, образом, графикой? Может быть это точка? Это ли простейшая форма? Но есть ли у точки размер? Цвет? Масса? Координаты? Время существования?

[![чёрное пространство, чёрная точка](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/2.png ""чёрное пространство, чёрная точка"")](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/2.png)

А что если повторить? Сделать копию? Создать дубликат? Из одного сделать два? Может это быть так? Инверсия? Отражение? Сумма?

[![белая точка, чёрная точка](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/3.png ""белая точка, чёрная точка"")](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/3.png)

А что если мы вообразим движение? Нужно ли время? Каким самым коротким будет путь? Что будет если этот путь зафиксировать? Запомнить след? Как две точки становятся линией? Чертой? Гранью? Разделителем? Единицей?

[![две белые точки, чёрная вертикальная линия](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/4.png ""две белые точки, чёрная вертикальная линия"")](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/4.png)

Можно ли замкнуть движение? Может ли это быть кругом? Можно ли замкнуть время? Или остаётся только спираль? Но что если замкнуть предел? Создать ограничение, разделение? Получится замкнутая область? Полностью отделённая от всего остального? Но что это всё остальное? Что можно делить? В каком направлении? Ничего или всё? Пустота или полнота? Начало или конец? Или может быть это единица и ноль? Дуальность? Противоположность? А что будет с кругом если у него нет размера? Будет ли круг точкой? Точка состоящая из точек?

[![белая вертикальная линия, чёрный круг](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/5.png ""белая вертикальная линия, чёрный круг"")](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/5.png)

Как ещё можно использовать грань, черту, линию? А что если она может что-то соединять, может тогда её нужно повернуть? Почему то, что перпендикулярно вертикальному горизонтально? Горизонт? Инвертирует ли это смысл? Что такое смысл? Из чего состоит смысл? Существует ли элементарная единица смысла?

[![белый круг, чёрная горизонтальная линия](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/6.png ""белый круг, чёрная горизонтальная линия"")](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/6.png)

Соединять, допустим, а какой смысл в этом есть ещё? Что если помимо смысла ""соединить, связать"", есть ещё и смысл направления ""от начала к концу""? От предка к потомку? От родителя к ребёнку? От общего к частному?

[![белая горизонтальная линия, чёрная горизонтальная стрелка](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/7.png ""белая горизонтальная линия, чёрная горизонтальная стрелка"")](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/7.png)

Шаг назад. Возьмём опять отделённую область, которая лишь та же замкнутая линия, что ещё она может представлять собой? Объект? Но в чём его суть? Разве не в том, что у него есть граница, разделяющая внутреннее и внешнее? Допустим связь, стрелка, линия соединяет два объекта, как бы это выглядело?

[![белая связь, чёрная направленная связь](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/8.png ""белая связь, чёрная направленная связь"")](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/8.png)

Допустим у нас есть смысл ""связать"" и смысл ""направления"", много ли это нам даёт? Много ли вариантов интерпретации? А что если уточнить, каким именно образом выполнена связь? Что если можно задать ей чёткий, конкретный смысл? Что это будет? Тип? Глагол? Связка? Действие? Трансформация? Переход из состояния в состояние? Или всё это и есть объект, суть которого в его конечном состоянии, если конечно конец определён направлением?

[![белая обычная и направленная связи, чёрная типизированная связь](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/9.png ""белая обычная и направленная связи, чёрная типизированная связь"")](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/9.png)

А что если всё это время, мы смотрели на суть как бы снаружи? Можно ли взглянуть на это изнутри? Что будет внутри объектов? Объекты ли это? Или это связи? Может ли эта структура описать сама себя? Но что тогда получится, разве это не рекурсия? Может это фрактал?

[![белая обычная и направленная связи с рекурсивной внутренней структурой, чёрная типизированная связь с рекурсивной внутренней структурой](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/10.png ""белая обычная и направленная связи с рекурсивной внутренней структурой, чёрная типизированная связь с рекурсивной внутренней структурой"")](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/10.png)

На один уровень внутрь (вниз)? Или на один уровень во вне (вверх)? Или это можно назвать шагом рекурсии или фрактала?

[![белая обычная и направленная связи с двойной рекурсивной внутренней структурой, чёрная типизированная связь с двойной рекурсивной внутренней структурой](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/11.png ""белая обычная и направленная связи с двойной рекурсивной внутренней структурой, чёрная типизированная связь с двойной рекурсивной внутренней структурой"")](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/11.png)

Последовательность? Массив? Список? Множество? Объект? Таблица? Элементы? Цвета? Символы? Буквы? Слово? Цифры? Число? Алфавит? Дерево? Сеть? Граф? Гиперграф?

[![белая обычная и направленная связи со структурой из 8 цветных элементов последовательности, чёрная типизированная связь со структурой из 8 цветных элементов последовательности](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/12.png ""белая обычная и направленная связи со структурой из 8 цветных элементов последовательности, чёрная типизированная связь со структурой из 8 цветных элементов последовательности"")](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/12.png)

...

[![анимация](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/intro-animation-500.gif ""анимация"")](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/intro-animation-500.gif)";


        private static readonly string _exampleLoremIpsumText = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

        public static void Run()
        {
            var text = _exampleLoremIpsumText;

            var iterationsCounter = new IterationsCounter();
            iterationsCounter.WalkAll(text);
            var result = iterationsCounter.IterationsCount;
            Console.WriteLine($"TextLength: {text.Length}. Iterations: {result}.");

            var walker = new Walker4();
            walker.WalkAll(text);
        }
    }

    public abstract class ConsolePrintedDublicateWalkerBase : DuplicateSegmentsWalkerBase<char, CharSegment>
    {
        protected override void OnDublicateFound(CharSegment segment) => Console.WriteLine(segment);

        protected override CharSegment CreateSegment(IList<char> elements, int offset, int length) => new CharSegment(elements, offset, length);
    }

    // Slow
    public class Walker1 : ConsolePrintedDublicateWalkerBase
    {
        private Node _rootNode;
        private Node _currentNode;

        public override void WalkAll(IList<char> elements)
        {
            _rootNode = new Node();

            base.WalkAll(elements);
        }

        protected override long GetSegmentFrequency(CharSegment segment)
        {
            for (int i = 0; i < segment.Length; i++)
            {
                var element = segment[i];

                _currentNode = _currentNode[element];
            }

            if (_currentNode.Value is int)
            {
                return (int)_currentNode.Value;
            }
            else
            {
                return 0;
            }
        }

        protected override void SetSegmentFrequency(CharSegment segment, long frequency) => _currentNode.Value = frequency;

        protected override void Iteration(CharSegment segment)
        {
            _currentNode = _rootNode;

            base.Iteration(segment);
        }
    }

    // Too much memory, but fast
    public class Walker2 : ConsolePrintedDublicateWalkerBase
    {
        private Dictionary<string, long> _cache;
        private string _currentKey;

        public override void WalkAll(IList<char> elements)
        {
            _cache = new Dictionary<string, long>();

            base.WalkAll(elements);
        }

        protected override long GetSegmentFrequency(CharSegment segment) => _cache.GetOrDefault(_currentKey);

        protected override void SetSegmentFrequency(CharSegment segment, long frequency) => _cache[_currentKey] = frequency;

        protected override void Iteration(CharSegment segment)
        {
            _currentKey = segment;

            base.Iteration(segment);
        }
    }

    // Super slow
    public class Walker3 : ConsolePrintedDublicateWalkerBase //-V3072
    {
        private HeapResizableDirectMemory _memory;
        private ILinks<TLink> _linksDisposable;
        private ILinks<TLink> _links;
        private TLink _meaningRoot;
        private TLink _frequencyProperty;
        private TLink _unaryOne;
        private TLink _treeRoot;
        private AddressToUnaryNumberConverter<TLink> _toNumberConverter;
        private PowerOf2ToUnaryNumberConverter<TLink> _powerOf2ToUnaryNumberConverter;
        private UnaryNumberToAddressAddOperationConverter<TLink> _fromNumberConverter;
        private PropertiesOperator<TLink> _propertyOperator;
        private TLink _currentLink;

        public override void WalkAll(IList<char> elements)
        {
            _memory = new HeapResizableDirectMemory(512 * 1024 * 1024);

            _linksDisposable = new ResizableDirectMemoryLinks<TLink>(_memory);

            _links = new LinksItselfConstantToSelfReferenceResolver<TLink>(_linksDisposable);
            _links = new NonNullContentsLinkDeletionResolver<TLink>(_links);

            _meaningRoot = _links.CreatePoint();
            _frequencyProperty = _links.CreateAndUpdate(_meaningRoot, _links.Constants.Itself);
            _unaryOne = _links.CreateAndUpdate(_meaningRoot, _links.Constants.Itself);
            _treeRoot = _links.CreateAndUpdate(_meaningRoot, _links.Constants.Itself);

            _fromNumberConverter = new UnaryNumberToAddressAddOperationConverter<TLink>(_links, _unaryOne);

            _powerOf2ToUnaryNumberConverter = new PowerOf2ToUnaryNumberConverter<TLink>(_links, _unaryOne);
            _toNumberConverter = new AddressToUnaryNumberConverter<TLink>(_links, _powerOf2ToUnaryNumberConverter);

            _propertyOperator = new PropertiesOperator<TLink>(_links);

            base.WalkAll(elements);

            _linksDisposable.DisposeIfPossible();
            _memory.DisposeIfPossible();
        }

        protected override long GetSegmentFrequency(CharSegment segment)
        {
            for (int i = 0; i < segment.Length; i++)
            {
                var element = segment[i];
                var link = _toNumberConverter.Convert(element);

                _currentLink = _links.GetOrCreate(_currentLink, link);
            }

            var frequency = _propertyOperator.GetValue(_currentLink, _frequencyProperty);

            if (frequency == _links.Constants.Null)
            {
                return 0;
            }
            else
            {
                return _fromNumberConverter.Convert(frequency);
            }
        }

        protected override void SetSegmentFrequency(CharSegment segment, long frequency) => _propertyOperator.SetValue(_currentLink, _frequencyProperty, _toNumberConverter.Convert((uint)frequency));

        protected override void Iteration(CharSegment segment)
        {
            _currentLink = _treeRoot;

            base.Iteration(segment);
        }
    }

    public class Walker4 : DictionaryBasedDuplicateSegmentsWalkerBase<char, CharSegment>
    {
        public Walker4()
            : base(DefaultMinimumStringSegmentLength, resetDictionaryOnEachWalk: true)
        {
        }

        private int _totalDuplicates;

        public override void WalkAll(IList<char> elements)
        {
            _totalDuplicates = 0;

            base.WalkAll(elements);

            Console.WriteLine($"Unique string segments: {Dictionary.Count}. Total duplicates: {_totalDuplicates}.");
        }

        protected override CharSegment CreateSegment(IList<char> elements, int offset, int length) => new CharSegment(elements, offset, length);

        protected override void OnDublicateFound(CharSegment segment)
        {
            Console.WriteLine(segment);
            _totalDuplicates++;
        }
    }

    public class IterationsCounter : AllSegmentsWalkerBase<char>
    {
        public long IterationsCount;

        protected override void Iteration(Segment<char> segment) => IterationsCount++;
    }
}
