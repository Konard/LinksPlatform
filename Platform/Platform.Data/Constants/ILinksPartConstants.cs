namespace Platform.Data.Constants
{
    public interface ILinksPartConstants<TPartIndex>
    {
        /// <summary>Возвращает индекс части, которая отвечает за индекс самой связи.</summary>
        TPartIndex IndexPart { get; }

        /// <summary>Возвращает индекс части, которая отвечает за ссылку на связь-начало.</summary>
        TPartIndex SourcePart { get; }

        /// <summary>Возвращает индекс части, которая отвечает за ссылку на связь-конец.</summary>
        TPartIndex TargetPart { get; }
    }
}
