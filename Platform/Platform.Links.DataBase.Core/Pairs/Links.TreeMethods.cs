using System;
using Platform.Links.DataBase.Core.TreeMethods;

namespace Platform.Links.DataBase.Core.Pairs
{
    /// <remarks>
    /// Устранить дублирование логики.
    /// </remarks>
    unsafe partial class Links
    {
        private class LinksSourcesTreeMethods : SizeBalancedTreeMethods2
        {
            private readonly Links _db;
            private readonly Link* _links;
            private readonly LinksHeader* _header;

            public LinksSourcesTreeMethods(Links links, LinksHeader* header)
            {
                _db = links;
                _links = links._links;
                _header = header;
            }

            protected override ulong* GetLeft(ulong node)
            {
                return &_links[node].LeftAsSource;
            }

            protected override ulong* GetRight(ulong node)
            {
                return &_links[node].RightAsSource;
            }

            protected override ulong GetSize(ulong node)
            {
                return _links[node].SizeAsSource;
            }

            protected override void SetLeft(ulong node, ulong left)
            {
                _links[node].LeftAsSource = left;
            }

            protected override void SetRight(ulong node, ulong right)
            {
                _links[node].RightAsSource = right;
            }

            protected override void SetSize(ulong node, ulong size)
            {
                _links[node].SizeAsSource = size;
            }

            protected override bool FirstIsToTheLeftOfSecond(ulong first, ulong second)
            {
                return _links[first].Source < _links[second].Source || (_links[first].Source == _links[second].Source && _links[first].Target < _links[second].Target);
            }

            protected override bool FirstIsToTheRightOfSecond(ulong first, ulong second)
            {
                return _links[first].Source > _links[second].Source || (_links[first].Source == _links[second].Source && _links[first].Target > _links[second].Target);
            }

            private bool FirstIsToTheLeftOfSecond(ulong firstSource, ulong firstTarget, ulong secondSource, ulong secondTarget)
            {
                return firstSource < secondSource || (firstSource == secondSource && firstTarget < secondTarget);
            }

            private bool FirstIsToTheRightOfSecond(ulong firstSource, ulong firstTarget, ulong secondSource, ulong secondTarget)
            {
                return firstSource > secondSource || (firstSource == secondSource && firstTarget > secondTarget);
            }

            /// <summary>
            /// Выполняет поиск и возвращает индекс связи с указанными Source (началом) и Target (концом)
            /// по дереву (индексу) связей, отсортированному по Source, а затем по Target.
            /// </summary>
            /// <param name="source">Индекс связи, которая является началом на искомой связи.</param>
            /// <param name="target">Индекс связи, которая является концом на искомой связи.</param>
            /// <returns>Индекс искомой связи.</returns>
            public ulong Search(ulong source, ulong target)
            {
                var root = _header->FirstAsSource;

                while (root != 0)
                {
                    var rootSource = _links[root].Source;
                    var rootTarget = _links[root].Target;

                    if (FirstIsToTheLeftOfSecond(source, target, rootSource, rootTarget)) // node.Key < root.Key
                        root = *GetLeft(root);
                    else if (FirstIsToTheRightOfSecond(source, target, rootSource, rootTarget)) // node.Key > root.Key
                        root = *GetRight(root);
                    else // node.Key == root.Key
                        return root;
                }

                return 0;
            }

            /// <summary>
            /// Подсчитывает и возвращает общее количество связей ссылающихся на связь с указанным индексом и "начинающихся" с этой связи (использующих её в качестве Source (начала)).
            /// </summary>
            /// <param name="link">Индекс связи.</param>
            /// <returns>Общее количество связей, "начинающихся" с указанной связи.</returns>
            public ulong CalculateReferences(ulong link)
            {
                ulong references = 0;
                EachReference(link, x => { references++; return true; });
                return references;
            }

            /// <summary>
            /// Выполняет проход по всем связям, "начинающихся" с указанной связи. 
            /// </summary>
            /// <param name="source">Индекс связи-начала.</param>
            /// <param name="handler">Функция-обработчик, выполняющая необходимое действие над каждой связью-ссылкой.</param>
            public bool EachReference(ulong source, Func<ulong, bool> handler)
            {
                return EachReferenceCore(source, _header->FirstAsSource, handler);
            }

            private bool EachReferenceCore(ulong source, ulong link, Func<ulong, bool> handler)
            {
                if (link == 0)
                    return true;

                var linkSource = _db.GetSource(link);

                if (linkSource > source)
                {
                    if (EachReferenceCore(source, *GetLeft(link), handler) == Break)
                        return false;
                }
                else if (linkSource < source)
                {
                    if (EachReferenceCore(source, *GetRight(link), handler) == Break)
                        return false;
                }
                else //if (linkSource == source)
                {
                    if (handler(link) == Break)
                        return false;

                    if (EachReferenceCore(source, *GetLeft(link), handler) == Break)
                        return false;
                    if (EachReferenceCore(source, *GetRight(link), handler) == Break)
                        return false;
                }

                return true;
            }
        }

        private class LinksTargetsTreeMethods : SizeBalancedTreeMethods2
        {
            private readonly Links _db;
            private readonly Link* _links;
            private readonly LinksHeader* _header;

            public LinksTargetsTreeMethods(Links links, LinksHeader* header)
            {
                _db = links;
                _links = links._links;
                _header = header;
            }

            protected override ulong* GetLeft(ulong node)
            {
                return &_links[node].LeftAsTarget;
            }

            protected override ulong* GetRight(ulong node)
            {
                return &_links[node].RightAsTarget;
            }

            protected override ulong GetSize(ulong node)
            {
                return _links[node].SizeAsTarget;
            }

            protected override void SetLeft(ulong node, ulong left)
            {
                _links[node].LeftAsTarget = left;
            }

            protected override void SetRight(ulong node, ulong right)
            {
                _links[node].RightAsTarget = right;
            }

            protected override void SetSize(ulong node, ulong size)
            {
                _links[node].SizeAsTarget = size;
            }

            protected override bool FirstIsToTheLeftOfSecond(ulong first, ulong second)
            {
                return _links[first].Target < _links[second].Target || (_links[first].Target == _links[second].Target && _links[first].Source < _links[second].Source);
            }

            protected override bool FirstIsToTheRightOfSecond(ulong first, ulong second)
            {
                return _links[first].Target > _links[second].Target || (_links[first].Target == _links[second].Target && _links[first].Source > _links[second].Source);
            }

            /// <summary>
            /// Подсчитывает и возвращает общее количество связей ссылающихся на связь с указанным индексом и "заканчивающихся" этой связью (использующих её в качестве Target (конца)).
            /// </summary>
            /// <param name="link">Индекс связи.</param>
            /// <returns>Общее количество связей, "заканчивающихся" указанной связью.</returns>
            public ulong CalculateReferences(ulong link)
            {
                ulong references = 0;
                EachReference(link, x => { references++; return true; });
                return references;
            }

            /// <summary>
            /// Выполняет проход по всем связям "заканчивающимся" указанной связью. 
            /// </summary>
            /// <param name="target">Индекс связи-конца.</param>
            /// <param name="handler">Функция-обработчик, выполняющая необходимое действие над каждой связью-ссылкой.</param>
            public bool EachReference(ulong target, Func<ulong, bool> handler)
            {
                return EachReferenceCore(target, _header->FirstAsTarget, handler);
            }

            private bool EachReferenceCore(ulong target, ulong link, Func<ulong, bool> handler)
            {
                if (link == 0)
                    return true;

                var linkTarget = _db.GetTarget(link);

                if (linkTarget > target)
                {
                    if (EachReferenceCore(target, *GetLeft(link), handler) == Break)
                        return false;
                }
                else if (linkTarget < target)
                {
                    if (EachReferenceCore(target, *GetRight(link), handler) == Break)
                        return false;
                }
                else //if (linkTarget == target)
                {
                    if (handler(link) == Break)
                        return false;

                    if (EachReferenceCore(target, *GetLeft(link), handler) == Break)
                        return false;
                    if (EachReferenceCore(target, *GetRight(link), handler) == Break)
                        return false;
                }

                return true;
            }
        }
    }
}
