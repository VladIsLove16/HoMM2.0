public class OperationResult
{
    public string Message;
    public bool IsSuccess;
    public OperationResult()
    {

    }

    public OperationResult(bool IsSuccess, string Message = "")
    {
        this.IsSuccess = IsSuccess;
        this.Message = Message;
    }
}