#LinksPlatform ([русская версия](https://github.com/Konard/LinksPlatform/blob/master/README.ru.md))

[![introduction](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/Intro/intro-animation-500.gif "introduction")](https://github.com/Konard/LinksPlatform/wiki/About-the-beginning)

[Graphical introduction](https://github.com/Konard/LinksPlatform/wiki/About-the-beginning)

Inspired by work of Simon Williams ([The Associative Model of Data](http://en.wikipedia.org/w/index.php?title=Associative_model_of_data&oldid=417122527)), [book](http://www.sentences.com/docs/other_docs/AMD.pdf).

This platform uses unified data type - link, which is a combination of Item and Link from a work by Simon Williams. So the Item or Point is a specific case of link, which references itself.

There also at least two variants of Link structure:

> <img src="https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/ST.png" width="400" title="Source-Target link, untyped" alt="Source-Target link, untyped" />
> <img src="https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/ST-dots.png" width="400" title="Source-Target link, untyped" alt="Source-Target link, untyped" />

- Untyped, the simplest yet, each link contains only Source (Beginning, First, Left, Subject) and Target (Ending, Last, Right, Predicate, Object).

> <img src="https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/SLT.png" width="400" title="Source-Linker-Target link, typed" alt="Source-Linker-Target link, typed" />
> <img src="https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/SLT-dots.png" width="400" title="Source-Linker-Target link, typed" alt="Source-Linker-Target link, typed" />

- Typed, with added Linker definition ([verb](https://en.wikipedia.org/wiki/Verb), [action](https://en.wikipedia.org/wiki/Action_(philosophy)), [type](https://en.wikipedia.org/wiki/Type_system), [category](https://en.wikipedia.org/wiki/Category_theory), [predicate](https://en.wikipedia.org/wiki/Predicate), [transition](https://en.wikipedia.org/wiki/Transition_system), [algorithm](https://en.wikipedia.org/wiki/Algorithm)), so any additional info about type of connection between two links can be stored here.

Links Platform is a system, that combine simple database (Links) and execution engine (Triggers). So it is provide ability to program that system in any way (also dynamically), due to fact that all algorithms are data inside this database. Idea behind Links Platform is a model of associative memory of human mind. So it is actually copy most of it advantages and disadvantages.

## Road map
[![Road Map, Status](https://raw.githubusercontent.com/Konard/LinksPlatform/master/doc/RoadMap-status.png "Road Map, Status")](https://github.com/Konard/LinksPlatform/milestones)

[Project status](https://github.com/Konard/LinksPlatform/milestones)

## Swagger Connector Concept

[Links (pairs) API swagger connector.](https://gist.github.com/Konard/c76f9948bb25a0d7aff1)

[Links (triples, micro RDF) API swagger connector.](https://gist.github.com/Konard/e6a0bff583bbca4d452b)

## [Frequently asked questions](https://github.com/Konard/LinksPlatform/wiki/FAQ)

## Contacts

https://vk.com/linksplatform
https://vk.com/konard

[![Join the chat at https://gitter.im/Konard/LinksPlatform](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/Konard/LinksPlatform?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
