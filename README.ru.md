[![Статус сборки](https://travis-ci.org/Konard/LinksPlatform.svg?branch=master "Статус сборки")](https://travis-ci.org/Konard/LinksPlatform)

# ПлатформаСвязей ([english version](https://github.com/linksplatform#linksplatform-%D1%80%D1%83%D1%81%D1%81%D0%BA%D0%B0%D1%8F-%D0%B2%D0%B5%D1%80%D1%81%D0%B8%D1%8F))

Платформа Связей — это [модульный](https://ru.wikipedia.org/wiki/%D0%9C%D0%BE%D0%B4%D1%83%D0%BB%D1%8C%D0%BD%D0%BE%D0%B5_%D0%BF%D1%80%D0%BE%D0%B3%D1%80%D0%B0%D0%BC%D0%BC%D0%B8%D1%80%D0%BE%D0%B2%D0%B0%D0%BD%D0%B8%D0%B5) [фреймворк](https://ru.wikipedia.org/wiki/%D0%A4%D1%80%D0%B5%D0%B9%D0%BC%D0%B2%D0%BE%D1%80%D0%BA), в который входят две [реализации](https://ru.wikipedia.org/wiki/%D0%A0%D0%B5%D0%B0%D0%BB%D0%B8%D0%B7%D0%B0%D1%86%D0%B8%D1%8F) [СУБД](https://ru.wikipedia.org/wiki/%D0%A1%D0%B8%D1%81%D1%82%D0%B5%D0%BC%D0%B0_%D1%83%D0%BF%D1%80%D0%B0%D0%B2%D0%BB%D0%B5%D0%BD%D0%B8%D1%8F_%D0%B1%D0%B0%D0%B7%D0%B0%D0%BC%D0%B8_%D0%B4%D0%B0%D0%BD%D0%BD%D1%8B%D1%85) на основе [ассоциативной модели данных](https://en.wikipedia.org/wiki/Associative_model_of_data): [Дуплеты](https://github.com/linksplatform/Data.Doublets) и [Триплеты](https://github.com/linksplatform/Data.Triplets); а также [трансляторы](https://ru.wikipedia.org/wiki/%D0%A2%D1%80%D0%B0%D0%BD%D1%81%D0%BB%D1%8F%D1%82%D0%BE%D1%80) (например [из C# в C++](https://github.com/linksplatform/RegularExpressions.Transformer.CSharpToCpp)) и [бот](https://github.com/linksplatform/Bot).

Каждая из [библиотек](https://ru.wikipedia.org/wiki/%D0%91%D0%B8%D0%B1%D0%BB%D0%B8%D0%BE%D1%82%D0%B5%D0%BA%D0%B0_(%D0%BF%D1%80%D0%BE%D0%B3%D1%80%D0%B0%D0%BC%D0%BC%D0%B8%D1%80%D0%BE%D0%B2%D0%B0%D0%BD%D0%B8%D0%B5)) этого [фреймворка](https://ru.wikipedia.org/wiki/%D0%A4%D1%80%D0%B5%D0%B9%D0%BC%D0%B2%D0%BE%D1%80%D0%BA) может быть использована отдельно и располагается [на странице организации "Платформы Связей"](https://github.com/linksplatform).

В данный момент мы используем следующие [языки программирования](https://ru.wikipedia.org/wiki/%D0%AF%D0%B7%D1%8B%D0%BA_%D0%BF%D1%80%D0%BE%D0%B3%D1%80%D0%B0%D0%BC%D0%BC%D0%B8%D1%80%D0%BE%D0%B2%D0%B0%D0%BD%D0%B8%D1%8F): [C#](https://ru.wikipedia.org/wiki/C_Sharp), [C++](https://ru.wikipedia.org/wiki/C%2B%2B), [C](https://ru.wikipedia.org/wiki/%D0%A1%D0%B8_(%D1%8F%D0%B7%D1%8B%D0%BA_%D0%BF%D1%80%D0%BE%D0%B3%D1%80%D0%B0%D0%BC%D0%BC%D0%B8%D1%80%D0%BE%D0%B2%D0%B0%D0%BD%D0%B8%D1%8F)), [JavaScript](https://ru.wikipedia.org/wiki/JavaScript) and [Python](https://ru.wikipedia.org/wiki/Python).

[Документация](https://linksplatform.github.io/Documentation/index.ru.html)

[![вступление](https://raw.githubusercontent.com/LinksPlatform/Documentation/master/doc/Intro/intro-animation-500.gif "вступление")](https://github.com/Konard/LinksPlatform/wiki/%D0%9E-%D1%82%D0%BE%D0%BC,-%D0%BA%D0%B0%D0%BA-%D0%B2%D1%81%D1%91-%D0%BD%D0%B0%D1%87%D0%B8%D0%BD%D0%B0%D0%BB%D0%BE%D1%81%D1%8C)

[Графическое вступление](https://github.com/Konard/LinksPlatform/wiki/%D0%9E-%D1%82%D0%BE%D0%BC,-%D0%BA%D0%B0%D0%BA-%D0%B2%D1%81%D1%91-%D0%BD%D0%B0%D1%87%D0%B8%D0%BD%D0%B0%D0%BB%D0%BE%D1%81%D1%8C)

## [Пример для быстрого старта](https://github.com/linksplatform/Examples.Doublets.CRUD.DotNet)

```C#
using System;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

// Хранилище дуплетов привязывается к файлу "db.links":
using var links = new UnitedMemoryLinks<uint>("db.links");

// Создание связи-дуплета: 
var link = links.Create();

// Связь обновляется чтобы ссылаться на себя дважды (в качестве начала и конца):
link = links.Update(link, newSource: link, newTarget: link);

// Операции чтения:
Console.WriteLine($"Количество связей в хранилище данных: {links.Count()}.");
Console.WriteLine("Содержимое хранилища данных:");
var any = links.Constants.Any; // Означает любой адрес связи или отсутствие ограничения на адрес связи
// Аргументы запроса интерпретируются в качестве органичений
var query = new Link<uint>(index: any, source: any, target: any);
links.Each((link) => {
    Console.WriteLine(links.Format(link));
    return links.Constants.Continue;
}, query);

// Сброс содержимого связи:
link = links.Update(link, newSource: default, newTarget: default);

// Удаление связи:
links.Delete(link);
```

## [SQLite против Дуплетов](https://github.com/linksplatform/Comparisons.SQLiteVSDoublets)

[![Изображение с результатом сравнения производительности SQLite и Дуплетов.](https://raw.githubusercontent.com/linksplatform/Documentation/master/doc/Examples/sqlite_vs_doublets_performance.png "Результат сравнения производительности SQLite и Дуплетов")](https://github.com/linksplatform/Comparisons.SQLiteVSDoublets)

## Описание

Вдохновлено работой Симона Вильямса ([Ассоциативная модель данных - англ.](https://web.archive.org/web/20210814063207/https://en.wikipedia.org/wiki/Associative_model_of_data)), [книга (англ.)](https://web.archive.org/web/20181219134621/http://sentences.com/docs/amd.pdf), [сравнение с реляционными моделями данных (англ.)](http://iacis.org/iis/2009/P2009_1301.pdf).

Сравнение моделей данных:

![Сравнение моделей данных](https://github.com/LinksPlatform/Documentation/raw/master/doc/ModelsComparison/relational_model_vs_associative_model_vs_links_ru.png)

Сравнение теорий:

![Сравнение теорий](https://github.com/LinksPlatform/Documentation/raw/master/doc/TheoriesComparison/theories_comparison_ru.png)

Эта платформа использует объединённый тип данных — связь, который является комбинацией Элемента и Связи из оригинальной работы Симона Вильямса. Таким образом Элемент или Точка являются частным случаем [связи, которая ссылается сама на себя](http://linksplatform.github.io/itself.html).

Есть два варианта структуры Связи:

> <img src="https://raw.githubusercontent.com/LinksPlatform/Documentation/master/doc/ST-dots.png" width="400" title="Связь Начало-Конец, нетипизированная" alt="Связь Начало-Конец, нетипизированная" />
> <img src="https://raw.githubusercontent.com/LinksPlatform/Documentation/master/doc/doublet-colored.png" width="400" title="Связь Начало-Конец, нетипизированная" alt="Связь Начало-Конец, нетипизированная" />

- Нетипизированная, каждая связь содержит Source (Начало, Подлежащее) и Target (Конец, Сказуемое, Дополнение).

> <img src="https://raw.githubusercontent.com/LinksPlatform/Documentation/master/doc/SLT-dots.png" width="400" title="Связь Начало-Связка-Конец, типизированная" alt="Связь Начало-Связка-Конец, типизированная" />
> <img src="https://raw.githubusercontent.com/LinksPlatform/Documentation/master/doc/triplet-colored.png" width="400" title="Связь Начало-Связка-Конец, типизированная" alt="Связь Начало-Связка-Конец, типизированная" />

- Типизированная, с добавленным Linker (Глагол, Тип, Связка, Предикат, Сказуемое), так, что теперь любая дополнительная информация о типе соединения между двумя связями может быть записана в это дополнительное поле.

Платформа Связей запланирована как система, которая комбинирует хранилище ассоциативной памяти (Связи) и движок выполнения трансформаций (Триггеры). Эту систему можно будет программировать динамически, благодаря тому факту, что все алгоритмы будут восприниматься как данные внутри этого хранилища. Такие алгоритмы также способны изменять сами себя в режиме реального времени на основе входных данных из окружающей среды. Платформа Связей это один из способов моделирования высокоуровневых эффектов ассоциативной памяти человеческого разума.

Одна из важнейших целей проекта — ускорить развитие автоматизации до того уровня, чтобы можно было автоматизировать саму автоматизацию. Другими словами, этот проект должен позволить реализовать бота-программиста, который смог бы создавать программы на основе описания на человеческом языке.

## Дорожная карта
![Технологическая Дорожная Карта, Состояние, Прогресс](https://raw.githubusercontent.com/LinksPlatform/Documentation/master/doc/RoadMap-status-ru.png "Технологическая Дорожная Карта, Состояние, Прогресс")

[Текущее состояние разработки (англ.)](https://github.com/Konard/LinksPlatform/milestones)

## Концепт Swagger Connector

[Links (doublets) API swagger connector.](https://gist.github.com/Konard/c76f9948bb25a0d7aff1)

[Links (triplets, micro RDF) API swagger connector.](https://gist.github.com/Konard/e6a0bff583bbca4d452b)

## [Часто задаваемые вопросы](https://github.com/Konard/LinksPlatform/wiki/%D0%A7%D0%90%D0%92%D0%9E)

## Support

Задавайте вопросы по адресу [stackoverflow.com/tags/links-platform](https://stackoverflow.com/tags/links-platform) (или с тегом `links-platform`) чтобы получить нашу бесплатную поддержку.

Вы так же можете получить поддержку в режиме реального времени на [нашем официальном Discord сервере](https://discord.gg/eEXJyjWv5e).

## Контакты

https://vk.com/linksplatform

https://vk.com/konard
