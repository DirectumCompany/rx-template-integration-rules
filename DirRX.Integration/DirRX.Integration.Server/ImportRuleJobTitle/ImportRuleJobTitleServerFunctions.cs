using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.Integration.ImportRuleJobTitle;
using Keys = DirRX.Integration.Constants.ImportRuleBase.Keys;

namespace DirRX.Integration.Server
{
  partial class ImportRuleJobTitleFunctions
  {
    
    #region Импорт Должностей из внешней системы.
    
    /// <summary>
    /// Выполнить сохранение полученных данных в RX.
    /// </summary>
    /// <param name="response">Матрица с ответом от внешней системы.</param>
    /// <param name="logs">Структурированный лог.</param>
    public override void SaveData(List<System.Collections.Generic.Dictionary<string, string>> response, List<DirRX.Integration.Structures.Module.LogStruct> logs)
    {
      var jobTitles = new List<DirRX.Integration.Structures.ImportRuleJobTitle.JobTitle>();
      
      foreach (var responseItem in response)
        jobTitles.Add(ParseResponseItem(responseItem));
      
      if (jobTitles.Any())
        ImportJobTitles(jobTitles, logs);
    }
    
    /// <summary>
    /// Обработка строки матрицы с ответом от внешней системы.
    /// </summary>
    /// <param name="responseItem">Строка матрицы с ответом от внешней системы.</param>
    /// <returns>Свойства сущности в структурированном виде.</returns>
    public virtual DirRX.Integration.Structures.ImportRuleJobTitle.JobTitle ParseResponseItem(System.Collections.Generic.Dictionary<string, string> responseItem)
    {
      var jobTitle = new DirRX.Integration.Structures.ImportRuleJobTitle.JobTitle();
      jobTitle.ExternalId = responseItem[Keys.Key0];
      jobTitle.Name = responseItem[Keys.Key1];
      return jobTitle;
    }
    
    /// <summary>
    /// Процедура импорта Должностей.
    /// </summary>
    /// <param name="items">Структурированный набор данных по импортируемым Должностям.</param>
    /// <returns>Список структурированных логов.</returns>
    [Remote]
    public void ImportJobTitles(List<DirRX.Integration.Structures.ImportRuleJobTitle.JobTitle> items, List<DirRX.Integration.Structures.Module.LogStruct> logs)
    {
      foreach (var item in items)
      {
        var jobTitleExternalLink = Sungero.Domain.ModuleFunctions.GetAllExternalLinks(l => l.EntityTypeGuid == DirRX.Integration.Constants.ImportRuleJobTitle.JobTitleDatabookGuid &&
                                                                                      l.ExternalEntityId == item.ExternalId && l.IsDeleted == false).FirstOrDefault();
        var jobTitleId = jobTitleExternalLink != null ? jobTitleExternalLink.EntityId : 0;
        var jobTitle = Sungero.Company.JobTitles.GetAll().Where(x => x.Id == jobTitleId).FirstOrDefault();
        var isNew = false;
        if (jobTitle == null)
        {
          jobTitle = Sungero.Company.JobTitles.Create();
          isNew = true;
        }
        
        if (!string.Equals(item.Name, jobTitle.Name))
          jobTitle.Name = item.Name;
        
        try
        {
          if (jobTitle.State.IsChanged)
          {
            jobTitle.Save();
            if (isNew)
              DirRX.Integration.PublicFunctions.IntegrationRuleBase.CreateExternalLink(jobTitle, DirRX.Integration.Constants.ImportRuleJobTitle.JobTitleDatabookGuid, item.ExternalId);
            Logger.Debug(string.Format("Обновление/создание карточки Должности {0}.", jobTitle.Name));
          }
        }
        catch (Exception ex)
        {
          var errorMessage = string.Format("Ошибка при обновлении карточки Должности {0}. Подробности: {1}", jobTitle.Name, ex.Message);
          Logger.Error(errorMessage, ex);
          logs.Add(DirRX.Integration.Functions.Module.CreateLogItem(_obj.Name,
                                                                       DirRX.Integration.Constants.Module.Logging.MessageLevel.ResponseLevel,
                                                                       DirRX.Integration.Constants.Module.Logging.MessageType.Error,
                                                                       errorMessage));
        }
      }
    }
    #endregion
  }
}