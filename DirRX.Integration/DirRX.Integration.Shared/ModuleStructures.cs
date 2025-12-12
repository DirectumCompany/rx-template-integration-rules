using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.Integration.Structures.Module
{
  /// <summary>
  /// Структурированние хранения результатов выполнения правил интеграции.
  /// </summary>
  partial class LogStruct
  {
    public string ImportRule {get; set;}
    public string MessageLevel {get; set;}
    public string MessageType {get; set;}
    public string Message {get; set;}
  }

}