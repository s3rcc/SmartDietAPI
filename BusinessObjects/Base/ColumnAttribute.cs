namespace BusinessObjects.Base
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public int Index { get; }

        public ColumnAttribute(int index)
        {
            Index = index;
        }
    }
} 