using System;
using System.Collections.Generic;
using System.Reflection;
using Platform.Data.Core.Triplets;
using Sigil;
using Sigil.NonGeneric;
using Label = System.Reflection.Emit.Label;

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

        private static readonly Link Source = Net.CreateMappedThing(Mapping.Source).SetName("source");
        private static readonly Link Linker = Net.CreateMappedThing(Mapping.Linker).SetName("linker");
        private static readonly Link Target = Net.CreateMappedThing(Mapping.Target).SetName("target");
        private static readonly Link SourceLink = Link.Create(Net.Link, Net.ThatIs, Source).SetName("source link");
        private static readonly Link LinkerLink = Link.Create(Net.Link, Net.ThatIs, Linker).SetName("linker link");
        private static readonly Link TargetLink = Link.Create(Net.Link, Net.ThatIs, Target).SetName("target link");

        ////  link that is represented by 
        ////      link representation that consists of 
        ////          reference to source link
        ////              and 
        ////                  reference to target link
        ////                      and
        ////                          reference to linker link
        //static Link _linkRepresentation = Link.Create(Link.Itself, Net.ThatConsistsOf, Link.Create(_reference, _to, _sourceLink) & Link.Create(_reference, _to, _linkerLink) & Link.Create(_reference, _to, _targetLink));

        private static readonly Link With = Net.CreateMappedLink(Mapping.With).SetName("with");
        private static readonly Link As = Net.CreateMappedLink(Mapping.As).SetName("as");
        private static readonly Link To = Net.CreateMappedThing(Mapping.To).SetName("to");
        private static readonly Link For = Net.CreateMappedThing(Mapping.For).SetName("for");
        private static readonly Link From = Net.CreateMappedLink(Mapping.From).SetName("from");
        private static readonly Link Creation = Net.CreateMappedThing(Mapping.Creation).SetName("creation");
        private static readonly Link Update = Net.CreateMappedThing(Mapping.Update).SetName("update");
        private static readonly Link Selection = Net.CreateMappedThing(Mapping.Selection).SetName("selection");
        private static readonly Link Deletion = Net.CreateMappedThing(Mapping.Deletion).SetName("deletion");
        private static readonly Link Execution = Net.CreateMappedThing(Mapping.Execution).SetName("execution");
        private static readonly Link Links = Net.CreateMappedThing(Mapping.Links).SetName("links");
        private static readonly Link Absence = Net.CreateMappedThing(Mapping.Absence).SetName("absence");
        private static readonly Link Result = Net.CreateMappedThing(Mapping.Result).SetName("result");
        private static readonly Link Representation = Net.CreateMappedThing(Mapping.Representation).SetName("representation");
        private static readonly Link RepresentationOfAbsenceOfLink = Link.Create(Representation, Net.Of, Link.Create(Absence, Net.Of, Net.Link));
        private static readonly Link Argument = Net.CreateMappedThing(Mapping.Argument).SetName("argument");
        private static readonly Link ArgumentLink = Link.Create(Net.Link, Net.ThatIs, Argument).SetName("argument link");
        private static readonly Link RepresentationOfArgumentLink = Link.Create(Representation, Net.Of, ArgumentLink);
        private static readonly Link Itself = Net.CreateMappedThing(Mapping.Itself).SetName("itself");
        private static readonly Link Reference = Net.CreateMappedThing(Mapping.Reference).SetName("reference");
        private static readonly Link RepresenationOfReferenceToItself = Link.Create(Representation, Net.Of, Link.Create(Reference, To, Itself));
        private static readonly Link ThatExactlyIs = Net.CreateMappedLink(Mapping.ThatExactlyIs).SetName("that exactly is");
        private static readonly Link OfMatchingOperationFrom = Net.CreateMappedLink(Mapping.OfMatchingOperationFrom).SetName("of matching operation from");
        private static readonly Link InCaseMatchingValueIs = Net.CreateMappedLink(Mapping.InCaseMatchingValueIs).SetName("in case matching value is");


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
            var dummyLinkDefinitionAfterUpdate = Link.Create(Net.Link, With, Link.Create(Net.String, As, SourceLink) & Link.Create(Net.IsNotA, As, LinkerLink) & Link.Create(Net.Letter, As, TargetLink));



            var creationSample = Link.Create(Creation, Net.Of, Link.Create(Net.Link, With, Link.Create(Net.String, As, SourceLink) & Link.Create(Net.IsNotA, As, LinkerLink) & Link.Create(Net.Letter, As, TargetLink)));

            var updateSample = Link.Create(Link.Create(Update, Net.Of, dummyLinkDefinition), To, dummyLinkDefinitionAfterUpdate);

            var selectionOfManySample = Link.Create(Selection, Net.Of,
                Link.Create(Links, With, Link.Create(Net.Name, As, SourceLink) & Link.Create(Net.ThatIsRepresentedBy, As, LinkerLink)
                                             & Link.Create(Link.Create(Net.Link, With, Link.Create(Net.String, As, SourceLink)), As, TargetLink)));

            var selectionOfOneSample = Link.Create(Selection, Net.Of, CreateDefinitionFromLink(Net.Of));

            var selectionOfPartSample = Link.Create(Selection, Net.Of, Link.Create(SourceLink, Net.Of, Link.Create(LinkerLink, Net.Of, Link.Create(TargetLink, Net.Of, Net.Of))));

            var deletionSample = Link.Create(Deletion, Net.Of, dummyLinkDefinitionAfterUpdate);

            var executionSample = Link.Create(Link.Create(Execution, Net.Of, creationSample & updateSample & selectionOfManySample & selectionOfOneSample & selectionOfPartSample & deletionSample), For, Net.CreateThing());

            var checker = CreateSelectionChecker(selectionOfManySample);

            var set = Net.CreateSet().SetName("set of 'name' link referers");
            Net.Name.WalkThroughReferersAsSource(referer =>
            {
                Link.Create(referer, Net.ContainedBy, set);
            });

            var selectionOfManyFromContext = Link.Create(Link.Create(Selection, Net.Of,
                Link.Create(Links, With, Link.Create(Net.Name, As, SourceLink) & Link.Create(Net.ThatIsRepresentedBy, As, LinkerLink)
                                             & Link.Create(Link.Create(Net.Link, With, Link.Create(Net.String, As, SourceLink)), As, TargetLink))), From, set);

            ExecuteLinksSelectionFromContext(selectionOfManyFromContext, Net.CreateThing());


            // execution <of matching operation from> (sequence)
            //        for (result of ... )

            // _вСлучаеЕслиСопоставляемымЗначениемЯвляется
            // _inCaseMatchingValueIs // это упростит логику

            var alternativeOperationsSequence = L(L(Creation, Net.Of, dummyLinkDefinition), InCaseMatchingValueIs, CreateDefinition(Net.Of, Net.Of, Net.Of)) &
                L(L(Creation, Net.Of, CreateDefinition(dummyLink, Net.And, dummyLink)), InCaseMatchingValueIs, CreateDefinitionFromLink(RepresentationOfAbsenceOfLink));

            var matchingSample = L(L(Execution, OfMatchingOperationFrom, alternativeOperationsSequence),
                    For, L(Result, Net.Of, L(Selection, Net.Of, CreateDefinition(Net.Of, Net.Of, Net.Of))));

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
            var resultOfSelectionOfSourceOfArgumentLink = L(Result, Net.Of, L(Selection, Net.Of, L(SourceLink, Net.Of, RepresentationOfArgumentLink)));

            // = argumentLink.Target <- Matching value

            // = argumentLink.Target.Source;
            var resultOfSelectionOfSourceOfTargetOfArgumentLink = L(Result, Net.Of, L(Selection, Net.Of, L(SourceLink, Net.Of, L(TargetLink, Net.Of, RepresentationOfArgumentLink))));
            // = argumentLink.Target.Linker;
            var resultOfSelectionOfLinkerOfTargetOfArgumentLink = L(Result, Net.Of, L(Selection, Net.Of, L(LinkerLink, Net.Of, L(TargetLink, Net.Of, RepresentationOfArgumentLink))));
            // = argumentLink.Target.Target;
            var resultOfSelectionOfTargetOfTargetOfArgumentLink = L(Result, Net.Of, L(Selection, Net.Of, L(LinkerLink, Net.Of, L(TargetLink, Net.Of, RepresentationOfArgumentLink))));

            // = (link with ((argumentLink.Target.Source as source link) and ('as' as linker link) and ('source link' as target link)))
            var linkDefitionSelectorSource = L(Net.Link, With, L(resultOfSelectionOfSourceOfTargetOfArgumentLink, As, SourceLink) & L(As, As, LinkerLink) & L(SourceLink, As, TargetLink));
            // = (link with ((argumentLink.Target.Target as source link) and ('as' as linker link) and ('target link' as target link)))
            var linkDefitionSelectorLinker = L(Net.Link, With, L(resultOfSelectionOfLinkerOfTargetOfArgumentLink, As, SourceLink) & L(As, As, LinkerLink) & L(LinkerLink, As, TargetLink));
            // = (link with ((argumentLink.Target.Linker as source link) and ('as' as linker link) and ('linker link' as target link)))
            var linkDefitionSelectorTarget = L(Net.Link, With, L(resultOfSelectionOfTargetOfTargetOfArgumentLink, As, SourceLink) & L(As, As, LinkerLink) & L(TargetLink, As, TargetLink));

            var linkDefitionSelectorPart = L(Net.Link, With, L(linkDefitionSelectorSource, As, SourceLink) & L(linkDefitionSelectorLinker, As, LinkerLink) & L(linkDefitionSelectorTarget, As, TargetLink));

            var caseSelectionQuery = L(L(Selection, Net.Of, L(Links, With, L(InCaseMatchingValueIs, As, LinkerLink)
                                                                        & L(linkDefitionSelectorPart, As, TargetLink))), From, resultOfSelectionOfSourceOfArgumentLink);

            // Решить что должно быть аргументом для выполняемой операции. (см. null)

            var exeuctionOfSelectedCase = L(L(Execution, Net.Of, L(Execution, Net.Of, resultOfSelectionOfSourceOfArgumentLink)), For, caseSelectionQuery);

            var cases = matchingQuery.Source.Target;
            var argument = matchingQuery.Target;
            var resultQuery = L(L(Execution, Net.Of, exeuctionOfSelectedCase), For, L(Net.Set, Net.ThatConsistsOf, cases) & argument);

            return resultQuery;
        }

        private static Link Eval(Link link, Link argumentLink)
        {
            if (link.Source == Net.Link && link.Linker == ThatExactlyIs)
                return link.Target;
            else if (link.Source == Result && link.Linker == Net.Of)
                return ExecuteLinkOperationCore(link.Target, argumentLink);
            else if (link == RepresentationOfArgumentLink)
                return argumentLink;
            else
                return link;

            // else
            // Нужна будет также поддержка рекурсивного прохода по опредлению связи, с созданием альтернативной связи (проверить насколько набросок снизу подходит)
            return Link.Create(Eval(link.Source, argumentLink), Eval(link.Linker, argumentLink), Eval(link.Target, argumentLink));
        }

        private static Link EvalWithReferenceToItself(Link link, Link argumentLink)
        {
            if (link.Source == Net.Link && link.Linker == ThatExactlyIs)
                return link.Target;
            else if (link.Source == Result && link.Linker == Net.Of)
                return ExecuteLinkOperationCore(link.Target, argumentLink);
            else if (link == RepresentationOfArgumentLink)
                return argumentLink;
            else if (link == RepresenationOfReferenceToItself)
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
            return Link.Create(Net.Link, With, Link.Create(source, As, SourceLink) & Link.Create(linker, As, LinkerLink) & Link.Create(target, As, TargetLink));
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
                arguments.WalkThroughReferersAsTarget(referer =>
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

                if (operationArgument == RepresentationOfArgumentLink)
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
                return RepresentationOfAbsenceOfLink;
            }
        }

        private static Link ExecuteLinkOperationCore(Link operation, Link argument)
        {
            if (operation.Source == Selection)
            {
                if (operation.Target.Source == Net.Link)
                {
                    return ExecuteLinkSelection(operation, argument);
                }
                else if (operation.Target.Source == Links)
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
            else if (operation.Source == Creation)
            {
                return ExecuteLinkCreation(operation, argument);
            }
            else if (operation.Source == Deletion)
            {
                ExecuteLinkDeletion(operation, argument);
                return null;
            }
            else if (operation.Source.Source == Update)
            {
                return ExecuteLinkUpdate(operation, argument);
            }
            else if (operation.Source.Source == Execution)
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
            GetLinks(definition, out Link sourceLink, out Link linkerLink, out Link targetLink);

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

            GetLinks(definition, out Link sourceLink, out Link linkerLink, out Link targetLink);

            var updateableLinkDefinition = updateQuery.Source.Target;
            var updateableLink = ExecuteLinkSelectionByDefinition(updateableLinkDefinition, argument);

            if (updateableLink != null)
            {
                Link.Update(ref updateableLink, sourceLink, linkerLink, targetLink);
                return updateableLink;
            }
            else
            {
                return RepresentationOfAbsenceOfLink;
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
                if (definition.Source == SourceLink)
                {
                    var link = ExecuteLinkPartSelectionCore(definition.Target, argument);
                    return link.Source;
                }
                else if (definition.Source == LinkerLink)
                {
                    var link = ExecuteLinkPartSelectionCore(definition.Target, argument);
                    return link.Linker;
                }
                else if (definition.Source == TargetLink)
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
                resultLink = RepresentationOfAbsenceOfLink;

            return resultLink;
        }

        private static Link ExecuteLinkSelectionByDefinition(Link definition, Link argument)
        {
            GetLinks(definition.Target, out Link sourceLink, out Link linkerLink, out Link targetLink);

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

        private static readonly Dictionary<Link, Func<Link, bool>> SelectionQueriesCheckers = new Dictionary<Link, Func<Link, bool>>();

        private static HashSet<Link> ExecuteLinksSelectionFromContext(Link selectionQuery, Link argument)
        {
            if (!SelectionQueriesCheckers.TryGetValue(selectionQuery, out Func<Link, bool> checker))
            {
                checker = CreateSelectionChecker(selectionQuery);
                SelectionQueriesCheckers.Add(selectionQuery, checker);
            }

            var set = selectionQuery.Target;

            var result = new HashSet<Link>();

            if (set.Is(Net.Set))
            {
                set.WalkThroughReferersAsTarget(referer =>
                    {
                        if (referer.Linker == Net.ContainedBy && checker(referer.Source))
                            result.Add(referer.Source);
                    });
                set.WalkThroughReferersAsSource(referer =>
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

        private static readonly Dictionary<Link, Func<Link, bool>> SelectionCheckers = new Dictionary<Link, Func<Link, bool>>();

        private static Func<Link, bool> CreateSelectionChecker(Link selectionQuery)
        {
            var definition = selectionQuery.Linker == From ? GetLinksDescriptionDefinition(selectionQuery.Source.Target) : GetLinksDescriptionDefinition(selectionQuery.Target);

            return Compilier.CompileSelectionCheck((il) =>
                {
                    il.DeclareLocal(typeof(Link)); // SelectionTemporaryVar

                    var exitOnFalseLabel = il.DefineLabel();
                    var finalExitLabel = il.DefineLabel();

                    EmitSelectionCheckerCore(il, exitOnFalseLabel, new List<string>(), definition);

                    il.LoadConstant(true); // Помещаем результат по умолчанию в стек
                    il.Branch(finalExitLabel);

                    il.MarkLabel(exitOnFalseLabel);
                    il.LoadConstant(false); // Помещаем результат в случае неудачи в стек

                    il.MarkLabel(finalExitLabel);
                    il.Return();
                });
        }

        private static void EmitSelectionCheckerCore(Emit<Func<Link, bool>> il, Sigil.Label exitLabel, List<string> levels, Link selectionQueryDefinition)
        {
            GetLinks(selectionQueryDefinition, out Link sourceLink, out Link linkerLink, out Link targetLink);

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

        private static void EmitEqualityCheck(Emit<Func<Link, bool>> il, Sigil.Label exitLabel, List<string> levels, Link standardLink, string nameOfStandardLinkProperty)
        {
            // Получаем сравниваемое значение
            il.LoadArgument(0);

            levels.ForEach(x =>
            {
                il.Call(typeof(Link).GetMethod("get_" + x));
            });
            il.Call(typeof(Link).GetMethod("get_" + nameOfStandardLinkProperty));

            // Создаём структуру Link по указателю на эталонную связя
            il.LoadConstant(standardLink.ToIndex());
            il.Convert<ulong>();
            il.NewObject<Link>();

            // Выполняем сравнение
            il.Call(typeof(Link).GetMethod("op_Equality"));

            il.BranchIfFalse(exitLabel);
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
                GetLinks(definition, out Link sourceLink, out Link linkerLink, out Link targetLink);

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

        private class SetsCountComparer : IComparer<HashSet<Link>>
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

            if (param.Target == SourceLink)
            {
                foreach (var value in paramValues)
                    value.WalkThroughReferersAsSource(referer => { result.Add(referer); });
            }
            else if (param.Target == LinkerLink)
            {
                foreach (var value in paramValues)
                    value.WalkThroughReferersAsLinker(referer => { result.Add(referer); });
            }
            else if (param.Target == TargetLink)
            {
                foreach (var value in paramValues)
                    value.WalkThroughReferersAsTarget(referer => { result.Add(referer); });
            }

            return result;
        }

        private static HashSet<Link> CollectParamLinkReferers(Link param)
        {
            var result = new HashSet<Link>();

            if (param.Target == SourceLink)
            {
                param.Source.WalkThroughReferersAsSource(referer => { result.Add(referer); });
            }
            else if (param.Target == LinkerLink)
            {
                param.Source.WalkThroughReferersAsLinker(referer => { result.Add(referer); });
            }
            else if (param.Target == TargetLink)
            {
                param.Source.WalkThroughReferersAsTarget(referer => { result.Add(referer); });
            }

            return result;
        }

        private static long GetParamLinkReferersCount(Link param)
        {
            if (param.Target == SourceLink)
                return param.Source.ReferersBySourceCount;
            else if (param.Target == LinkerLink)
                return param.Source.ReferersByLinkerCount;
            else if (param.Target == TargetLink)
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
                if (x.Linker == As)
                {
                    if (x.Target == SourceLink)
                    {
                        sourceLinkLocal = x.Source;
                    }
                    else if (x.Target == LinkerLink)
                    {
                        linkerLinkLocal = x.Source;
                    }
                    else if (x.Target == TargetLink)
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
            if (link.Source == Links || link.Source == Net.Link)
            {
                if (link.Linker == With)
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
            return paramSource.Source != Net.Link || paramSource.Linker != With;
        }
    }
}