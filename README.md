#LinksPlatform

Inspired by work of Simon Williams (The Associative Model of Data, http://en.wikipedia.org/w/index.php?title=Associative_model_of_data&oldid=417122527).

Вдохновлено работой Симона Вильямса (Ассоциативная модель данных, http://en.wikipedia.org/w/index.php?title=Associative_model_of_data&oldid=417122527).

This platform uses unified data type - link, which is a combination of Item and Link from a work by Simon Williams.

Эта платформа использует объединённый тип данных - связь, который является комбинацией Элемента и Связи из оригинальной работы Симона Вильямса.

There also at least two variants of Link structure:

Также существует как минимум два варианта структуры Связи:

> ![Source-Target link, untyped](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/ST.png "Source-Target link, untyped")

- Untyped, the simplest yet, each link contains only Source (Beginning) and Target (Ending).
- Нетипизированная, простейшая (пока ещё), каждая связь содержит только Source (Начало) и Target (Конец).

> ![Source-Linker-Target link, typed](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/SLT.png "Source-Linker-Target link, typed")

- Typed, with added Linker (Verb, Type) definition, so any additional info about type of connection between two links can be stored here.
- Типизированная, с добавленным поле Linker (Глагол, Тип, Связка, Предикат), так, что теперь любая дополнительная информация о типе соединения между двумя связями может быть записана в это дополнительное поле.

Links Platform is a system, that combine simple database (Links) and execution engine (Triggers). So it is provide ability to program that system in any way (also dynamically), due to fact that all algorithms are data inside this database. Idea behind Links Platform is a model of associative memory of human mind. So it is actually copy most of it advantages and disadvantages.

Платформа Связей это система, которая комбинирует простую базу данных (Связи) и движок выполнения операций (Триггеры). Таким образом предоставляется возможность программирования этой системы любым способом (в том числе динамически), благодаря тому факту, что все алгоритмы являются данными внутри этой базы данных. Идея, которая стоит за Платформой Связей это модель ассоциативной памяти человека. Таким образом копируются большинство приемуществ и недостатков такой модели.
