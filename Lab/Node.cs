// =============================================================
//  המחלקה Node<T> - כמו בבגרות. אין צורך לשנות כאן דבר.
// =============================================================
public class Node<T>
{
    private T value;
    private Node<T> next;

    public Node(T value)
    {
        this.value = value;
        this.next = null;
    }

    public Node(T value, Node<T> next)
    {
        this.value = value;
        this.next = next;
    }

    public T GetValue()
    {
        return value;
    }

    public Node<T> GetNext()
    {
        return next;
    }

    public void SetValue(T value)
    {
        this.value = value;
    }

    public void SetNext(Node<T> next)
    {
        this.next = next;
    }

    public bool HasNext()
    {
        return next != null;
    }

    public override string ToString()
    {
        return value.ToString();
    }
}
