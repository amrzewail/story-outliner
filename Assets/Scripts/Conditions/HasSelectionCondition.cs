public class HasSelectionCondition : ICondition
{
    public bool Test() => SelectionController.Instance && SelectionController.Instance.HasSelection;
}
