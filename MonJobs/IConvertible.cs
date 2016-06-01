namespace MonJobs
{
    public interface IConvertible<out T>
    {
        T ToValueType();
    }
}