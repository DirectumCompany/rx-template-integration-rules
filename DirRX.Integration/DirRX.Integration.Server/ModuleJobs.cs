using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.Integration.Server
{
  public partial class ModuleJobs
  {

    /// <summary>
    /// Фоновый процесс. Выполняет обмен данными между внешней системой и Directum RX.
    /// </summary>
    public virtual void ExecuteIntegration()
    {
      var logs = new List<DirRX.Integration.Structures.Module.LogStruct>();
      var integrationSettings = Functions.Module.GetIntegrationSettings();
      if (!integrationSettings.Any())
      {
        Logger.Debug("ExecuteIntegration. Отсутствуют действующие настройки интеграции.");
        return;
      }
      
      foreach (var settingsItem in integrationSettings)
      {
        Logger.Debug($"ExecuteIntegration. Настройка {settingsItem}({settingsItem.Id}). Старт");
        if (settingsItem != null)
        {
          foreach (var ruleItem in settingsItem.Rules.OrderBy(x => x.ExecutionOrder))
            DirRX.Integration.Functions.IntegrationRuleBase.ExecuteIntegration(ruleItem.Rule, settingsItem, logs);
        }
        else
        {
          var message = Resources.ErrorCheckIntegrationSettings;
          logs.Add(Functions.Module.CreateLogItem(string.Empty,
                                                  Constants.Module.Logging.MessageLevel.JobLevel,
                                                  Constants.Module.Logging.MessageType.Error,
                                                  message));
        }
        
        Functions.Module.SendResults(Functions.IntegrationSetting.GetResponsibles(settingsItem), logs);
        Logger.Debug($"ExecuteIntegration. Настройка {settingsItem}({settingsItem.Id}). Завершение");
      }
    }

  }
}