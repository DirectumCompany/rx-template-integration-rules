using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.Integration.ImportRuleDepartment;
using Keys = DirRX.Integration.Constants.ImportRuleBase.Keys;

namespace DirRX.Integration.Server
{
  partial class ImportRuleDepartmentFunctions
  {
    #region Импорт Подразделений из внешней системы.
    
    /// <summary>
    /// Выполнить сохранение полученных данных в RX.
    /// </summary>
    /// <param name="response">Матрица с ответом от внешней системы.</param>
    /// <param name="logs">Структурированный лог.</param>
    public override void SaveData(List<System.Collections.Generic.Dictionary<string, string>> response, List<DirRX.Integration.Structures.Module.LogStruct> logs)
    {
      var departments = new List<DirRX.Integration.Structures.ImportRuleDepartment.Department>();
      
      foreach (var responseItem in response)
        departments.Add(ParseResponseItem(responseItem));
      
      if (departments.Any())
      {
        ImportDepartments(departments, logs);
        UpdateDepartamentsHeadOffice(departments, logs);
      }
    }
    
    /// <summary>
    /// Обработка строки матрицы с ответом от внешней системы.
    /// </summary>
    /// <param name="responseItem">Строка матрицы с ответом от внешней системы.</param>
    /// <returns>Свойства сущности в структурированном виде.</returns>
    public virtual DirRX.Integration.Structures.ImportRuleDepartment.Department ParseResponseItem(System.Collections.Generic.Dictionary<string, string> responseItem)
    {
      var department = new DirRX.Integration.Structures.ImportRuleDepartment.Department();
      department.ExternalId = responseItem[Keys.Key0];
      department.Name = responseItem[Keys.Key1];
      department.ManagerExternalId = responseItem[Keys.Key2];
      department.HeadOfficeExternalId = responseItem[Keys.Key3];
      department.BusinessUnitCode = responseItem[Keys.Key4];
      department.Status = responseItem[Keys.Key5] == DirRX.Integration.Constants.ImportRuleBase.Active ? Sungero.Company.Department.Status.Active : Sungero.Company.Department.Status.Closed;
      return department;
    }
    
    /// <summary>
    /// Процедура обновления головного подразделения для подразделений.
    /// </summary>
    /// <param name="departments">Структурированный набор данных по импортируемым подразделениям.</param>
    /// <param name="logs">Список структурированных логов.</param>
    [Remote]
    public virtual void UpdateDepartamentsHeadOffice(List<DirRX.Integration.Structures.ImportRuleDepartment.Department> departments, List<DirRX.Integration.Structures.Module.LogStruct> logs)
    {
      foreach (var department in departments)
      {
        var externalLink = Sungero.Domain.ModuleFunctions.GetAllExternalLinks(l => l.EntityTypeGuid == DirRX.Integration.Constants.ImportRuleDepartment.DepartmentDatabookGuid &&
                                                                              l.ExternalEntityId == department.ExternalId & l.IsDeleted == false).FirstOrDefault();
        var departmentId = externalLink != null ? externalLink.EntityId : 0;
        var rxDepartment = Sungero.Company.Departments.GetAll(x => x.Id == departmentId).FirstOrDefault();
        if (rxDepartment != null)
        {
          var headOfficeExternalLink = Sungero.Domain.ModuleFunctions.GetAllExternalLinks(l => l.EntityTypeGuid == DirRX.Integration.Constants.ImportRuleDepartment.DepartmentDatabookGuid &&
                                                                                          l.ExternalEntityId == department.HeadOfficeExternalId && l.IsDeleted == false).FirstOrDefault();
          var headOfficeId = headOfficeExternalLink != null ? headOfficeExternalLink.EntityId : 0;
          var headOffice = Sungero.Company.Departments.GetAll().Where(d => d.Id == headOfficeId).FirstOrDefault();
          if (headOffice != null && !Sungero.Company.Departments.Equals(rxDepartment.HeadOffice, headOffice))
            rxDepartment.HeadOffice = headOffice;
          try
          {
            if (rxDepartment.State.IsChanged)
            {
              rxDepartment.Save();
              Logger.Debug(string.Format("Обновление свойства HeadOffice карточки Подразделения {0}", rxDepartment.Name));
            }
          }
          catch (Exception ex)
          {
            var errorMessage = string.Format("Ошибка при обновлении HeadOffice карточки Подразделения {0}. Подробности: {1}.", rxDepartment.Name, ex.Message);
            Logger.Error(errorMessage, ex);
            logs.Add(DirRX.Integration.Functions.Module.CreateLogItem(_obj.Name,
                                                                      DirRX.Integration.Constants.Module.Logging.MessageLevel.ResponseLevel,
                                                                      DirRX.Integration.Constants.Module.Logging.MessageType.Error,
                                                                      errorMessage));
          }
        }
      }
    }
    
    /// <summary>
    /// Процедура импорта Подразделений.
    /// </summary>
    /// <param name="items">Структурированный набор данных по импортируемым Подразделений.</param>
    /// <param name="logs">Список структурированных логов.</param>
    [Remote]
    public virtual void ImportDepartments(List<DirRX.Integration.Structures.ImportRuleDepartment.Department> items, List<DirRX.Integration.Structures.Module.LogStruct> logs)
    {
      foreach (var item in items)
      {
        var externalLink = Sungero.Domain.ModuleFunctions.GetAllExternalLinks(l => l.EntityTypeGuid == DirRX.Integration.Constants.ImportRuleDepartment.DepartmentDatabookGuid &&
                                                                              l.ExternalEntityId == item.ExternalId && l.IsDeleted == false).FirstOrDefault();
        var departmentId = externalLink != null ? externalLink.EntityId : 0;
        var department = Sungero.Company.Departments.GetAll().Where(x => x.Id == departmentId).FirstOrDefault();
        var isNew = false;
        if (department == null)
        {
          department = Sungero.Company.Departments.Create();
          isNew = true;
        }
        
        if (department.Name != item.Name)
          department.Name = item.Name;
        
        if (!string.IsNullOrEmpty(item.BusinessUnitCode))
        {
          var businessUnit = Sungero.Company.BusinessUnits.GetAll().Where(x => x.Code == item.BusinessUnitCode).FirstOrDefault();
          if (businessUnit != null && department.BusinessUnit != businessUnit)
            department.BusinessUnit = businessUnit;
        }
        
        var employeeExternalLink = Sungero.Domain.ModuleFunctions.GetAllExternalLinks(l => l.EntityTypeGuid == DirRX.Integration.PublicConstants.ImportRuleEmployee.EmployeeDatabookGuid &&
                                                                                      l.ExternalEntityId == item.ManagerExternalId && l.IsDeleted == false).FirstOrDefault();
        var employeeId = employeeExternalLink != null ? employeeExternalLink.EntityId : 0;
        var manager = Sungero.Company.Employees.GetAll().Where(x => x.Id == employeeId).FirstOrDefault();
        if (manager != null && department.Manager != manager)
          department.Manager = manager;
        
        if (department.Status != item.Status)
          department.Status = item.Status;
        
        try
        {
          if (department.State.IsChanged || department.State.IsInserted)
          {
            department.Save();
            if (isNew)
              DirRX.Integration.PublicFunctions.IntegrationRuleBase.CreateExternalLink(department, DirRX.Integration.Constants.ImportRuleDepartment.DepartmentDatabookGuid, item.ExternalId);
            Logger.Debug(string.Format("Обновление/создание карточки Подразделения {0}.", department.Name));
          }
        }
        catch (Exception ex)
        {
          var errorMessage = string.Format("Ошибка при обновлении карточки Подразделения {0}. Подробности: {1}", department.Name, ex.Message);
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