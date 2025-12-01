using System;
namespace MinimalApiSample.Exceptions
{
  public class AppException : Exception
  {
    public string messageId { get; set; }
  }

  public class InvalidParamAppException : AppException
  {
    public string invalidParam { get; set; }
  }
}