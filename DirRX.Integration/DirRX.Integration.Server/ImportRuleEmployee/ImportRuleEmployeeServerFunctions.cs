using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.Integration.ImportRuleEmployee;
using Keys = DirRX.Integration.Constants.ImportRuleBase.Keys;

namespace DirRX.Integration.Server
{
  partial class ImportRuleEmployeeFunctions
  {
    
    #region Импорт Сотрудников из внешней системы.
    
    /// <summary>
    /// Выполнить сохранение полученных данных в RX.
    /// </summary>
    /// <param name="response">Матрица с ответом от внешней системы.</param>
    /// <param name="logs">Структурированный лог.</param>
    public override void SaveData(List<System.Collections.Generic.Dictionary<string, string>> response, List<DirRX.Integration.Structures.Module.LogStruct> logs)
    {
      var employees = new List<DirRX.Integration.Structures.ImportRuleEmployee.Employee>();
      
      foreach (var responseItem in response)
        employees.Add(ParseResponseItem(responseItem));
      
      if (employees.Any())
      {
        ImportEmployees(employees, logs);
      }
    }
    
    /// <summary>
    /// Обработка строки матрицы с ответом от внешней системы.
    /// </summary>
    /// <param name="responseItem">Строка матрицы с ответом от внешней системы.</param>
    /// <returns>Свойства сущности в структурированном виде.</returns>
    public virtual DirRX.Integration.Structures.ImportRuleEmployee.Employee ParseResponseItem(System.Collections.Generic.Dictionary<string, string> responseItem)
    {
      var employee = new DirRX.Integration.Structures.ImportRuleEmployee.Employee();
      employee.ExternalId = responseItem[Keys.Key0];
      employee.LastName = responseItem[Keys.Key1];
      employee.FirstName = responseItem[Keys.Key2];
      employee.MiddleName = responseItem[Keys.Key3];
      employee.LoginName = responseItem[Keys.Key4];
      employee.DepartmentExternalId = responseItem[Keys.Key5];
      employee.JobTitleExternalId = responseItem[Keys.Key6];
      employee.Email = responseItem[Keys.Key7];
      employee.Status = responseItem[Keys.Key8] == DirRX.Integration.Constants.ImportRuleBase.Active ? Sungero.Company.Employee.Status.Active : Sungero.Company.Employee.Status.Closed;
      return employee;
    }
    
    /// <summary>
    /// Процедура импорта Персон.
    /// </summary>
    /// <param name="employee">Данные по сотруднику в виде структуры.</param>
    /// <param name="logs">Фиксация логов.</param>
    /// <returns>Персона</returns>
    public virtual Sungero.Parties.IPerson ImportPerson(DirRX.Integration.Structures.ImportRuleEmployee.Employee employee, List<DirRX.Integration.Structures.Module.LogStruct> logs)
    {
      var employeeExternalLink = Sungero.Domain.ModuleFunctions.GetAllExternalLinks(l => l.EntityTypeGuid == DirRX.Integration.Constants.ImportRuleEmployee.EmpliyeeDatabookGuid &&
                                                                                    l.ExternalEntityId == employee.ExternalId && l.IsDeleted == false).FirstOrDefault();
      var employeeId = employeeExternalLink != null ? employeeExternalLink.EntityId : 0;
      var person = Sungero.Company.Employees.GetAll().Where(x => x.Id == employeeId).Select(x => x.Person).FirstOrDefault();
      if (person == null)
        person = Sungero.Parties.People.Create();
      
      if (!string.Equals(person.LastName, employee.LastName))
        person.LastName = employee.LastName;
      
      if (!string.Equals(person.FirstName, employee.FirstName))
        person.FirstName = employee.FirstName;
      
      if (!string.Equals(person.MiddleName, employee.MiddleName))
        person.MiddleName = employee.MiddleName;
      
      if (person.Email != employee.Email)
        person.Email = employee.Email;
      
      if (person.Status != employee.Status)
        person.Status = employee.Status;
      
      try
      {
        if (person.State.IsChanged)
        {
          person.Save();
          Logger.Debug(string.Format("Обновление/создание карточки персоны {0} {1} {2}.", person.LastName, person.FirstName, person.MiddleName));
        }
      }
      catch (Exception ex)
      {
        var errorMessage = string.Format("Ошибка при обновлении карточки персоны {0} {1} {2}. Подробности: {3}", person.LastName, person.FirstName, person.MiddleName, ex.Message);
        Logger.Error(errorMessage, ex);
        logs.Add(DirRX.Integration.Functions.Module.CreateLogItem(_obj.Name,
                                                                  DirRX.Integration.Constants.Module.Logging.MessageLevel.ResponseLevel,
                                                                  DirRX.Integration.Constants.Module.Logging.MessageType.Error,
                                                                  errorMessage));
      }
      return person;
    }
    
    /// <summary>
    /// Процедура импорта Учетных записей.
    /// </summary>
    /// <param name="employee">Данные по сотруднику в виде структуры.</param>
    /// <param name="logs">Фиксация логов.</param>
    /// <returns>Логин.</returns>
    /// <remarks>Тип аутентификации учетной записи меняется на "Внешняя аутентификация".</remarks>
    public virtual Sungero.CoreEntities.ILogin ImportLogin(DirRX.Integration.Structures.ImportRuleEmployee.Employee employee, List<DirRX.Integration.Structures.Module.LogStruct> logs)
    {
      var login = Sungero.CoreEntities.Logins.GetAll().Where(l => l.LoginName == employee.LoginName).FirstOrDefault();
      if (login == null)
      {
        login = Sungero.CoreEntities.Logins.Create();
        login.LoginName = employee.LoginName;
      }
      
      if (login.TypeAuthentication != Sungero.CoreEntities.Login.TypeAuthentication.Windows)
        login.TypeAuthentication = Sungero.CoreEntities.Login.TypeAuthentication.Windows;

      try
      {
        if (login.State.IsChanged)
        {
          login.Save();
          Logger.Debug(string.Format("Обновление/создание карточки учетной записи {0}.", login.LoginName));
        }
      }
      catch (Exception ex)
      {
        var errorMessage = string.Format("Ошибка при обновлении карточки учетной записи {0}. Подробности: {1}", login.LoginName, ex.Message);
        Logger.Error(errorMessage, ex);
        logs.Add(DirRX.Integration.Functions.Module.CreateLogItem(_obj.Name,
                                                                  DirRX.Integration.Constants.Module.Logging.MessageLevel.ResponseLevel,
                                                                  DirRX.Integration.Constants.Module.Logging.MessageType.Error,
                                                                  errorMessage));
      }
      return login;
    }
    
    /// <summary>
    /// Процедура импорта Сотрудников.
    /// </summary>
    /// <param name="items">Структурированный набор данных по импортируемым Сотрудникам.</param>
    /// <param name="logs">Фиксация логов.</param>
    [Remote]
    public virtual void ImportEmployees(List<DirRX.Integration.Structures.ImportRuleEmployee.Employee> items, List<DirRX.Integration.Structures.Module.LogStruct> logs)
    {
      foreach (var item in items)
      {
        var employeeExternalLink = Sungero.Domain.ModuleFunctions.GetAllExternalLinks(l => l.EntityTypeGuid == DirRX.Integration.Constants.ImportRuleEmployee.EmpliyeeDatabookGuid &&
                                                                                      l.ExternalEntityId == item.ExternalId && l.IsDeleted == false).FirstOrDefault();
        var employeeId = employeeExternalLink != null ? employeeExternalLink.EntityId : 0;
        var employee = Sungero.Company.Employees.GetAll().Where(x => x.Id == employeeId).FirstOrDefault();
        var isNew = false;
        if (employee == null)
        {
          employee = Sungero.Company.Employees.Create();
          isNew = true;
        }
        
        var person = ImportPerson(item, logs);
        if (!Sungero.Parties.People.Equals(employee.Person, person))
          employee.Person = person;
        
        if (!string.IsNullOrWhiteSpace(item.LoginName))
        {
          var login = ImportLogin(item, logs);
          if (!Sungero.CoreEntities.Logins.Equals(employee.Login, login))
            employee.Login = login;
        }
        
        var departmentExternalLink = Sungero.Domain.ModuleFunctions.GetAllExternalLinks(l => l.EntityTypeGuid == DirRX.Integration.Constants.ImportRuleDepartment.DepartmentDatabookGuid &&
                                                                                        l.ExternalEntityId == item.DepartmentExternalId && l.IsDeleted == false).FirstOrDefault();
        var departmentId = departmentExternalLink != null ? departmentExternalLink.EntityId : 0;
        var department = Sungero.Company.Departments.GetAll().Where(d => d.Id == departmentId).FirstOrDefault();
        if (department != null && employee.Department != department)
          employee.Department = department;
        
        var jobTitleExternalLink = Sungero.Domain.ModuleFunctions.GetAllExternalLinks(l => l.EntityTypeGuid == DirRX.Integration.Constants.ImportRuleJobTitle.JobTitleDatabookGuid &&
                                                                                      l.ExternalEntityId == item.JobTitleExternalId && l.IsDeleted == false).FirstOrDefault();
        var jobTitleId = jobTitleExternalLink != null ? jobTitleExternalLink.EntityId : 0;
        var jobTitle = Sungero.Company.JobTitles.GetAll().Where(j => j.Id == jobTitleId).FirstOrDefault();
        if (jobTitle != null && employee.JobTitle != jobTitle)
          employee.JobTitle = jobTitle;
        
        if (employee.Status != item.Status)
          employee.Status = item.Status;
        
        if (!string.Equals(employee.Email, item.Email))
          employee.Email = item.Email;
        
        if (!string.IsNullOrWhiteSpace(employee.Email))
        {
          if (employee.NeedNotifyExpiredAssignments == null)
            employee.NeedNotifyExpiredAssignments = true;
          
          if (employee.NeedNotifyNewAssignments == null)
            employee.NeedNotifyNewAssignments = true;
        }
        else
        {
          if (employee.NeedNotifyExpiredAssignments != false)
            employee.NeedNotifyExpiredAssignments = false;
          if (employee.NeedNotifyNewAssignments != false)
            employee.NeedNotifyNewAssignments = false;
        }
        
        try
        {
          if (employee.State.IsChanged)
          {
            employee.Save();
            if (isNew)
              DirRX.Integration.PublicFunctions.IntegrationRuleBase.CreateExternalLink(employee, DirRX.Integration.Constants.ImportRuleEmployee.EmpliyeeDatabookGuid, item.ExternalId);
            Logger.Debug(string.Format("Обновление/создание карточки Сотрудников {0}.", employee.Name));
          }
        }
        catch (Exception ex)
        {
          var errorMessage = string.Format("Ошибка при обновлении карточки Сотрудников {0}. Подробности: {1}", employee.Name, ex.Message);
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