namespace Application.Builders
{
    public interface IBuilder<TSource, TOutput>
    {
        TOutput Build(TSource source);
    }
}
