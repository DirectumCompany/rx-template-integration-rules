using System;
using Sungero.Core;

namespace DirRX.Integration.Constants
{
  public static class IntegrationRuleBase
  {
    public const string ExternalLinkSystem = "ExternalSystem";
    
    /// <summary>
    /// Размер пакета для обработки в транзакции.
    /// </summary>
    public const int TransactionPackageSize = 100;
  }
}