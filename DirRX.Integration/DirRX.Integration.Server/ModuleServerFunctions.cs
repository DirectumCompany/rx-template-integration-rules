using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Text;

namespace DirRX.Integration.Server
{
  public partial class ModuleFunctions
  {
    /// <summary>
    /// Получить настройки интеграции для выполнения.
    /// </summary>
    public virtual IQueryable<IIntegrationSetting> GetIntegrationSettings()
    {
      return IntegrationSettings.GetAll(s => s.Status == Sungero.CoreEntities.DatabookEntry.Status.Active);
    }
    
    #region Интеграция. Вспомогательные методы.
    
    /// <summary>
    /// Закрыть ссылку на внешний объект.
    /// </summary>
    /// <param name="entityTypeGuid">GUID типа сущности.</param>
    /// <param name="entityId">ИД закрываемого объекта.</param>
    [Public, Remote]
    public static void CloseExternalLink(Guid entityTypeGuid, int entityId)
    {
      var externalLink = Sungero.Domain.ModuleFunctions.GetAllExternalLinks(l => l.EntityTypeGuid == entityTypeGuid &&
                                                                            l.EntityId == entityId).FirstOrDefault();
      if (externalLink != null)
      {
        externalLink.IsDeleted = true;
        externalLink.Save();
      }
    }
    
    /// <summary>
    /// Процедура создания элемента лог-файла.
    /// </summary>
    /// <param name="integrationRule">Тип правила интеграции.</param>
    /// <param name="messageLevel">Уровень, на котором возникает ошибка.</param>
    /// <param name="messageType">Тип ошибки.</param>
    /// <param name="message">Сообщение.</param>
    /// <returns>Структурированный элемент лога.</returns>
    public virtual DirRX.Integration.Structures.Module.LogStruct CreateLogItem(string integrationRule, string messageLevel, string messageType, string message)
    {
      return DirRX.Integration.Structures.Module.LogStruct.Create(integrationRule, messageLevel, messageType, message);
    }
    
    /// <summary>
    /// Подготовка результатов и отправка ответственным лицам.
    /// </summary>
    /// <param name="logs">Список логируемых событий.</param>
    public virtual void SendResults(Sungero.CoreEntities.IRecipient recipient, List<DirRX.Integration.Structures.Module.LogStruct> logs)
    {
      var result = new StringBuilder();
      
      // TODO добавить в текстовку какой блок ошибок к чему относится например "Логи фонового процесса / отправки запроса / обработки ответа".
      result.AppendLine(CreateReport(DirRX.Integration.Constants.Module.Logging.MessageLevel.JobLevel,
                                     logs));
      
      result.AppendLine(CreateReport(DirRX.Integration.Constants.Module.Logging.MessageLevel.RequestLevel,
                                     logs));
      
      result.AppendLine(CreateReport(DirRX.Integration.Constants.Module.Logging.MessageLevel.ResponseLevel,
                                     logs));
      
      var isErrors = logs.Any(x => x.MessageType == Constants.Module.Logging.MessageType.Error);
      
      var subject = isErrors ? Resources.LogSubjectFormat(Resources.ExchangeErrors, Calendar.Now.ToString("g")) :
        Resources.InformationTaskSubjectFormat(Resources.ExchangeSuccess, Calendar.Now.ToString("g"));
      
      var isSendSuccess = SendSyncResultByNotice(subject, result.ToString(), recipient);
      
      if (!isSendSuccess)
        Logger.Debug(result.ToString());
    }
    
    /// <summary>
    /// Подготовка отчета по ошибкам.
    /// </summary>
    /// <param name="messageLevel">Уровень, где фиксируется ошибка.</param>
    /// <param name="entityCode">Код логируемой сущности.</param>
    /// <param name="logs">Список логируемых событий.</param>
    /// <returns>Строка с результатами.</returns>
    public virtual string CreateReport(string messageLevel, List<DirRX.Integration.Structures.Module.LogStruct> logs)
    {
      var result = new StringBuilder();
      
      result.AppendLine(CreateReportBlock(messageLevel, logs));
      result.AppendLine("------------------------------------");
      
      return result.ToString();
    }
    
    /// <summary>
    /// Подготовка блока отчета.
    /// </summary>
    /// <param name="logs">Список логируемых событий.</param>
    /// <param name="messageLevel">Уровень, где фиксируется ошибка.</param>
    /// <returns>Результат выполнения операции.</returns>
    public virtual string CreateReportBlock(string messageLevel, List<DirRX.Integration.Structures.Module.LogStruct> logs)
    {
      var logsAll = logs.Where(x => !string.IsNullOrEmpty(messageLevel) && x.MessageLevel == messageLevel);
      var errors = logsAll.Where(x => x.MessageType == Constants.Module.Logging.MessageType.Error);
      var warnings = logsAll.Where(x => x.MessageType == Constants.Module.Logging.MessageType.Warn);
      var infos = logsAll.Where(x => x.MessageType == Constants.Module.Logging.MessageType.Info);
      
      var result = new StringBuilder();
      
      result.AppendLine(Resources.LogSubjectFormat(errors.Count(), warnings.Count(), infos.Count()));
      if (errors.Count() == 0 && warnings.Count() == 0 && infos.Count() == 0)
        result.AppendLine(Resources.ExchangeSuccess);
      else
      {
        result.AppendLine(Resources.LogErrors);
        foreach (var item in errors)
          result.AppendLine(Resources.LogLineFormat(item.ImportRule, item.Message));
        
        result.AppendLine(Resources.LogWarnings);
        foreach (var item in warnings)
          result.AppendLine(Resources.LogLineFormat(item.ImportRule, item.Message));
        
        result.AppendLine(Resources.LogInfos);
        foreach (var item in infos)
          result.AppendLine(Resources.LogLineFormat(item.ImportRule, item.Message));
      }
      return result.ToString();
    }
    
    /// <summary>
    /// Оповестить участников специализированной роли об итогах процедуры импорта.
    /// </summary>
    /// <param name="subject">Заголовок.</param>
    /// <param name="text">Текст сообщения.</param>
    /// <param name="recipient">Ответственный.</param>
    /// <returns>Результат отправки уведомления.</returns>
    /// <remarks>Добавлена возможность перекрытия, в случае необходимости изменить способ отправки отчета.</remarks>
    [Remote, Public]
    public virtual bool SendSyncResultByNotice(string subject, string text, Sungero.CoreEntities.IRecipient recipient)
    {
      try
      {
        var performers = new List<Sungero.CoreEntities.IRecipient>();
        performers.Add(recipient);
        var newTask = Sungero.Workflow.SimpleTasks.CreateWithNotices(subject, performers.ToArray());
        newTask.ActiveText = text;
        newTask.Start();
      }
      catch (Exception ex)
      {
        Logger.Error(ex.Message, ex);
        return false;
      }
      return true;
    }
    #endregion
  }
}