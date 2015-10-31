#LinksPlatform

![Intro animation](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/intro-animation-500.gif "Intro animation")

Inspired by work of Simon Williams ([The Associative Model of Data](http://en.wikipedia.org/w/index.php?title=Associative_model_of_data&oldid=417122527)), [book](http://www.sentences.com/docs/other_docs/AMD.pdf).

Вдохновлено работой Симона Вильямса ([Ассоциативная модель данных](http://en.wikipedia.org/w/index.php?title=Associative_model_of_data&oldid=417122527)), [книга](http://www.sentences.com/docs/other_docs/AMD.pdf).

This platform uses unified data type - link, which is a combination of Item and Link from a work by Simon Williams.

Эта платформа использует объединённый тип данных - связь, который является комбинацией Элемента и Связи из оригинальной работы Симона Вильямса.

There also at least two variants of Link structure:

Также существует как минимум два варианта структуры Связи:

> ![Source-Target link, untyped](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/ST.png "Source-Target link, untyped")
> ![Source-Target link, untyped](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/ST-dots.png "Source-Target link, untyped")

- Untyped, the simplest yet, each link contains only Source (Beginning) and Target (Ending).
- Нетипизированная, простейшая (пока ещё), каждая связь содержит только Source (Начало) и Target (Конец).

> ![Source-Linker-Target link, typed](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/SLT.png "Source-Linker-Target link, typed")
> ![Source-Linker-Target link, typed](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/SLT-dots.png "Source-Linker-Target link, typed")

- Typed, with added Linker (Verb, Type) definition, so any additional info about type of connection between two links can be stored here.
- Типизированная, с добавленным поле Linker (Глагол, Тип, Связка, Предикат), так, что теперь любая дополнительная информация о типе соединения между двумя связями может быть записана в это дополнительное поле.

Links Platform is a system, that combine simple database (Links) and execution engine (Triggers). So it is provide ability to program that system in any way (also dynamically), due to fact that all algorithms are data inside this database. Idea behind Links Platform is a model of associative memory of human mind. So it is actually copy most of it advantages and disadvantages.

Платформа Связей это система, которая комбинирует простую базу данных (Связи) и движок выполнения операций (Триггеры). Таким образом предоставляется возможность программирования этой системы любым способом (в том числе динамически), благодаря тому факту, что все алгоритмы являются данными внутри этой базы данных. Идея, которая стоит за Платформой Связей это модель ассоциативной памяти человека. В итоге копируются большинство преимуществ и недостатков такой модели.

## Road Map
![Road Map, Status](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/RoadMap-status.png "Road Map, Status")

![Технологическая Дорожная Карта, Состояние, Прогресс](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/RoadMap-status-ru.png "Технологическая Дорожная Карта, Состояние, Прогресс")

[Project status | Текущее состояние разработки](https://github.com/Konard/LinksPlatform/milestones)

## Swagger Connector

[Links (pairs) API swagger connector.](https://gist.github.com/Konard/c76f9948bb25a0d7aff1)

[Links (triples, micro RDF) API swagger connector.](https://gist.github.com/Konard/e6a0bff583bbca4d452b)

## [Frequently asked questions](https://github.com/Konard/LinksPlatform/wiki/FAQ) | [Часто задаваемые вопросы](https://github.com/Konard/LinksPlatform/wiki/%D0%A7%D0%90%D0%92%D0%9E)

## Contacts

https://vk.com/linksplatform

[![Join the chat at https://gitter.im/Konard/LinksPlatform](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/Konard/LinksPlatform?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
