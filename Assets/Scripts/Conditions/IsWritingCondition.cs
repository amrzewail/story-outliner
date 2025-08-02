public class IsWritingCondition : ICondition
{
    public bool testOpposite;
    public bool Test()
    {
        try 
        {
            return Controller.Instance.IsWriting != testOpposite;
        }
        catch (System.NullReferenceException ex)
        {
            return false != testOpposite; //not writing
        }
    }
}