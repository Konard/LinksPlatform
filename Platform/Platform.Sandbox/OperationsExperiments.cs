using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Platform.Data.Core.Triplets;
using Platform.Helpers;

namespace Platform.Sandbox
{
    public class OperationsExperiments
    {
        public enum Mapping
        {
            Source = 200,
            Linker,
            Target,

            With,
            As,
            To,
            For,
            From,
            Creation,
            Update,
            Selection,
            Deletion,
            Execution,
            Links,
            Absence,
            Result,
            Representation,
            Argument,

            Itself,
            Reference,

            ThatExactlyIs,

            OfMatchingOperationFrom,
            InCaseMatchingValueIs
        }

        //static Link _reference = Net.CreateThing();
        //static Link _to = Net.CreateLink();

        static Link _source = Net.CreateMappedThing(Mapping.Source).SetName("source");
        static Link _linker = Net.CreateMappedThing(Mapping.Linker).SetName("linker");
        static Link _target = Net.CreateMappedThing(Mapping.Target).SetName("target");

        static Link _sourceLink = Link.Create(Net.Link, Net.ThatIs, _source).SetName("source link");
        static Link _linkerLink = Link.Create(Net.Link, Net.ThatIs, _linker).SetName("linker link");
        static Link _targetLink = Link.Create(Net.Link, Net.ThatIs, _target).SetName("target link");

        ////  link that is represented by 
        ////      link representation that consists of 
        ////          reference to source link
        ////              and 
        ////                  reference to target link
        ////                      and
        ////                          reference to linker link
        //static Link _linkRepresentation = Link.Create(Link.Itself, Net.ThatConsistsOf, Link.Create(_reference, _to, _sourceLink) & Link.Create(_reference, _to, _linkerLink) & Link.Create(_reference, _to, _targetLink));

        static Link _with = Net.CreateMappedLink(Mapping.With).SetName("with");
        static Link _as = Net.CreateMappedLink(Mapping.As).SetName("as");
        static Link _to = Net.CreateMappedThing(Mapping.To).SetName("to");
        static Link _for = Net.CreateMappedThing(Mapping.For).SetName("for");
        static Link _from = Net.CreateMappedLink(Mapping.From).SetName("from");
        static Link _creation = Net.CreateMappedThing(Mapping.Creation).SetName("creation");
        static Link _update = Net.CreateMappedThing(Mapping.Update).SetName("update");
        static Link _selection = Net.CreateMappedThing(Mapping.Selection).SetName("selection");
        static Link _deletion = Net.CreateMappedThing(Mapping.Deletion).SetName("deletion");
        static Link _execution = Net.CreateMappedThing(Mapping.Execution).SetName("execution");
        static Link _links = Net.CreateMappedThing(Mapping.Links).SetName("links");
        static Link _absence = Net.CreateMappedThing(Mapping.Absence).SetName("absence");
        static Link _result = Net.CreateMappedThing(Mapping.Result).SetName("result");
        static Link _representation = Net.CreateMappedThing(Mapping.Representation).SetName("representation");
        static Link _representationOfAbsenceOfLink = Link.Create(_representation, Net.Of, Link.Create(_absence, Net.Of, Net.Link));
        static Link _argument = Net.CreateMappedThing(Mapping.Argument).SetName("argument");
        static Link _argumentLink = Link.Create(Net.Link, Net.ThatIs, _argument).SetName("argument link");
        static Link _representationOfArgumentLink = Link.Create(_representation, Net.Of, _argumentLink);

        static Link _itself = Net.CreateMappedThing(Mapping.Itself).SetName("itself");
        static Link _reference = Net.CreateMappedThing(Mapping.Reference).SetName("reference");
        static Link _represenationOfReferenceToItself = Link.Create(_representation, Net.Of, Link.Create(_reference, _to, _itself));

        static Link _thatExactlyIs = Net.CreateMappedLink(Mapping.ThatExactlyIs).SetName("that exactly is");

        static Link _ofMatchingOperationFrom = Net.CreateMappedLink(Mapping.OfMatchingOperationFrom).SetName("of matching operation from");
        static Link _inCaseMatchingValueIs = Net.CreateMappedLink(Mapping.InCaseMatchingValueIs).SetName("in case matching value is");


        // conditional execution of ((x) or (y)) determined by existense of Z
        // execution of ((X in case of equality of (Z and (link definition)))
        // execution of ((Y in case of equality of (Z and (representaion of absence))
        // execution of ((Y in case of absence of Z))

        // execution <of matching operation from> (... and ...)
        // for (X)

        // (execution of X) if (Z is true)

        // selection of set

        //static Link _conditionalExecution = Net.CreateThing();
        //static Link _

        public static void RunExperiment()
        {
            // Creation of operation

            // Creation operation requires

            // В данный момент, чтобы работал указанный мной синтаксис, технически получается, нужно делать определение этого синтаксиса (описывать его в самой же сети),
            // либо пробовать стандартизировать базовые операции путём ввода стандартного способа передачи параметров и выполнения операций



            var dummyLink = Net.CreateThing();
            dummyLink.SetName("dummyLink");

            // Проверить удаление, и после этого можно спокойно приступать к работе с операциями

            var dummyLinkDefinition = CreateDefinitionFromLink(dummyLink);
            var dummyLinkDefinitionAfterUpdate = Link.Create(Net.Link, _with, Link.Create(Net.String, _as, _sourceLink) & Link.Create(Net.IsNotA, _as, _linkerLink) & Link.Create(Net.Letter, _as, _targetLink));



            var creationSample = Link.Create(_creation, Net.Of, Link.Create(Net.Link, _with, Link.Create(Net.String, _as, _sourceLink) & Link.Create(Net.IsNotA, _as, _linkerLink) & Link.Create(Net.Letter, _as, _targetLink)));

            var updateSample = Link.Create(Link.Create(_update, Net.Of, dummyLinkDefinition), _to, dummyLinkDefinitionAfterUpdate);

            var selectionOfManySample = Link.Create(_selection, Net.Of,
                Link.Create(_links, _with, Link.Create(Net.Name, _as, _sourceLink) & Link.Create(Net.ThatIsRepresentedBy, _as, _linkerLink)
                                             & Link.Create(Link.Create(Net.Link, _with, Link.Create(Net.String, _as, _sourceLink)), _as, _targetLink)));

            var selectionOfOneSample = Link.Create(_selection, Net.Of, CreateDefinitionFromLink(Net.Of));

            var selectionOfPartSample = Link.Create(_selection, Net.Of, Link.Create(_sourceLink, Net.Of, Link.Create(_linkerLink, Net.Of, Link.Create(_targetLink, Net.Of, Net.Of))));

            var deletionSample = Link.Create(_deletion, Net.Of, dummyLinkDefinitionAfterUpdate);

            var executionSample = Link.Create(Link.Create(_execution, Net.Of, creationSample & updateSample & selectionOfManySample & selectionOfOneSample & selectionOfPartSample & deletionSample), _for, Net.CreateThing());

            var checker = CreateSelectionChecker(selectionOfManySample);

            var set = Net.CreateSet().SetName("set of 'name' link referers");
            Net.Name.WalkThroughReferersBySource(referer =>
            {
                Link.Create(referer, Net.ContainedBy, set);
            });

            var selectionOfManyFromContext = Link.Create(Link.Create(_selection, Net.Of,
                Link.Create(_links, _with, Link.Create(Net.Name, _as, _sourceLink) & Link.Create(Net.ThatIsRepresentedBy, _as, _linkerLink)
                                             & Link.Create(Link.Create(Net.Link, _with, Link.Create(Net.String, _as, _sourceLink)), _as, _targetLink))), _from, set);

            ExecuteLinksSelectionFromContext(selectionOfManyFromContext, Net.CreateThing());


            // execution <of matching operation from> (sequence)
            //        for (result of ... )

            // _вСлучаеЕслиСопоставляемымЗначениемЯвляется
            // _inCaseMatchingValueIs // это упростит логику

            var alternativeOperationsSequence = L(L(_creation, Net.Of, dummyLinkDefinition), _inCaseMatchingValueIs, CreateDefinition(Net.Of, Net.Of, Net.Of)) &
                L(L(_creation, Net.Of, CreateDefinition(dummyLink, Net.And, dummyLink)), _inCaseMatchingValueIs, CreateDefinitionFromLink(_representationOfAbsenceOfLink));

            var matchingSample = L(L(_execution, _ofMatchingOperationFrom, alternativeOperationsSequence),
                    _for, L(_result, Net.Of, L(_selection, Net.Of, CreateDefinition(Net.Of, Net.Of, Net.Of))));

            //ExecuteLinksOperationsExecution(executionSample, Net.CreateThing());

            CompileMatchingQueryExecutionOperation(matchingSample);
        }

        private static Link L(Link source, Link linker, Link target)
        {
            return Link.Create(source, linker, target);
        }

        private static Link CompileMatchingQueryExecutionOperation(Link matchingQuery)
        {
            // = argumentLink.Source; <- Operations alternatives
            var resultOfSelectionOfSourceOfArgumentLink = L(_result, Net.Of, L(_selection, Net.Of, L(_sourceLink, Net.Of, _representationOfArgumentLink)));

            // = argumentLink.Target <- Matching value

            // = argumentLink.Target.Source;
            var resultOfSelectionOfSourceOfTargetOfArgumentLink = L(_result, Net.Of, L(_selection, Net.Of, L(_sourceLink, Net.Of, L(_targetLink, Net.Of, _representationOfArgumentLink))));
            // = argumentLink.Target.Linker;
            var resultOfSelectionOfLinkerOfTargetOfArgumentLink = L(_result, Net.Of, L(_selection, Net.Of, L(_linkerLink, Net.Of, L(_targetLink, Net.Of, _representationOfArgumentLink))));
            // = argumentLink.Target.Target;
            var resultOfSelectionOfTargetOfTargetOfArgumentLink = L(_result, Net.Of, L(_selection, Net.Of, L(_linkerLink, Net.Of, L(_targetLink, Net.Of, _representationOfArgumentLink))));

            // = (link with ((argumentLink.Target.Source as source link) and ('as' as linker link) and ('source link' as target link)))
            var linkDefitionSelectorSource = L(Net.Link, _with, L(resultOfSelectionOfSourceOfTargetOfArgumentLink, _as, _sourceLink) & L(_as, _as, _linkerLink) & L(_sourceLink, _as, _targetLink));
            // = (link with ((argumentLink.Target.Target as source link) and ('as' as linker link) and ('target link' as target link)))
            var linkDefitionSelectorLinker = L(Net.Link, _with, L(resultOfSelectionOfLinkerOfTargetOfArgumentLink, _as, _sourceLink) & L(_as, _as, _linkerLink) & L(_linkerLink, _as, _targetLink));
            // = (link with ((argumentLink.Target.Linker as source link) and ('as' as linker link) and ('linker link' as target link)))
            var linkDefitionSelectorTarget = L(Net.Link, _with, L(resultOfSelectionOfTargetOfTargetOfArgumentLink, _as, _sourceLink) & L(_as, _as, _linkerLink) & L(_targetLink, _as, _targetLink));

            var linkDefitionSelectorPart = L(Net.Link, _with, L(linkDefitionSelectorSource, _as, _sourceLink) & L(linkDefitionSelectorLinker, _as, _linkerLink) & L(linkDefitionSelectorTarget, _as, _targetLink));

            var caseSelectionQuery = L(L(_selection, Net.Of, L(_links, _with, L(_inCaseMatchingValueIs, _as, _linkerLink)
                                                                        & L(linkDefitionSelectorPart, _as, _targetLink))), _from, resultOfSelectionOfSourceOfArgumentLink);

            // Решить что должно быть аргументом для выполняемой операции. (см. null)

            var exeuctionOfSelectedCase = L(L(_execution, Net.Of, L(_execution, Net.Of, resultOfSelectionOfSourceOfArgumentLink)), _for, caseSelectionQuery);

            var cases = matchingQuery.Source.Target;
            var argument = matchingQuery.Target;
            var resultQuery = L(L(_execution, Net.Of, exeuctionOfSelectedCase), _for, L(Net.Set, Net.ThatConsistsOf, cases) & argument);

            return resultQuery;
        }

        private static Link Eval(Link link, Link argumentLink)
        {
            if (link.Source == Net.Link && link.Linker == _thatExactlyIs)
                return link.Target;
            else if (link.Source == _result && link.Linker == Net.Of)
                return ExecuteLinkOperationCore(link.Target, argumentLink);
            else if (link == _representationOfArgumentLink)
                return argumentLink;
            else
                return link;

            // else
            // Нужна будет также поддержка рекурсивного прохода по опредлению связи, с созданием альтернативной связи (проверить насколько набросок снизу подходит)
            return Link.Create(Eval(link.Source, argumentLink), Eval(link.Linker, argumentLink), Eval(link.Target, argumentLink));
        }

        private static Link EvalWithReferenceToItself(Link link, Link argumentLink)
        {
            if (link.Source == Net.Link && link.Linker == _thatExactlyIs)
                return link.Target;
            else if (link.Source == _result && link.Linker == Net.Of)
                return ExecuteLinkOperationCore(link.Target, argumentLink);
            else if (link == _representationOfArgumentLink)
                return argumentLink;
            else if (link == _represenationOfReferenceToItself)
                return null;
            else
                return link;
        }

        private static Link CreateDefinitionFromLink(Link link)
        {
            return CreateDefinition(link.Source, link.Linker, link.Target);
        }

        private static Link CreateDefinition(Link source, Link linker, Link target)
        {
            return Link.Create(Net.Link, _with, Link.Create(source, _as, _sourceLink) & Link.Create(linker, _as, _linkerLink) & Link.Create(target, _as, _targetLink));
        }

        private static Link ExecuteLinksOperationsExecution(Link executionQuery, Link argument)
        {
            var operations = executionQuery.Source.Target;
            var arguments = executionQuery.Target;

            var operationsList = new List<Link>();

            if (operations.Linker == Net.And)
                operations.WalkThroughSequence(operation => operationsList.Add(operation));
            else
                operationsList.Add(operations);

            var argumentsList = new List<Link>();

            if (arguments.Linker == Net.And)
                arguments.WalkThroughSequence(arg => argumentsList.Add(arg));
            else if (arguments.Is(Net.Set))
            {
                arguments.WalkThroughReferersByTarget(referer =>
                {
                    if (referer.Linker == Net.ContainedBy)
                        argumentsList.Add(referer.Source);
                });
            }
            else
                argumentsList.Add(arguments);

            Link result = null;

            for (var j = 0; j < argumentsList.Count; j++)
            {
                var operationArgument = argumentsList[j];

                if (operationArgument == _representationOfArgumentLink)
                    operationArgument = argument;

                for (var i = 0; i < operationsList.Count; i++)
                {
                    result = ExecuteLinkOperationCore(operationsList[i], operationArgument);
                }
            }

            if (result != null && argumentsList.Count == 1)
            {
                return result;
            }
            else
            {
                return _representationOfAbsenceOfLink;
            }
        }

        private static Link ExecuteLinkOperationCore(Link operation, Link argument)
        {
            if (operation.Source == _selection)
            {
                if (operation.Target.Source == Net.Link)
                {
                    return ExecuteLinkSelection(operation, argument);
                }
                else if (operation.Target.Source == _links)
                {
                    var set = Net.CreateSet();
                    var selectionResult = ExecuteLinksSelection(operation, argument);
                    foreach (var item in selectionResult)
                        Link.Create(item, Net.ContainedBy, set);
                    return set;
                }
                else
                {
                    return ExecuteLinkPartSelection(operation, argument);
                }
            }
            else if (operation.Source == _creation)
            {
                return ExecuteLinkCreation(operation, argument);
            }
            else if (operation.Source == _deletion)
            {
                ExecuteLinkDeletion(operation, argument);
                return null;
            }
            else if (operation.Source.Source == _update)
            {
                return ExecuteLinkUpdate(operation, argument);
            }
            else if (operation.Source.Source == _execution)
            {
                return ExecuteLinksOperationsExecution(operation, argument);
            }
            else
            {
                // Сюда может быть добавлена поддержка пользовательских функций
            }
            return null;
        }

        private static Link ExecuteLinkCreation(Link creationQuery, Link argument)
        {
            var definition = GetLinksDescriptionDefinition(creationQuery.Target);
            return ExecuteLinkCreationCore(definition, argument);
        }

        private static Link ExecuteLinkCreationCore(Link definition, Link argument)
        {
            Link sourceLink, linkerLink, targetLink;
            GetLinks(definition, out sourceLink, out linkerLink, out targetLink);

            if (sourceLink != null && !IsLinkDescriptionParamSourceSpecific(sourceLink))
            {
                var subDefinition = GetLinksDescriptionDefinition(sourceLink);
                sourceLink = ExecuteLinkCreationCore(subDefinition, argument);
            }
            if (linkerLink != null && !IsLinkDescriptionParamSourceSpecific(linkerLink))
            {
                var subDefinition = GetLinksDescriptionDefinition(linkerLink);
                linkerLink = ExecuteLinkCreationCore(subDefinition, argument);
            }
            if (targetLink != null && !IsLinkDescriptionParamSourceSpecific(targetLink))
            {
                var subDefinition = GetLinksDescriptionDefinition(targetLink);
                targetLink = ExecuteLinkCreationCore(targetLink, argument);
            }

            if (sourceLink == null || linkerLink == null || targetLink == null)
            {
                return null;
            }

            return Link.Create(sourceLink, linkerLink, targetLink);
        }

        private static Link ExecuteLinkUpdate(Link updateQuery, Link argument)
        {
            var definition = GetLinksDescriptionDefinition(updateQuery.Target);

            Link sourceLink, linkerLink, targetLink;
            GetLinks(definition, out sourceLink, out linkerLink, out targetLink);

            var updateableLinkDefinition = updateQuery.Source.Target;
            var updateableLink = ExecuteLinkSelectionByDefinition(updateableLinkDefinition, argument);

            if (updateableLink != null)
            {
                Link.Update(ref updateableLink, sourceLink, linkerLink, targetLink);
                return updateableLink;
            }
            else
            {
                return _representationOfAbsenceOfLink;
            }
        }

        private static void ExecuteLinkDeletion(Link deletionQuery, Link argument)
        {
            var deletableLinkDefinition = deletionQuery.Target;
            var deletableLink = ExecuteLinkSelectionByDefinition(deletableLinkDefinition, argument);
            if (deletableLink != null)
                Link.Delete(ref deletableLink);
        }

        private static Link ExecuteLinkPartSelection(Link selectionQuery, Link argument)
        {
            var definition = selectionQuery.Target;
            return ExecuteLinkPartSelectionCore(definition, argument);
        }

        private static Link ExecuteLinkPartSelectionCore(Link definition, Link argument)
        {
            if (definition.Linker == Net.Of)
            {
                if (definition.Source == _sourceLink)
                {
                    var link = ExecuteLinkPartSelectionCore(definition.Target, argument);
                    return link.Source;
                }
                else if (definition.Source == _linkerLink)
                {
                    var link = ExecuteLinkPartSelectionCore(definition.Target, argument);
                    return link.Linker;
                }
                else if (definition.Source == _targetLink)
                {
                    var link = ExecuteLinkPartSelectionCore(definition.Target, argument);
                    return link.Target;
                }
            }
            return definition;
        }

        private static Link ExecuteLinkSelection(Link selectionQuery, Link argument)
        {
            var resultLink = ExecuteLinkSelectionByDefinition(selectionQuery.Target, argument);

            if (resultLink == null)
                resultLink = _representationOfAbsenceOfLink;

            return resultLink;
        }

        private static Link ExecuteLinkSelectionByDefinition(Link definition, Link argument)
        {
            Link sourceLink, linkerLink, targetLink;
            GetLinks(definition.Target, out sourceLink, out linkerLink, out targetLink);

            if (sourceLink != null && !IsLinkDescriptionParamSourceSpecific(sourceLink))
            {
                var subDefinition = GetLinksDescriptionDefinition(sourceLink);
                sourceLink = ExecuteLinkSelectionByDefinition(subDefinition, argument);
            }
            if (linkerLink != null && !IsLinkDescriptionParamSourceSpecific(linkerLink))
            {
                var subDefinition = GetLinksDescriptionDefinition(linkerLink);
                linkerLink = ExecuteLinkSelectionByDefinition(subDefinition, argument);
            }
            if (targetLink != null && !IsLinkDescriptionParamSourceSpecific(targetLink))
            {
                var subDefinition = GetLinksDescriptionDefinition(targetLink);
                targetLink = ExecuteLinkSelectionByDefinition(targetLink, argument);
            }

            if (sourceLink == null || linkerLink == null || targetLink == null)
            {
                return null;
            }


            return Link.Search(sourceLink, linkerLink, targetLink);
        }

        private static Dictionary<Link, Func<Link, bool>> SelectionQueriesCheckers = new Dictionary<Link, Func<Link, bool>>();

        private static HashSet<Link> ExecuteLinksSelectionFromContext(Link selectionQuery, Link argument)
        {
            Func<Link, bool> checker;
            if (!SelectionQueriesCheckers.TryGetValue(selectionQuery, out checker))
            {
                checker = CreateSelectionChecker(selectionQuery);
                SelectionQueriesCheckers.Add(selectionQuery, checker);
            }

            var set = selectionQuery.Target;

            var result = new HashSet<Link>();

            if (set.Is(Net.Set))
            {
                set.WalkThroughReferersByTarget(referer =>
                    {
                        if (referer.Linker == Net.ContainedBy && checker(referer.Source))
                            result.Add(referer.Source);
                    });
                set.WalkThroughReferersBySource(referer =>
                    {
                        if (referer.Linker == Net.Contains && checker(referer.Target))
                            result.Add(referer.Target);
                    });
            }
            else if (set.Linker == Net.And)
            {
                set.WalkThroughSequence(element =>
                    {
                        if (checker(element))
                            result.Add(element);
                    });
            }
            else
            {
                if (checker(set))
                    result.Add(set);
            }

            return result;
        }

        static Dictionary<Link, Func<Link, bool>> SelectionCheckers = new Dictionary<Link, Func<Link, bool>>();

        private static Func<Link, bool> CreateSelectionChecker(Link selectionQuery)
        {
            var definition = selectionQuery.Linker == _from ? GetLinksDescriptionDefinition(selectionQuery.Source.Target) : GetLinksDescriptionDefinition(selectionQuery.Target);

            return Compilier.CompileSelectionCheck((il) =>
                {
                    il.DeclareLocal(typeof(Link)); // SelectionTemporaryVar

                    var exitOnFalseLabel = il.DefineLabel();
                    var finalExitLabel = il.DefineLabel();

                    EmitSelectionCheckerCore(il, exitOnFalseLabel, new List<string>(), definition);

                    il.EmitLiteralLoad(true); // Помещаем результат по умолчанию в стек
                    il.EmitJumpTo(finalExitLabel);

                    il.MarkLabel(exitOnFalseLabel);
                    il.EmitLiteralLoad(false); // Помещаем результат в случае неудачи в стек

                    il.MarkLabel(finalExitLabel);
                    il.Emit(OpCodes.Ret);
                });
        }

        private static void EmitSelectionCheckerCore(ILGenerator il, Label exitLabel, List<string> levels, Link selectionQueryDefinition)
        {
            Link sourceLink, linkerLink, targetLink;
            GetLinks(selectionQueryDefinition, out sourceLink, out linkerLink, out targetLink);

            if (sourceLink != null)
            {
                if (IsLinkDescriptionParamSourceSpecific(sourceLink))
                {
                    EmitEqualityCheck(il, exitLabel, levels, sourceLink, "Source");
                }
                else
                {
                    levels.Add("Source");

                    EmitSelectionCheckerCore(il, exitLabel, levels, sourceLink.Target);
                }
            }
            if (linkerLink != null)
            {
                if (IsLinkDescriptionParamSourceSpecific(linkerLink))
                {
                    EmitEqualityCheck(il, exitLabel, levels, linkerLink, "Linker");
                }
                else
                {
                    levels.Add("Linker");

                    EmitSelectionCheckerCore(il, exitLabel, levels, linkerLink.Target);
                }
            }
            if (targetLink != null)
            {
                if (IsLinkDescriptionParamSourceSpecific(targetLink))
                {
                    EmitEqualityCheck(il, exitLabel, levels, targetLink, "Target");
                }
                else
                {
                    levels.Add("Target");

                    EmitSelectionCheckerCore(il, exitLabel, levels, targetLink.Target);
                }
            }
        }

        private static void EmitEqualityCheck(ILGenerator il, Label exitLabel, List<string> levels, Link standardLink, string nameOfStandardLinkProperty)
        {
            // Получаем сравнивание значение
            il.Emit(OpCodes.Ldarga_S, (byte)0);
            levels.ForEach((x) =>
            {
                il.EmitCall(typeof(Link).GetMethod("get_" + x));
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloca_S, (byte)0);
            });
            il.EmitCall(typeof(Link).GetMethod("get_" + nameOfStandardLinkProperty));

            // Создаём структуру Link по указателю на эталонную связя
            il.EmitLiteralLoad(standardLink.ToIndex());
            il.Emit(OpCodes.Conv_U);
            il.Emit(OpCodes.Newobj, typeof(Link).GetConstructors()[0]);

            // Выполняем сравнение
            il.EmitCall(typeof(Link).GetMethod("op_Equality"));

            il.Emit(OpCodes.Brfalse_S, exitLabel);
        }

        private static HashSet<Link> ExecuteLinksSelection(Link selectionQuery, Link argument)
        {
            var definition = GetLinksDescriptionDefinition(selectionQuery.Target);

            var specificLinksCount = 0;
            definition.WalkThroughSequence(param =>
            {
                if (IsLinkDescriptionParamSourceSpecific(param.Source))
                    specificLinksCount++;
            });

            if (specificLinksCount == 3)
            {
                Link sourceLink, linkerLink, targetLink;
                GetLinks(definition, out sourceLink, out linkerLink, out targetLink);

                var resultSet = new HashSet<Link>();
                var resultLink = Link.Search(sourceLink, linkerLink, targetLink);
                if (resultLink != null)
                    resultSet.Add(resultLink);
                return resultSet;
            }
            else
                return ExecuteLinksSelectionCore(definition, argument);
        }

        private static HashSet<Link> ExecuteLinksSelectionCore(Link definition, Link argument)
        {
            var possibleResults = new HashSet<Link>[3];
            var possibleResultsCount = 0;

            definition.WalkThroughSequence(param =>
                {
                    if (IsLinkDescriptionParamSourceSpecific(param.Source))
                    {
                        var referers = CollectParamLinkReferers(param);
                        possibleResults[possibleResultsCount++] = referers;
                    }
                    else
                    {
                        var subDefinition = GetLinksDescriptionDefinition(param.Source);
                        var possibleValues = ExecuteLinksSelectionCore(subDefinition, argument);
                        var referersOfPossibleValues = CollectParamLinkReferers(param, possibleValues);
                        possibleResults[possibleResultsCount++] = referersOfPossibleValues;
                    }
                });

            if (possibleResultsCount > 1)
            {
                Array.Sort(possibleResults, 0, possibleResultsCount, SetsCountComparer.Default);

                var result = possibleResults[0];

                for (var i = 1; i < possibleResultsCount; i++)
                {
                    result.IntersectWith(possibleResults[i]);
                }

                return result;
            }
            else if (possibleResultsCount == 1)
            {
                return possibleResults[0];
            }
            else
            {
                return new HashSet<Link>();
            }
        }

        class SetsCountComparer : IComparer<HashSet<Link>>
        {
            public static SetsCountComparer Default = new SetsCountComparer();

            private SetsCountComparer()
            {
            }

            public int Compare(HashSet<Link> x, HashSet<Link> y)
            {
                return x.Count.CompareTo(y.Count);
            }
        }

        private static HashSet<Link> CollectParamLinkReferers(Link param, HashSet<Link> paramValues)
        {
            var result = new HashSet<Link>();

            if (param.Target == _sourceLink)
            {
                foreach (var value in paramValues)
                    value.WalkThroughReferersBySource(referer => { result.Add(referer); });
            }
            else if (param.Target == _linkerLink)
            {
                foreach (var value in paramValues)
                    value.WalkThroughReferersByLinker(referer => { result.Add(referer); });
            }
            else if (param.Target == _targetLink)
            {
                foreach (var value in paramValues)
                    value.WalkThroughReferersByTarget(referer => { result.Add(referer); });
            }

            return result;
        }

        private static HashSet<Link> CollectParamLinkReferers(Link param)
        {
            var result = new HashSet<Link>();

            if (param.Target == _sourceLink)
            {
                param.Source.WalkThroughReferersBySource(referer => { result.Add(referer); });
            }
            else if (param.Target == _linkerLink)
            {
                param.Source.WalkThroughReferersByLinker(referer => { result.Add(referer); });
            }
            else if (param.Target == _targetLink)
            {
                param.Source.WalkThroughReferersByTarget(referer => { result.Add(referer); });
            }

            return result;
        }

        private static long GetParamLinkReferersCount(Link param)
        {
            if (param.Target == _sourceLink)
                return param.Source.ReferersBySourceCount;
            else if (param.Target == _linkerLink)
                return param.Source.ReferersByLinkerCount;
            else if (param.Target == _targetLink)
                return param.Source.ReferersByTargetCount;
            else
                throw new Exception("Бляяя");
        }

        private static void GetLinks(Link sequence, out Link sourceLink, out Link linkerLink, out Link targetLink)
        {
            Link sourceLinkLocal = null;
            Link linkerLinkLocal = null;
            Link targetLinkLocal = null;

            sequence.WalkThroughSequence(x =>
            {
                if (x.Linker == _as)
                {
                    if (x.Target == _sourceLink)
                    {
                        sourceLinkLocal = x.Source;
                    }
                    else if (x.Target == _linkerLink)
                    {
                        linkerLinkLocal = x.Source;
                    }
                    else if (x.Target == _targetLink)
                    {
                        targetLinkLocal = x.Source;
                    }
                }
            });

            sourceLink = sourceLinkLocal;
            linkerLink = linkerLinkLocal;
            targetLink = targetLinkLocal;
        }

        private static Link GetLinksDescriptionDefinition(Link link)
        {
            if (link.Source == _links || link.Source == Net.Link)
            {
                if (link.Linker == _with)
                {
                    return link.Target;
                }
            }
            return null;
        }

        private static void CollectAllSpecificLinks(Link linkDescription, ref HashSet<Link> result)
        {
            var linkDescriptionParamsSequence = linkDescription.Target;
            var linkDescriptionParams = LinkConverter.ToList(linkDescriptionParamsSequence);

            foreach (var param in linkDescriptionParams)
            {
                var paramSource = param.Source;
                if (IsLinkDescriptionParamSourceSpecific(paramSource))
                {
                    result.Add(paramSource);
                }
                else
                {
                    var subLinkDescription = paramSource;
                    CollectAllSpecificLinks(subLinkDescription, ref result);
                }
            }
        }

        private static bool IsLinkDescriptionParamSourceSpecific(Link paramSource)
        {
            return paramSource.Source != Net.Link || paramSource.Linker != _with;
        }
    }
}
