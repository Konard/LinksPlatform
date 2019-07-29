namespace Platform.Data.Constants
{
    public interface ILinksPartConstants<TPartIndex>
    {
        /// <summary>Возвращает индекс части, которая отвечает за индекс (адрес, идентификатор) самой связи.</summary>
        TPartIndex IndexPart { get; }

        /// <summary>Возвращает индекс части, которая отвечает за ссылку на связь-начало (первая часть-значение).</summary>
        TPartIndex SourcePart { get; }

        /// <summary>Возвращает индекс части, которая отвечает за ссылку на связь-конец (последняя часть-значение).</summary>
        TPartIndex TargetPart { get; }
    }
}
