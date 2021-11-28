namespace HMTSolution.BCS.Validations
{
    public interface IStructValidator
    {
        string PropertyName { get; set; }
    }
    public class BaseStructValidator<T> : BaseAbstractValidator<T>, IStructValidator
    {
        public string PropertyName { get; set; }
    }
}
