/*

using System;
namespace NetLibrary
{
    interface ILink
    {
        ILink Source { get; }
        ILink Linker { get; }
        ILink Target { get; }

        void WalkThroughReferersByLinker(Action<ILink> walker);
        void WalkThroughReferersAsSource(Action<ILink> walker);
        void WalkThroughReferersAsTarget(Action<ILink> walker);

        // И статические методы (общие для всех связей)

        public static ILink Create(ILink source, ILink linker, ILink target);
        public static void Update(ref ILink link, ILink newSource, ILink newLinker, ILink newTarget);
        public static void Delete(ref ILink link);
        public static ILink Search(ILink source, ILink linker, ILink target);
    }
}

*/

/*
Набор функций, который необходим для работы с сущностью Link:

(Работа со значением сущности Link, значение состоит из 3-х частей, также сущностей Link)
1. Получить адрес "начальной" сущности Link. (Получить адрес из поля Source)
2. Получить адрес сущности Link, которая играет роль связки между "начальной" и "конечной" сущностями Link. (Получить адрес из поля Linker)
3. Получить адрес "конечной" сущности Link. (Получить адрес из поля Target)

4. Пройтись по всем сущностями Link, которые ссылаются на сущность Link с указанным адресом, и у которых поле Source равно этому адресу.
5. Пройтись по всем сущностями Link, которые ссылаются на сущность Link с указанным адресом, и у которых поле Linker равно этому адресу.
6. Пройтись по всем сущностями Link, которые ссылаются на сущность Link с указанным адресом, и у которых поле Target равно этому адресу.

7. Создать сущность Link со значением (смыслом), которым являются адреса на другие 3 сущности Link (где первая является "начальной", вторая является "связкой", а третья является "конечной").
8. Обновление сущности Link с указанным адресом новым значением (смыслом), которым являются адреса на другие 3 сущности Link (где первая является "начальной", вторая является "связкой", а третья является "конечной").
9. Удаление сущности Link с указаным адресом.
10. Поиск сущности Link со значением (смыслом), которым являются адреса на другие 3 сущности Link (где первая является "начальной", вторая является "связкой", а третья является "конечной").
*/