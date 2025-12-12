using System;
using Sungero.Core;

namespace DirRX.Integration.Constants
{
  public static class Module
  {
    /// <summary>
    /// Константы для формирования лога.
    /// </summary>
    public static class Logging
    {
      public static class MessageLevel
      {
        public const string JobLevel = "JobLevel";
        public const string RequestLevel = "RequestLevel";
        public const string ResponseLevel = "ResponseLevel";
      }
      public static class MessageType
      {
        public const string Warn = "Warning";
        public const string Error = "Error";
        public const string Info = "Information";
      }
    }
  }
}