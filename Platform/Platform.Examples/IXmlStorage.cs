namespace Platform.Examples
{
    public interface IXmlStorage<TLink>
    {
        TLink CreateDocument(string name);
        TLink CreateElement(string name);
        TLink CreateTextElement(string content);
        void AttachElementToParent(TLink elementToAttach, TLink parent);
    }
}
