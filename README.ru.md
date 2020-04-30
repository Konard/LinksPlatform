[![Статус сборки](https://travis-ci.org/Konard/LinksPlatform.svg?branch=master "Статус сборки")](https://travis-ci.org/Konard/LinksPlatform)

# ПлатформаСвязей ([english version](https://github.com/Konard/LinksPlatform/blob/master/README.md))

Платформа Связей — это фреймворк, в который входят две реализации СУБД на основе ассоциативной модели данных: [Дуплеты](https://github.com/linksplatform/Data.Doublets) и [Треплеты](https://github.com/linksplatform/Data.Triplets); а также трансляторы между языками программирования (например между [из C# в C++](https://github.com/linksplatform/RegularExpressions.Transformer.CSharpToCpp)).

Большая часть библиотек входящих в этот фреймворк могут быть использованы отдельно при необходимости и располагаются [на странице организации "Платформа Связей"](). Для всех библиотек доступна реализация на C#, в ближайшем будущем они так же будут доступны и на С++ (в данный момент ведётся перевод).

[Документация](http://linksplatform.github.io/Documentation)

[![вступление](https://raw.githubusercontent.com/LinksPlatform/Documentation/master/doc/Intro/intro-animation-500.gif "вступление")](https://github.com/Konard/LinksPlatform/wiki/%D0%9E-%D1%82%D0%BE%D0%BC,-%D0%BA%D0%B0%D0%BA-%D0%B2%D1%81%D1%91-%D0%BD%D0%B0%D1%87%D0%B8%D0%BD%D0%B0%D0%BB%D0%BE%D1%81%D1%8C)

[Графическое вступление](https://github.com/Konard/LinksPlatform/wiki/%D0%9E-%D1%82%D0%BE%D0%BC,-%D0%BA%D0%B0%D0%BA-%D0%B2%D1%81%D1%91-%D0%BD%D0%B0%D1%87%D0%B8%D0%BD%D0%B0%D0%BB%D0%BE%D1%81%D1%8C)

## [Пример](https://github.com/linksplatform/HelloWorld.Doublets.DotNet)

```C#
using System;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace HelloWorld.Doublets.DotNet
{
    class Program
    {
        static void Main()
        {
            using (var links = new UnitedMemoryLinks<uint>("db.links"))
            {
                var link = links.Create();
                link = links.Update(link, link, link);
                Console.WriteLine("Привет Мир!");
                Console.WriteLine($"Это моя первая связь: {links.Format(link)}");
                Console.WriteLine($"Всего связей в хранилище: {links.Count()}.");
                link = links.Update(link, default, default);
                links.Delete(link);
            }
        }
    }
}
```

## [SQLite против Дуплетов](https://github.com/linksplatform/Comparisons.SQLiteVSDoublets)

[![Изображение с результатом сравнения производительности SQLite и Дуплетов.](https://raw.githubusercontent.com/linksplatform/Documentation/master/doc/Examples/sqlite_vs_doublets_performance.png "Результат сравнения производительности SQLite и Дуплетов")](https://github.com/linksplatform/Comparisons.SQLiteVSDoublets)

## Описание

Вдохновлено работой Симона Вильямса ([Ассоциативная модель данных - англ.](https://en.wikipedia.org/w/index.php?title=Associative_model_of_data&oldid=913469847)), [книга (англ.)](http://www.sentences.com/docs/other_docs/AMD.pdf), [сравнение с реляционными моделями данных (англ.)](http://iacis.org/iis/2009/P2009_1301.pdf).

Сравнение моделей данных:

![Сравнение моделей данных](https://github.com/LinksPlatform/Documentation/raw/master/doc/ModelsComparison/relational_model_vs_associative_model_vs_links_ru.png)

Сравнение теорий:

![Сравнение теорий](https://github.com/LinksPlatform/Documentation/raw/master/doc/TheoriesComparison/theories_comparison_ru.png)

Эта платформа использует объединённый тип данных — связь, который является комбинацией Элемента и Связи из оригинальной работы Симона Вильямса. Таким образом Элемент или Точка являются частным случаем [связи, которая ссылается сама на себя](http://linksplatform.github.io/itself.html).

Есть два варианта структуры Связи:

> <img src="https://raw.githubusercontent.com/LinksPlatform/Documentation/master/doc/ST.png" width="400" title="Связь Начало-Конец, нетипизированная" alt="Связь Начало-Конец, нетипизированная" />
> <img src="https://raw.githubusercontent.com/LinksPlatform/Documentation/master/doc/ST-dots.png" width="400" title="Связь Начало-Конец, нетипизированная" alt="Связь Начало-Конец, нетипизированная" />

- Нетипизированная, каждая связь содержит Source (Начало, Подлежащее) и Target (Конец, Сказуемое, Дополнение).

> <img src="https://raw.githubusercontent.com/LinksPlatform/Documentation/master/doc/SLT.png" width="400" title="Связь Начало-Связка-Конец, типизированная" alt="Связь Начало-Связка-Конец, типизированная" />
> <img src="https://raw.githubusercontent.com/LinksPlatform/Documentation/master/doc/SLT-dots.png" width="400" title="Связь Начало-Связка-Конец, типизированная" alt="Связь Начало-Связка-Конец, типизированная" />

- Типизированная, с добавленным Linker (Глагол, Тип, Связка, Предикат, Сказуемое), так, что теперь любая дополнительная информация о типе соединения между двумя связями может быть записана в это дополнительное поле.

Платформа Связей запланирована как система, которая комбинирует хранилище ассоциативной памяти (Связи) и движок выполнения трансформаций (Триггеры). Эту систему можно будет программировать динамически, благодаря тому факту, что все алгоритмы будут восприниматься как данные внутри этого хранилища. Такие алгоритмы также способны изменять сами себя в режиме реального времени на основе входных данных из окружающей среды. Платформа Связей это один из способов моделирования высокоуровневых эффектов ассоциативной памяти человеческого разума.

Одна из важнейших целей проекта — ускорить развитие автоматизации до того уровня, чтобы можно было автоматизировать саму автоматизацию. Другими словами, этот проект должен позволить реализовать робота-программиста, который смог бы создавать программы на основе описания на человеческом языке.

## Дорожная карта
![Технологическая Дорожная Карта, Состояние, Прогресс](https://raw.githubusercontent.com/LinksPlatform/Documentation/master/doc/RoadMap-status-ru.png "Технологическая Дорожная Карта, Состояние, Прогресс")

[Текущее состояние разработки (англ.)](https://github.com/Konard/LinksPlatform/milestones)

## Концепт Swagger Connector

[Links (doublets) API swagger connector.](https://gist.github.com/Konard/c76f9948bb25a0d7aff1)

[Links (triplets, micro RDF) API swagger connector.](https://gist.github.com/Konard/e6a0bff583bbca4d452b)

## [Часто задаваемые вопросы](https://github.com/Konard/LinksPlatform/wiki/%D0%A7%D0%90%D0%92%D0%9E)

## Контакты

https://vk.com/linksplatform

https://vk.com/konard

[![Присоединиться к чату https://gitter.im/Konard/LinksPlatform](https://badges.gitter.im/Join%20Chat.svg "Присоединиться к чату")](https://gitter.im/Konard/LinksPlatform?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
