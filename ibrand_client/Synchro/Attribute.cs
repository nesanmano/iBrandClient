namespace NomadLogic.iBrandDesktop.Synchro
{
    public class Attribute
    {
        public Attribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public string Value { get; }
    }
}
