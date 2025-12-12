using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.Integration.ImportRuleBase;

namespace DirRX.Integration.Server
{
  partial class ImportRuleBaseFunctions
  {
    /// <summary>
    /// Обработать ответ от внешней системы.
    /// </summary>
    /// <param name="response">Матрица с ответом от внешней системы.</param>
    /// <param name="logs">Структурированный лог.</param>
    public override void ProcessResponse(List<System.Collections.Generic.Dictionary<string, string>> response, List<DirRX.Integration.Structures.Module.LogStruct> logs)
    {
      SaveData(response, logs);
    }
    
    /// <summary>
    /// Выполнить сохранение полученных данных в RX.
    /// </summary>
    /// <param name="response">Матрица с ответом от внешней системы.</param>
    /// <param name="logs">Структурированный лог.</param>
    public virtual void SaveData(List<System.Collections.Generic.Dictionary<string, string>> response, List<DirRX.Integration.Structures.Module.LogStruct> logs)
    {
      throw new NotImplementedException();
    }
    
  }
}