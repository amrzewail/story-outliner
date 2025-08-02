public class IsLoadedCondition : ICondition
{
    public bool Test() => !string.IsNullOrEmpty(Controller.Instance.CurrentFile);
}
