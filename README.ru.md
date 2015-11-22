#Платформа Связей ([english version](https://github.com/Konard/LinksPlatform/blob/master/README.md))

[![вступление](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/intro-animation-500.gif "вступление")](https://github.com/Konard/LinksPlatform/wiki/%D0%9E-%D1%82%D0%BE%D0%BC,-%D0%BA%D0%B0%D0%BA-%D0%B2%D1%81%D1%91-%D0%BD%D0%B0%D1%87%D0%B8%D0%BD%D0%B0%D0%BB%D0%BE%D1%81%D1%8C)

Вдохновлено работой Симона Вильямса ([Ассоциативная модель данных - англ.](http://en.wikipedia.org/w/index.php?title=Associative_model_of_data&oldid=417122527)), [книга (англ.)](http://www.sentences.com/docs/other_docs/AMD.pdf).

Эта платформа использует объединённый тип данных - связь, который является комбинацией Элемента и Связи из оригинальной работы Симона Вильямса. Таким образом Элемент или Точка являются частным случаем связи, которая ссылается сама на себя.

Также существует как минимум два варианта структуры Связи:

> ![Source-Target link, untyped](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/ST.png "Source-Target связь, нетипизированная")
> ![Source-Target link, untyped](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/ST-dots.png "Source-Target связь, нетипизированная")

- Нетипизированная, простейшая (пока ещё), каждая связь содержит только Source (Начало, Подлежащее) и Target (Конец, Сказуемое, Дополнение).

> ![Source-Linker-Target link, typed](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/SLT.png "Source-Linker-Target связь, типизированная")
> ![Source-Linker-Target link, typed](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/SLT-dots.png "Source-Linker-Target связь, типизированная")

- Типизированная, с добавленным поле Linker (Глагол, Тип, Связка, Предикат, Сказуемое), так, что теперь любая дополнительная информация о типе соединения между двумя связями может быть записана в это дополнительное поле.

Платформа Связей это система, которая комбинирует простую базу данных (Связи) и движок выполнения операций (Триггеры). Таким образом предоставляется возможность программирования этой системы любым способом (в том числе динамически), благодаря тому факту, что все алгоритмы являются данными внутри этой базы данных. Идея, которая стоит за Платформой Связей это модель ассоциативной памяти человека. В итоге копируются большинство преимуществ и недостатков такой модели.

## Дорожная карта
![Технологическая Дорожная Карта, Состояние, Прогресс](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/RoadMap-status-ru.png "Технологическая Дорожная Карта, Состояние, Прогресс")

[Текущее состояние разработки (англ.)](https://github.com/Konard/LinksPlatform/milestones)

## Концепт Swagger Connector

[Links (pairs) API swagger connector.](https://gist.github.com/Konard/c76f9948bb25a0d7aff1)

[Links (triples, micro RDF) API swagger connector.](https://gist.github.com/Konard/e6a0bff583bbca4d452b)

## [Часто задаваемые вопросы](https://github.com/Konard/LinksPlatform/wiki/%D0%A7%D0%90%D0%92%D0%9E)

## Контакты

https://vk.com/linksplatform
https://vk.com/konard

[![Присоединиться к чату https://gitter.im/Konard/LinksPlatform](https://badges.gitter.im/Join%20Chat.svg "Присоединиться к чату")](https://gitter.im/Konard/LinksPlatform?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
