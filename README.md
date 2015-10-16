#LinksPlatform

![blackspace, whitespace](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/1.png "blackspace, whitespace")
![blackspace, black dot](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/2.png "blackspace, black dot")
![white dot, black dot](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/3.png "white dot, black dot")
![two white dots, black vertical line](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/4.png "two white dots, black vertical line")
![white vertical line, black circle](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/5.png "white vertical line, black circle")
![white circle, black horizontal line](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/6.png "white circle, black horizontal line")
![white horizontal line, black horizontal arrow](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/7.png "white horizontal line, black horizontal arrow")
![white link, black directed link](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/8.png "white link, black directed link")
![white regular & directed links, black typed link](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/9.png "white regular & directed links, black typed link")
![white regular & directed links with recursive inner structure, black typed link with recursive inner structure](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/10.png "white regular & directed links with recursive inner structure, black typed link with recursive inner structure")
![white regular & directed links with double recursive inner structure, black typed link with double recursive inner structure](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/11.png "white regular & directed links with double recursive inner structure, black typed link with double recursive inner structure")
![white regular & directed links with coloured 8 elements sequence structure, black typed link with coloured 8 elements sequence structure](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/12.png "white regular & directed links with coloured 8 elements sequence structure, black typed link with coloured 8 elements sequence structure")

Inspired by work of Simon Williams (The Associative Model of Data, http://en.wikipedia.org/w/index.php?title=Associative_model_of_data&oldid=417122527).

Вдохновлено работой Симона Вильямса (Ассоциативная модель данных, http://en.wikipedia.org/w/index.php?title=Associative_model_of_data&oldid=417122527).

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
> ![Road Map Status](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/RoadMap-status.png "Road Map Status")

## Contacts

https://vk.com/linksplatform

[![Join the chat at https://gitter.im/Konard/LinksPlatform](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/Konard/LinksPlatform?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)